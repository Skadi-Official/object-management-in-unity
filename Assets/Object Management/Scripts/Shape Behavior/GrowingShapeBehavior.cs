using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
   public sealed class GrowingShapeBehavior : ShapeBehavior
   {
      // 形状在被生成时的初始缩放
      private Vector3 originalScale;
      // 生长效果的持续时间
      private float duration;
      public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Growing;
      
      public void Initialize(Shape shape, float duration)
      {
         originalScale = shape.transform.localScale;
         this.duration = duration;
         shape.transform.localScale = Vector3.zero;
      }
      
      public override bool GameUpdate(Shape shape)
      {
         if (shape.Age < duration)
         {
            float s = shape.Age / duration;  // 当前形状创建的时间和生长时间的比值，比值从0开始到1为止
            s = (3f - 2f * s) * s * s;
            shape.transform.localScale = s * originalScale;
            return true;
         }
         Debug.Log("shape.transform.localScale = originalScale;");
         shape.transform.localScale = originalScale;
         return false;
      }

      public override void Save(GameDataWriter writer)
      {
         writer.Write(originalScale);
         writer.Write(duration);
      }

      public override void Load(GameDataReader reader)
      {
         originalScale = reader.ReadVector3();
         duration = reader.ReadFloat();
      }

      public override void Recycle()
      {
         ShapeBehaviorPool<GrowingShapeBehavior>.Reclaim(this);
      }

      
   } 
}

