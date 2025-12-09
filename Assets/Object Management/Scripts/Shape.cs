using System;
using System.Collections;
using System.Collections.Generic;
using ObjectManagement;
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

        public int MaterialID { get;private set; }
        public Vector3 AngularVelocity { get; set; }
        public Vector3 Velocity { get; set; }
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
        }
        private int shapeID = Int32.MinValue;    // 记录形状种类
        private Color color;
        private MeshRenderer meshRenderer;
        
        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// 这个GameUpdate控制了所有Shape的更新逻辑
        /// </summary>
        public void GameUpdate()
        {
            // 这里的Rotate用法，实际上是把Vector3的三个数据当作在xyz轴上分别旋转的角度应用到物体上
            transform.Rotate(AngularVelocity * Time.deltaTime);
            transform.localPosition += Velocity * Time.deltaTime;
        }

        public void SetMaterial(Material material, int materialID)
        {
            // 设置材质并记录材质对应的编号
            meshRenderer.material = material;
            MaterialID = materialID;
        }
        
        public void SetColor(Color color)
        {
            // 记录传入的颜色并将当前物体的颜色修改为传入的颜色
            this.color = color;
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
            meshRenderer.SetPropertyBlock(sharedPropertyBlock);

        }

        public override void Save(GameDataWriter writer)
        {
            base.Save(writer);
            writer.Write(color);
            writer.Write(AngularVelocity);
            writer.Write(Velocity);
        }

        public override void Load(GameDataReader reader)
        {
            base.Load(reader);
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
            AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
            Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        }
    }
}

