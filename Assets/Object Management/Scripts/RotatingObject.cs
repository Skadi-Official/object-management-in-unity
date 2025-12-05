using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    public class RotatingObject : MonoBehaviour
    {
        [SerializeField]
        Vector3 angularVelocity;

        private void Update()
        {
            transform.Rotate(angularVelocity * Time.deltaTime);
        }
    }
}
