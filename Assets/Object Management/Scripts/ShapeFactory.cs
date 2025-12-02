using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject
    {
        [SerializeField] private Shape[] prefabs;
        [SerializeField] private Material[] materials;
        /// <summary>
        /// 生成一个指定形状与材质的物体并返回Shape
        /// </summary>
        /// <param name="shapeID"></param>
        /// <param name="materialID"></param>
        /// <returns></returns>
        public Shape Get(int shapeID = 0, int materialID = 0)
        {
            // 创建时直接写入对应的编号
            Shape instance = Instantiate(prefabs[shapeID]);
            instance.ShapeID = shapeID;
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
    }
}