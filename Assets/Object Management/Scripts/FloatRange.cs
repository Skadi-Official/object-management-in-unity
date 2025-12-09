using System;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    [Serializable]
    public struct FloatRange
    {
        public float min, max;
        public float RandomValueInRange => Random.Range(min, max); // 返回一个大于min小于max的值
    }    
}
