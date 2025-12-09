using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    [Serializable]
    public struct ColorRangeHSV
    {
        // hue: 色相，0度为红色，120度为绿色，240度为蓝色
        // saturation: 饱和度，色彩的深浅度；value: 色调，色彩的亮度
        // 这个附加属性表示这个字段在编辑器中要用 FloatRangeSliderDrawer 来绘制，并且范围是 0 到 1
        [FloatRangeSlider(0f, 1f)]  
        public FloatRange hue, saturation, value;
        public Color RandomInRange =>
            Random.ColorHSV(hue.min, hue.max, saturation.min, saturation.max, value.min, value.max, 1f, 1f);
    }
}
