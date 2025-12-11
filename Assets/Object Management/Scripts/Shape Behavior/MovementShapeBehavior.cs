using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 按照固定方向和速度进行线性移动的简单行为
    /// </summary>
    public class MovementShapeBehavior : ShapeBehavior
    {
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Movement;
        public Vector3 Velocity { get; set; }
        
        public override void GameUpdate(Shape shape)
        {
            shape.transform.localPosition += Velocity * Time.deltaTime;
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
    }
}
