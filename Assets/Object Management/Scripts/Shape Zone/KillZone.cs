using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 当shape进入区域时将其销毁
    /// </summary>
    public class KillZone : MonoBehaviour
    {
        [SerializeField] private float dyingDuration;
        private void OnTriggerEnter(Collider other)
        {
            var shape = other.GetComponent<Shape>();
            if (shape)
            {
                if (dyingDuration <= 0f)
                {
                    shape.Die();   
                }
                else if(!shape.IsMarkedAsDying)
                {
                    shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, dyingDuration);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;

            var c = GetComponent<Collider>();
            if (!c) return;

            // 保存原矩阵，避免污染其他 Gizmos
            Matrix4x4 oldMatrix = Gizmos.matrix;

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
