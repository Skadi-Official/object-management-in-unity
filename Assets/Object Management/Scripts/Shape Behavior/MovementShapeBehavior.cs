using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 按照固定方向和速度进行线性移动的简单行为，不可被继承
    /// </summary>
    public sealed class MovementShapeBehavior : ShapeBehavior
    {
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Movement;
        public Vector3 Velocity { get; set; }
        
        public override bool GameUpdate(Shape shape)
        {
            shape.transform.localPosition += Velocity * Time.deltaTime;
            return true;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(Velocity);
            //Debug.Log($"MovementShapeBehavior Save::{Velocity}");
        }

        public override void Load(GameDataReader reader)
        {
            Velocity = reader.ReadVector3();
            //Debug.Log($"MovementShapeBehavior Load::{Velocity}");
        }

        public override void Recycle()
        {
            ShapeBehaviorPool<MovementShapeBehavior>.Reclaim(this);
        }
    }
}
