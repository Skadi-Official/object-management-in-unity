using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 来回震荡的移动行为
    /// </summary>
    public class OscillationShapeBehavior : ShapeBehavior
    {
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Oscillation;
        /// <summary>
        /// 偏移值，由振幅和方向相乘得出
        /// </summary>
        public Vector3 Offset { get; set; }
        public float Frequency { get; set; }
        
        private float previousOscillation;  // 上一帧的偏移量
        public override void GameUpdate(Shape shape)
        {
            float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * shape.Age);
            shape.transform.localPosition += (oscillation - previousOscillation) * Offset;
            previousOscillation = oscillation;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(Offset);
            writer.Write(Frequency);
            writer.Write(previousOscillation);
        }

        public override void Load(GameDataReader reader)
        {
            Offset = reader.ReadVector3();
            Frequency = reader.ReadFloat();
            previousOscillation = reader.ReadFloat();
        }

        public override void Recycle()
        {
            previousOscillation = 0f;
            ShapeBehaviorPool<OscillationShapeBehavior>.Reclaim(this);
        }
    }
}
