using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 泛型的行为对象池，每个行为类都会有自己的池来进行放入和取出，将会在第一次被使用时由CLR自动构造
    /// </summary>
    /// <typeparam name="T">必须继承ShapeBehavior</typeparam>
    public static class ShapeBehaviorPool<T> where T : ShapeBehavior, new()
    {
        private static Stack<T> stack = new Stack<T>();
        /// <summary>
        /// 从池中返回一个当前行为，如果没有就会创建
        /// </summary>
        /// <returns>当前池管理的行为类型的实例</returns>
        public static T Get()
        {
            if (stack.Count > 0)
            {
                T behavior = stack.Pop();
#if UNITY_EDITOR
                behavior.IsReclaimed = false;
#endif
                return behavior;
            }
            // 如果要通过无参构造函数来创建实例，在定义方法时必须在where后面显式规范构造函数
            // 但对于'ScriptableObject' 实例必须使用 'ScriptableObject.CreateInstance<T>()' 而不是 'new' 来实例化
#if UNITY_EDITOR
            return ScriptableObject.CreateInstance<T>();
#else
            return new T();
#endif
        }

        /// <summary>
        /// 将当前行为放入池中
        /// </summary>
        /// <param name="behavior">需要放入池的行为类实例</param>
        public static void Reclaim(T behavior)
        {
#if UNITY_EDITOR
            behavior.IsReclaimed = true;
#endif
            stack.Push(behavior);
        }
    }
}