using UnityEngine;
using UnityEditor;

namespace ObjectManagement
{
    /// <summary>
    /// 自定义在 Inspector 中绘制 FloatRange（min/max）的方式。
    /// 默认 FloatRange 会被拆成两个 float 字段，但我们让它们并排显示。
    /// </summary>
    [CustomPropertyDrawer(typeof(FloatRange)), CustomPropertyDrawer(typeof(IntRange))]
    public class FloatOrIntRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 保存原始编辑器布局数据：
            int originalIndentLevel = EditorGUI.indentLevel;        // 缩进层级（影响标签缩进）
            float originalLabelWidth = EditorGUIUtility.labelWidth; // 标签宽度（影响字段左侧 label 的占比）

            // 开始绘制序列化属性（用于支持 Prefab override、Undo/Redo）
            EditorGUI.BeginProperty(position, label, property);

            // 绘制左侧总标签（例如 "Hue" / "Saturation"），
            // 返回“标签右侧的剩余区域”，用于绘制 min 和 max 两个字段。
            //
            // 使用 Passive 的 ControlID：  
            //   - Prevent label from being selectable  
            //   - Prevent label from taking keyboard focus  
            //   - Prevent label highlight issue
            position = EditorGUI.PrefixLabel(
                position,
                GUIUtility.GetControlID(FocusType.Passive),
                label
            );

            // 拆分区域：宽度一分为二，用来绘制 min 和 max
            position.width = position.width / 2f;

            // 每个字段内部的子标签（min/max）宽度设置为一半
            EditorGUIUtility.labelWidth = position.width / 2f;

            // 将缩进重设为 1，使内部 min/max 有更好的视觉对齐
            EditorGUI.indentLevel = 1;

            // 绘制 min 输入框（使用 FloatRange 内部的“min”字段）
            EditorGUI.PropertyField(position, property.FindPropertyRelative("min"));

            // 移动到右边，绘制 max
            position.x += position.width;

            // 绘制 max 输入框
            EditorGUI.PropertyField(position, property.FindPropertyRelative("max"));

            // 结束绘制属性（用于正确处理序列化、prefab override 等）
            EditorGUI.EndProperty();

            // 恢复原始编辑器布局（避免影响其它字段绘制）
            EditorGUI.indentLevel = originalIndentLevel;
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }
    }
}
