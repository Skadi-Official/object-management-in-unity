using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 行为的抽象基础类，继承它来实现具体的移动
    /// </summary>
    public abstract class ShapeBehavior : MonoBehaviour
    {
        /// <summary>
        /// 子类实现行为时子类所属的行为类型
        /// </summary>
        public abstract ShapeBehaviorType BehaviorType { get; }
        
        /// <summary>
        /// 在Update中需要执行的逻辑
        /// </summary>
        /// <param name="shape"></param>
        public abstract void GameUpdate(Shape shape);
        
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
    }
}