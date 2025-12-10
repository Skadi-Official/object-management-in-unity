using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject
    {
        public int FactoryId
        {
            get => factoryId;
            set
            {
                // 当id还是默认值且传入的参数不是默认值时我们才修改
                if (factoryId == Int32.MinValue && value != Int32.MinValue)
                {
                    factoryId = value;
                }
                else
                {
                    Debug.LogError("工厂id已经被设置，无法再修改");
                }
            }
        }
        [SerializeField] private Shape[] prefabs;                   // 工厂能生成的预制体
        [SerializeField] private Material[] materials;              // 用到的材质
        [SerializeField] private bool recycle;                      // 是否开启回收（使用对象池）
        // 如果这个id被序列化了，会在意想不到的情况下被unity保存，例如复制一份so时
        // 如果不序列化，数据不会持久化，但也不会被错误保存。即使会在重启 Unity 或脚本重载时丢失，但这是可控的、可预测的行为。
        [NonSerialized]  private int factoryId = Int32.MinValue;    // 工厂ID，用于存档，它只被代码控制

        private Scene poolScene;
        // 每个形状都要有一个池，所以我们使用一个数组存储所有的池，这个池我们用列表实现
        private List<Shape>[] pools;

        /// <summary>
        /// 创建回收所用的池，同时创建场景来把所有对象放置在单独的场景里面
        /// </summary>
        private void CreatePools()
        {
            pools = new List<Shape>[prefabs.Length];
            for (int i = 0; i < pools.Length; i++)
            {
                pools[i] = new List<Shape>();
            }

            // 仅在 Unity 编辑器中执行。Play 模式下热重编译只会发生在编辑器里，
            // 所以这个恢复逻辑不需要在打包后的游戏中运行。
            if (Application.isEditor)
            {
                // 根据工厂的名字获取之前创建的 poolScene。
                // 由于热重编译会重置 ScriptableObject 的字段，原先的 Scene struct 可能丢失引用，
                // 所以需要重新获取 Scene 的引用。
                poolScene = SceneManager.GetSceneByName(name);

                // 如果场景已经加载，说明之前创建过对象池场景
                if (poolScene.isLoaded)
                {
                    Debug.Log("poolScene场景已经加载");

                    // 获取该场景下的所有根对象（Root GameObjects）。
                    // 根对象就是没有父对象的对象，poolScene 专门存放所有 Shape 实例，所以这里得到的就是所有 Shape
                    GameObject[] rootObjects = poolScene.GetRootGameObjects();

                    // 遍历所有根对象，检查哪些是可以回收的 Shape
                    foreach (var rootObject in rootObjects)
                    {
                        // 获取根对象上的 Shape 组件
                        Shape pooledShape = rootObject.GetComponent<Shape>();

                        // 如果这个 Shape 的 GameObject 当前未激活（inactive），
                        // 表示它在对象池中可被回收
                        if (!rootObject.gameObject.activeSelf)
                        {
                            // 将这个 Shape 添加回对应形状类型的对象池
                            // 这里用 ShapeID 作为池数组索引区分不同类型的 Shape
                            pools[pooledShape.ShapeID].Add(pooledShape);
                        }
                    }

                    // 恢复完对象池后，直接返回，不再执行 CreatePools 的逻辑
                    return;
                }
            }

            // 这个name就是ScriptableObject在目录里面的名字
            poolScene = SceneManager.CreateScene(name);
        }
        
        /// <summary>
        /// 生成一个指定形状与材质的物体并返回Shape，只有这个方法执行了实际的生成逻辑
        /// </summary>
        /// <param name="shapeID"></param>
        /// <param name="materialID"></param>
        /// <returns></returns>
        public Shape Get(int shapeID = 0, int materialID = 0)
        {
            Shape instance;
            if (recycle)
            {
                if (pools == null)
                {
                    CreatePools();
                }
                List<Shape> pool = pools[shapeID];
                int lastIndex = pool.Count - 1;
                // 只要lastIndex大于等于0说明池中还有可用的元素，将其移除池并激活
                // 池里面只保留空闲对象，并且由于我们不关心顺序，所以每次都移除最后一个以保持效率为O1
                if (lastIndex >= 0)
                {
                    instance = pool[lastIndex];
                    instance.gameObject.SetActive(true);
                    pool.RemoveAt(lastIndex);
                }
                // 否则只能再生成一个
                else
                {
                    // 被创建的对象一定是当前工厂手动指定的prefab中的一个，把工厂id记录到shape中去
                    instance = Instantiate(prefabs[shapeID]);
                    instance.OriginFactory = this;
                    instance.ShapeID = shapeID;
                    // 创建后把物体迁移到另一个场景
                    SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
                }
            }
            else
            {
                instance = Instantiate(prefabs[shapeID]);
                // 创建时直接写入对应的编号
                instance.ShapeID = shapeID;
                // 创建后把物体迁移到另一个场景
                SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
            }
            instance.SetMaterial(materials[materialID], materialID);
            return instance;
        }

        /// <summary>
        /// 随机返回一个形状
        /// </summary>
        /// <returns></returns>
        public Shape GetRandom()
        {
            // 如果 Random.Range 使用的是 整数参数，那么不会包含最大值。Random.Range(0, 3)只会返回 0、1、2
            return Get(Random.Range(0, prefabs.Length), Random.Range(0, materials.Length));
        }

        /// <summary>
        /// 将传入的形状关闭或者销毁
        /// </summary>
        /// <param name="shapeToRecycle"></param>
        public void Reclaim(Shape shapeToRecycle)
        {
            if (shapeToRecycle.OriginFactory != this)
            {
                Debug.LogError("当前被调用的工厂回收方法不是创建当前Shape的工厂");
                return;
            }
            // 如果启用了回收，关闭物体并将其添加到池中表示可以被使用
            // 否则直接销毁
            if (recycle)
            {
                if(pools == null) CreatePools();
                shapeToRecycle.gameObject.SetActive(false);
                pools[shapeToRecycle.ShapeID].Add(shapeToRecycle);
            }
            else
            {
                Destroy(shapeToRecycle.gameObject);
            }
        }
    }
}