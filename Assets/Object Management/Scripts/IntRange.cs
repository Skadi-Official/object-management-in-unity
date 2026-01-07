using UnityEngine;

namespace ObjectManagement
{
    [System.Serializable]
    public struct IntRange
    {
        public int min, max;

        public int RandomValueInRange => Random.Range(min, max + 1);

    }    
}
