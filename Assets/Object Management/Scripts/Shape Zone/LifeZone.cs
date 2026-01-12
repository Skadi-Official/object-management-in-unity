using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 只要进入了这个区域的shape在离开时就会被销毁
    /// </summary>
    public class LifeZone : MonoBehaviour
    {
        [SerializeField] private float dyingDuration;

        private void OnTriggerExit(Collider other)
        {
            var shape = other.GetComponent<Shape>();
            if (!shape)
            {
                Debug.LogWarning($"{other.name} is not a shape");
                return;
            }
            if (shape.IsMarkedAsDying) return;
            shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, dyingDuration);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            var c = GetComponent<Collider>();
            if (!c) return;

            // 保存原矩阵，避免污染其他 Gizmos
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // 这里对于boxCollider来说，使用Gizmos.matrix = transform.localToWorldMatrix;也是可以的
            var b = c as BoxCollider;
            if (b != null)
            {
                Gizmos.matrix = Matrix4x4.TRS(
                    transform.position,
                    transform.rotation,
                    transform.lossyScale
                );
                Gizmos.DrawWireCube(b.center, b.size);
                Gizmos.matrix = oldMatrix;
                return;
            }

            // 但是由于unity的SphereCollider 在世界空间中始终保持等比缩放的球体，如果我们还是用localToWorld的话，一旦使用非均匀缩放
            // 就会出现碰撞体由于使用的是缩放的最大绝对值，而gizmos会应用完整的非均匀缩放，会出现不一致
            // 这个时候我们只能自己维护一个变换矩阵来保证gizmos绘制的正确性
            var s = c as SphereCollider;
            if (s != null)
            {
                Vector3 scale = transform.lossyScale;
                scale = Vector3.one * Mathf.Max(
                    Mathf.Abs(scale.x),
                    Mathf.Abs(scale.y),
                    Mathf.Abs(scale.z)
                );

                Gizmos.matrix = Matrix4x4.TRS(
                    transform.position,
                    transform.rotation,
                    scale
                );
                Gizmos.DrawWireSphere(s.center, s.radius);
            }

            Gizmos.matrix = oldMatrix;
        }

    }
}
