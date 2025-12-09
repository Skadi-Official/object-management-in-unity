using UnityEditor;
using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 自定义属性绘制器：在 Inspector 中绘制带最大最小值的滑条（MinMaxSlider）
    /// 用于 FloatRangeSliderAttribute 类型的字段
    /// </summary>
    [CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))] // 指定这个 PropertyDrawer 适用于 FloatRangeSliderAttribute
    public class FloatRangeSliderDrawer : PropertyDrawer
    {
        // 重写 OnGUI 方法来绘制自定义 Inspector UI
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 保存原来的缩进等级，以便在结束绘制后恢复
            int originalIndentLevel = EditorGUI.indentLevel;

            // 告诉 Unity 开始绘制一个属性，确保支持 Prefab 重写、Undo/Redo 等
            EditorGUI.BeginProperty(position, label, property);

            // 绘制属性的标签，并返回剩余可用区域给 position
            position = EditorGUI.PrefixLabel(
                position, 
                GUIUtility.GetControlID(FocusType.Passive), // 获取唯一的控制 ID，避免选择输入框时标签高亮
                label
            );

            // 设置缩进为 0，保证布局不会受全局缩进影响
            EditorGUI.indentLevel = 0;

            // 获取 min 和 max 对应的 SerializedProperty
            SerializedProperty minProperty = property.FindPropertyRelative("min");
            SerializedProperty maxProperty = property.FindPropertyRelative("max");

            // 读取当前值
            float minValue = minProperty.floatValue;
            float maxValue = maxProperty.floatValue;

            // 计算布局宽度：左右输入框各占四分之一，中间滑条占一半，输入框与滑条间留 4 像素间距
            float fieldWidth = position.width / 4f - 4f;
            float sliderWidth = position.width / 2f;

            // 绘制 min 输入框
            position.width = fieldWidth;
            minValue = EditorGUI.FloatField(position, minValue); // 返回用户可能修改的新值

            // 移动 position 到滑条区域
            position.x += fieldWidth + 4f;
            position.width = sliderWidth;

            // 获取当前属性的 FloatRangeSliderAttribute，以便获取 Min/Max 限制
            FloatRangeSliderAttribute limit = attribute as FloatRangeSliderAttribute;

            // 绘制滑条（MinMaxSlider），可以同时调整 min/max
            EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, limit.Min, limit.Max);

            // 移动 position 到 max 输入框
            position.x += sliderWidth + 4f;
            position.width = fieldWidth;

            // 绘制 max 输入框
            maxValue = EditorGUI.FloatField(position, maxValue);

            // 限制值，保证 min/max 不超出范围，且 max 不小于 min
            if (minValue < limit.Min)
            {
                minValue = limit.Min;
            }
            else if (minValue > limit.Max)
            {
                minValue = limit.Max;
            }

            if (maxValue < minValue)
            {
                maxValue = minValue;
            }
            else if (maxValue > limit.Max) 
            {
                maxValue = limit.Max;
            }

            // 写回值给 SerializedProperty，同步到 Unity 的序列化系统
            // 这样才能保证 Inspector 显示、Undo/Redo、Prefab 检测等功能正常
            minProperty.floatValue = minValue;
            maxProperty.floatValue = maxValue;

            // 结束绘制属性
            EditorGUI.EndProperty();

            // 恢复原始缩进
            EditorGUI.indentLevel = originalIndentLevel;
        }
    }
}
