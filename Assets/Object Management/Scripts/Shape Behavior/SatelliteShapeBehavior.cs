using Unity.VisualScripting;
using UnityEngine;

namespace ObjectManagement
{
    public class SatelliteShapeBehavior : ShapeBehavior
    {
        #region 卫星行为的数据配置
        public ShapeInstance focalShape;
        public float frequency;
        public Vector3 cosOffset, sinOffset;
        public Vector3 orbitAxis;
        public Vector3 previousPosition;    // 上一帧的位置
        #endregion
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Satellite;
        public override bool GameUpdate(Shape shape)
        {
            // 如果焦点不存在，我们用上一帧的位置和当前位置算出一个速度并施加一个移动行为来模拟甩出去的行为
            if (!focalShape.IsValid)
            {
                shape.AddBehavior<MovementShapeBehavior>().Velocity =
                    (shape.transform.localPosition - previousPosition) / Time.deltaTime;
                return false;
            }
            // 这里使用2f * Mathf.PI是为了保证frequency参数的物理意义，即一秒转几圈
            // 2f * Mathf.PI：用弧度表示的数值，frequency * shape.Age是圈/秒 * 秒 => 即2pi * 圈数
            float t = 2f * Mathf.PI * frequency * shape.Age;
            previousPosition = shape.transform.localPosition;
            var currentFocalPos = focalShape.Shape.transform.localPosition;
            var newPosX = cosOffset * Mathf.Cos(t);
            var newPosZ = sinOffset * Mathf.Sin(t);
            shape.transform.localPosition = currentFocalPos + newPosX + newPosZ;
            return true;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(focalShape);
            writer.Write(frequency);
            writer.Write(cosOffset);
            writer.Write(sinOffset);
            writer.Write(previousPosition);
        }

        public override void Load(GameDataReader reader)
        {
            focalShape = reader.ReadShapeInstance();
            frequency = reader.ReadFloat();
            cosOffset = reader.ReadVector3();
            sinOffset = reader.ReadVector3();
            previousPosition = reader.ReadVector3();
            //Debug.Log($"SatelliteShapeBehavior Load::{frequency} {cosOffset} {sinOffset} {previousPosition}");
        }

        public override void Recycle()
        {
            ShapeBehaviorPool<SatelliteShapeBehavior>.Reclaim(this);
        }

        #region 初始化逻辑

        /// <summary>
        /// 初始化卫星运动数据
        /// </summary>
        /// <param name="shape">作为卫星的shape</param>
        /// <param name="focalShape">作为焦点的shape</param>
        /// <param name="radius">轨道半径</param>
        /// <param name="frequency">环绕频率</param>
        public void Initialize(Shape shape, Shape focalShape, float radius, float frequency)
        {
            this.focalShape = focalShape;
            this.frequency = frequency;
            // 卫星的旋转轴随机获取
            orbitAxis = Random.onUnitSphere;
            // 得到一条“必定垂直于轨道法线的方向向量(以 orbitAxis 为法线的平面)”，也就是“轨道平面里的一条轴”。
            do
            {
                cosOffset = Vector3.Cross(orbitAxis, Random.onUnitSphere).normalized;
            } while (cosOffset.sqrMagnitude < 0.01f);
            // 得到一个必定垂直与cosOffset的方向向量
            sinOffset = Vector3.Cross(cosOffset, orbitAxis);
            cosOffset *= radius;
            sinOffset *= radius;
            shape.AddBehavior<RotationShapeBehavior>().AngularVelocity =
                -360f * frequency * shape.transform.InverseTransformDirection(orbitAxis);
            // 为了确保卫星在生成时就处于正确的位置，需要在 Initialize 的末尾手动调用一次 GameUpdate。
            // 因为在形状被生成的同一帧内，GameUpdate 不会被自动调用。
            GameUpdate(shape);
            // 初始化的时候也需要设置位置
            previousPosition = shape.transform.localPosition;
        }

        #endregion

        public override void ResolveShapeInstances()
        {
            focalShape.Resolve();
        }
    }
}