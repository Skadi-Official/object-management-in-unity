using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 按照指定Vector3进行旋转的简单行为
    /// </summary>
    public class RotationShapeBehavior : ShapeBehavior
    {
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Rotation;
        public Vector3 AngularVelocity { get; set; }
        
        public override void GameUpdate(Shape shape)
        {
            shape.transform.Rotate(AngularVelocity * Time.deltaTime);
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
    }
}
