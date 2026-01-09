using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
    public class LifecycleShapeBehavior : ShapeBehavior
    {
        // 成体状态持续时间，死亡过程的持续时间，从出生开始多久开始死亡
        private float adultDuration, dyingDuration, dyingAge;
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.LifeCycle;

        public void Initialize(Shape shape,
            float growingDuration, float adultDuration, float dyingDuration)
        {
            this.adultDuration = adultDuration;
            this.dyingDuration = dyingDuration;
            dyingAge = growingDuration + adultDuration;

            if (growingDuration > 0)
            {
                shape.AddBehavior<GrowingShapeBehavior>().Initialize(shape, growingDuration);
            }
        }
        
        public override bool GameUpdate(Shape shape)
        {
            if (shape.Age >= dyingAge)
            {
                if (dyingDuration <= 0f)
                {
                    shape.Die();
                    return true;
                }

                if (!shape.IsMarkedAsDying)
                {
                    shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, 
                        dyingDuration + dyingAge - shape.Age);
                }
                return false;
            }
            return true;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(adultDuration);
            writer.Write(dyingDuration);
            writer.Write(dyingAge);
        }

        public override void Load(GameDataReader reader)
        {
            adultDuration = reader.ReadFloat();
            dyingDuration = reader.ReadFloat();
            dyingAge = reader.ReadFloat();
        }

        public override void Recycle()
        {
            ShapeBehaviorPool<LifecycleShapeBehavior>.Reclaim(this);
        }
    }

}