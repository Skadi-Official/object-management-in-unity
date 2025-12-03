using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    public class CubeSpawnZone : SpawnZone
    {
        [SerializeField] private bool surfaceOnly;
        public override Vector3 SpawnPoint
        {
            get
            {
                // 这里我们在一个单位立方体中随机取一个点，中心是000，边长是1
                Vector3 p = Vector3.zero;
                p.x = Random.Range(-0.5f, 0.5f);
                p.y = Random.Range(-0.5f, 0.5f);
                p.z = Random.Range(-0.5f, 0.5f);
                // 取好一个点之后把它的坐标转换到世界空间下的位置，这个位置会收到挂载这个脚本的物体的位置旋转缩放影响
                if (surfaceOnly)
                {
                    // 随机选出一个轴，这个轴上目前的坐标如果小于0，就直接设置到-0.5，否则就是0.5，这样就实现了只在边上生成
                    int axis = Random.Range(0, 3);
                    p[axis] = p[axis] < 0 ? -0.5f : 0.5f;
                }
                return transform.TransformPoint(p);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}

