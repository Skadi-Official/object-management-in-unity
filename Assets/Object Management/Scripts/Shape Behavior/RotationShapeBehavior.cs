using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 按照指定Vector3进行旋转的简单行为，不可被继承
    /// </summary>
    public sealed class RotationShapeBehavior : ShapeBehavior
    {
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Rotation;
        public Vector3 AngularVelocity { get; set; }
        
        public override bool GameUpdate(Shape shape)
        {
            shape.transform.Rotate(AngularVelocity * Time.deltaTime);
            return true;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(AngularVelocity);
        }

        public override void Load(GameDataReader reader)
        {
            AngularVelocity = reader.ReadVector3();
            //Debug.Log($"RotationShapeBehavior Load::{AngularVelocity}");
        }

        public override void Recycle()
        {
            ShapeBehaviorPool<RotationShapeBehavior>.Reclaim(this);
        }
    }
}
