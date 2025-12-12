using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// shape的所有行为模式枚举
    /// </summary>
    public enum ShapeBehaviorType
    {
        Movement,           // 简单线性移动
        Rotation,           // 简单单向旋转
        Oscillation         // 来回震动
    }

    public static class ShapeBehaviorTypeMethods
    {
        public static ShapeBehavior GetInstance(this ShapeBehaviorType type)
        {
            switch (type)
            {
                case ShapeBehaviorType.Movement:
                    return ShapeBehaviorPool<MovementShapeBehavior>.Get();
                case ShapeBehaviorType.Rotation:
                    return ShapeBehaviorPool<RotationShapeBehavior>.Get();
                case ShapeBehaviorType.Oscillation:
                    return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
            }
            Debug.Log($"未实现或设置该行为模式对应的脚本");
            return null;
        }
    }
    
    // 这是一个静态类与扩展方法的实现示例
    // 扩展方法必须写在静态类里面，并且扩展方法也必须是静态方法
    // 语法：public static 返回类型 方法名(this 拓展类型 参数名，其他参数)
    // 紧跟在this后面的扩展类型就是我们希望将当前方法拓展到的类型
    // 这个示例中我们希望把这个方法拓展给int类型，this后面紧跟着的就是int
    public static class ExtendMethodsToInt
    {
        public static void ExtendMethodToInt(this int num, bool needToDebug = false)
        {
            if (!needToDebug) return;
            Debug.Log(num);
        }
    }
}