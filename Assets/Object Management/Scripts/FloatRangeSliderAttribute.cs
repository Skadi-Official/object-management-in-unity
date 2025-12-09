using UnityEngine;

namespace ObjectManagement
{
    // 定义一个可用于字段上的自定义 Attribute
    public class FloatRangeSliderAttribute : PropertyAttribute
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public FloatRangeSliderAttribute(float min, float max)
        {
            if (max < min) max = min;
            Min = min; Max = max;
        }
    }    
}