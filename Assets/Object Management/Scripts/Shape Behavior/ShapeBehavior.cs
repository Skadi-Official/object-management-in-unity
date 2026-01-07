using System;
using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 行为的抽象基础类，继承它来实现具体的移动，基础类不继承MonoBehaviour
    /// </summary>
    public abstract class ShapeBehavior 
    #if UNITY_EDITOR
        : ScriptableObject // 这里用so是为了在编辑器模式下支持热重载
    #endif
    {
        #if UNITY_EDITOR
        /// <summary>
        /// 当前行为是否被标记为已经被回收
        /// </summary>
        public bool IsReclaimed { get; set; }

        private void OnEnable()
        {
            if (IsReclaimed)
            {
                Recycle();
            }
        }
        #endif

        /// <summary>
        /// 子类实现行为时子类所属的行为类型
        /// </summary>
        public abstract ShapeBehaviorType BehaviorType { get; }
        
        /// <summary>
        /// 在Update中需要执行的逻辑
        /// </summary>
        /// <param name="shape"></param>
        public abstract bool GameUpdate(Shape shape);
        
        /// <summary>
        /// 存档时需要执行的逻辑
        /// </summary>
        /// <param name="writer"></param>
        public abstract void Save(GameDataWriter writer);
        
        /// <summary>
        /// 读档时需要执行的逻辑
        /// </summary>
        /// <param name="reader"></param>
        public abstract void Load(GameDataReader reader);

        /// <summary>
        /// 回收行为对象需要执行的逻辑
        /// </summary>
        public abstract void Recycle();
        /// <summary>
        /// 将shape持有的SaveIndex解析成instanceID，需要子类手动实现
        /// </summary>
        public virtual void ResolveShapeInstances() { }
    }
}