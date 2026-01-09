using System;
using UnityEngine;

namespace ObjectManagement
{
    public class DyingShapeBehavior : ShapeBehavior
    {
        public Vector3 originalScale;
        public float duration, dyingAge;
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Dying;

        public void Initialize(Shape shape, float duration)
        {
            originalScale = shape.transform.localScale;
            this.duration = duration;
            dyingAge = shape.Age;
            shape.MarkAsDying();
        }
        
        public override bool GameUpdate(Shape shape)
        {
            // dyingDuration是一个不断变大的值
            float dyingDuration = shape.Age - dyingAge;
            if (dyingDuration < duration)
            {
                // 最终的缩放值是零所以这里要取反
                float s = 1f - dyingDuration / duration;
                s = (3f - 2f * s) * s * s;
                shape.transform.localScale = s * originalScale;
                return true;
            }
            //shape.transform.localScale = Vector3.zero;
            shape.Die();
            return true;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(originalScale);
            writer.Write(duration);
            writer.Write(dyingAge);
        }

        public override void Load(GameDataReader reader)
        {
            originalScale = reader.ReadVector3();
            duration = reader.ReadFloat();
            dyingAge = reader.ReadFloat();
        }

        public override void Recycle()
        {
            ShapeBehaviorPool<DyingShapeBehavior>.Reclaim(this);
        }
    }
}
