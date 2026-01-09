using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    public class Shape : PersistableObject
    {
        // Shader 中属性名 "_Color" 对应的整数 ID。
        // 使用 PropertyToID 可以避免运行时字符串查找，提高性能。
        static int colorPropertyID = Shader.PropertyToID("_Color");

        // 全局复用的 MaterialPropertyBlock。
        // MaterialPropertyBlock 用于对 Renderer 设置“临时的材质属性”，不会创建材质实例。
        // 这里用 static 复用同一个 block，避免频繁分配和 GC。
        // 注意：Unity 引擎对象不能在 static 上直接初始化，因此初次使用时再创建。
        static MaterialPropertyBlock sharedPropertyBlock;

        public int MaterialID { get;private set; }      // 记录当前shape所使用的material
        //public Vector3 AngularVelocity { get; set; }    // 角速度
        //public Vector3 Velocity { get; set; }           // 移动速度
        public int ShapeID
        {
            get => shapeID;
            set
            {
                if (shapeID == Int32.MinValue && value != Int32.MinValue)
                {
                    shapeID = value;
                }
                else
                {
                    Debug.LogError("形状已经被设置过，不允许修改");
                }
            }
        }                         // 记录当前shape的形状
        public int ColorCount => colors.Length;                 // 一个shape可能由多个物体组成，记录他们的颜色总数

        public ShapeFactory OriginFactory
        {
            get => originFactory;
            set
            {
                if (originFactory == null)
                {
                    originFactory = value;
                }
                else
                {
                    Debug.Log("来源工厂已经被记录过，无法修改");
                }
            }
        }
        public float Age{ get; private set;}                    // shape从被启用到现在过了多久，秒为单位
        /// <summary>
        /// 当前shape在shape列表中的唯一索引
        /// </summary>
        public int SaveIndex { get; set; }
        public int InstanceId { get; private set; }             // 用来记录形状的唯一id

        /// <summary>
        /// 指示该形状是否已被标记为“即将消亡”
        /// </summary>
        public bool IsMarkedAsDying => Game.Instance.IsMarkedDying(this);
        [SerializeField] private MeshRenderer[] meshRenderers;  // 每个物体的meshRender
        private ShapeFactory originFactory;
        private int shapeID = Int32.MinValue;                   // 记录形状种类
        private Color[] colors;                                 // 同一个shape的不同物体可能有不同的颜色
        private List<ShapeBehavior> behaviorList = new();       // 存储当前shape所有的行为
        private void Awake()
        {
            colors = new Color[meshRenderers.Length];
        }


        /// <summary>
        /// 这个GameUpdate控制了所有Shape的更新逻辑
        /// </summary>
        public void GameUpdate()
        {
            Age += Time.deltaTime;
            for (int i = 0; i < behaviorList.Count; i++)
            {
                if (!behaviorList[i].GameUpdate(this))
                {
                    behaviorList[i].Recycle();
                    behaviorList.RemoveAt(i--);
                }
            }
        }

        #region 设置材质与颜色

        public void SetMaterial(Material material, int materialID)
        {
            // 设置材质并记录材质对应的编号
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = material;
            }
            MaterialID = materialID;
        }
        
        public void SetColor(Color color)
        {
            // 直接对材质的颜色进行设置，会导致 Unity 创建一个新的材质实例，专属于当前物体。每次设置颜色都会创建一次新材质，这并不好。
            // 我们可以通过使用 MaterialPropertyBlock 来避免这一点。
            // meshRenderer.material.color = color;
            if (sharedPropertyBlock == null)
            {
                sharedPropertyBlock = new MaterialPropertyBlock();
            }
            // 将颜色写入共享的 MaterialPropertyBlock。
            // colorPropertyID 是 Shader 中 "_Color" 属性对应的整数 ID。
            // color 是当前 Shape 要显示的颜色。
            // 这一操作只是修改 block 的内容，并不会立即影响材质或其他 Renderer。
            sharedPropertyBlock.SetColor(colorPropertyID, color);

            // 将共享的 MaterialPropertyBlock 应用到当前 MeshRenderer。
            // Unity 会复制 block 的内容到 Renderer 内部，使这个 Renderer 显示指定的颜色。
            // 这样做不会创建新的材质实例，也不会修改材质本身。
            // 不同 Renderer 可以使用同一个 block 设置各自的颜色，因为 SetPropertyBlock 会复制内容。
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                colors[i] = color;
                meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
            }

        }

        /// <summary>
        /// 将颜色设置到对应索引的材质中并存放到colors中
        /// </summary>
        /// <param name="color">需要设置的颜色</param>
        /// <param name="index">需要设置的材质索引</param>
        public void SetColor(Color color, int index)
        {
            if(sharedPropertyBlock == null) sharedPropertyBlock = new MaterialPropertyBlock();
            sharedPropertyBlock.SetColor(colorPropertyID, color);
            colors[index] = color;
            meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
        }

        #endregion

        #region 存档与读档

        public override void Save(GameDataWriter writer)
        {
            base.Save(writer);
            writer.Write(colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                writer.Write(colors[i]);
                //Debug.Log($"{colors[i]}");
            }
            // writer.Write(AngularVelocity);
            // writer.Write(Velocity);
            writer.Write(Age);
            writer.Write(behaviorList.Count);
            //Debug.Log($"behaviorList.Count::{behaviorList.Count}");
            foreach (var behavior in behaviorList)
            {
                //Debug.Log($"BehaviorType::{(int)behavior.BehaviorType}");
                writer.Write((int)behavior.BehaviorType);
                behavior.Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            base.Load(reader);
            if (reader.Version >= 5)
            {
                LoadColors(reader);
            }
            else
            {
                SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
            }
            // AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
            // Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
            if (reader.Version >= 6)
            {
                Age = reader.ReadFloat();
                int behaviorCount = reader.ReadInt();
                //Debug.Log($"behaviorCount::{behaviorCount}");
                for (int i = 0; i < behaviorCount; i++)
                {
                    var behavior = ((ShapeBehaviorType)reader.ReadInt()).GetInstance();
                    //Debug.Log($"type::{type}");
                    behaviorList.Add(behavior);
                    behavior.Load(reader);
                }
            }
            else if (reader.Version >= 4)
            {
                AddBehavior<RotationShapeBehavior>().AngularVelocity = reader.ReadVector3();
                AddBehavior<MovementShapeBehavior>().Velocity = reader.ReadVector3();
            }
        }

        #endregion

        #region 加载颜色

        private void LoadColors(GameDataReader reader)
        {
            int count = reader.ReadInt();
            // 这里我们需要保存一个相对更小的值
            int max = count <= colors.Length ? count : colors.Length;
            int i = 0;
            // 先把max中的全部处理掉，因为是较小的值，所以这些一定可以被正常设置的
            for (; i < max; i++)
            {
                SetColor(reader.ReadColor(), i);
            }
            // count大于colors.Length说明我们保存的颜色比需要的更多，因为Length就是meshRender的数量
            // 这种情况下只读取不实际应用即可
            if (count > colors.Length)
            {
                for (; i < count; i++)
                {
                    reader.ReadColor();
                }
            }
            // 当保存的颜色比需要的还少时，说明有一些数据无论如何读取不到，只能全部设置成白色
            else if (count < colors.Length)
            {
                for (; i < colors.Length; i++)
                {
                    SetColor(Color.white, i);
                }
            }
        }

        #endregion

        #region Behavior相关

        /// <summary>
        /// 添加一个新的行为到behaviorList中
        /// </summary>
        // 访问级别 返回类型 方法名<T>(参数列表)
        public T AddBehavior<T>() where T : ShapeBehavior, new()
        {
            // 添加的时候要从池中去取而不是手动new
            T behavior = ShapeBehaviorPool<T>.Get();
            behaviorList.Add(behavior);
            return behavior;
        }

        #endregion

        #region shape生命周期逻辑相关

        /// <summary>
        /// 回收掉调用此方法的shape
        /// </summary>
        public void Die()
        {
            Game.Instance.Kill(this);
        }

        public void ResolveShapeInstance()
        {
            foreach (ShapeBehavior behavior in behaviorList)
            {
                behavior.ResolveShapeInstances();
            }
        }
        
        public void Recycle()
        {
            Age = 0f;
            /*这里的+=1是为了区分逻辑实例与内存对象
            对象池中，`Shape` 的 C# 对象（内存）是不会被销毁的，而是会被重复利用 
            但是当我们进入这个方法时，我们实际希望做的事是让形状的生命周期结束
            这个时候外部还持有对这个shape的引用，还是同一个内存地址的对象，但我已经不再是之前的那个 ID 了
            并且外部的shapeInstance的isValid属性会进行校验，从而知道引用是否失效*/ 

            InstanceId += 1;
            for (int i = 0; i < behaviorList.Count; i++)
            {
                //Destroy(behaviorList[i]);
                // 不是统一销毁而是让每个行为自己执行自己重写的回收逻辑
                behaviorList[i].Recycle();
            }
            behaviorList.Clear();
            OriginFactory.Reclaim(this);  
        }

        /// <summary>
        /// 调用Game单例中的MarkAsDying，将当前shape标记为需要被销毁
        /// </summary>
        public void MarkAsDying()
        {
            Game.Instance.MarkAsDying(this);
        }

        #endregion
        
        
        private void OnDrawGizmos()
        {
            foreach (var behavior in behaviorList)
            {
                if (behavior is SatelliteShapeBehavior satellite)
                {
                    Gizmos.color = Color.green;
                    Vector3 center = satellite.focalShape.Shape.transform.position; // 世界坐标
                    Vector3 axis = satellite.orbitAxis;
                    Gizmos.DrawLine(center - axis, center + axis);
                }
            }
        }
    }
}

