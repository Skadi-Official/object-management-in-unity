using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObjectManagement
{
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject
    {
        [SerializeField] private Shape[] prefabs;
        [SerializeField] private Material[] materials;
        [SerializeField] private bool recycle;

        private Scene poolScene;
        // 每个形状都要有一个池，所以我们使用一个数组存储所有的池，这个池我们用列表实现
        private List<Shape>[] pools;

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
        /// 生成一个指定形状与材质的物体并返回Shape
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
                    instance = Instantiate(prefabs[shapeID]);
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

        public void Reclaim(Shape shapeToRecycle)
        {
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