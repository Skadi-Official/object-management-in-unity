using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ObjectManagement
{
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject
    {
        [SerializeField] private Shape[] prefabs;
        [SerializeField] private Material[] materials;
        [SerializeField] private bool recycle;

        // 每个形状都要有一个池，所以我们使用一个数组存储所有的池，这个池我们用列表实现
        private List<Shape>[] pools;

        private void CreatePools()
        {
            pools = new List<Shape>[prefabs.Length];
            for (int i = 0; i < pools.Length; i++)
            {
                pools[i] = new List<Shape>();
            }
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
                }
            }
            else
            {
                instance = Instantiate(prefabs[shapeID]);
                // 创建时直接写入对应的编号
                instance.ShapeID = shapeID;
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