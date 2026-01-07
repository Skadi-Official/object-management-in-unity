using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    /// <summary>
    /// 【SpawnZone - 生成区域抽象基类】
    ///
    /// 核心职责：
    /// 抽象定义“生成点”的获取规则
    /// 作为所有具体生成区域（如圆形、方形、多边形区域等）的统一父类
    /// 通过 SpawnPoint 属性，对外提供一个“合法的生成位置”
    ///
    /// 架构意义：
    /// 通过抽象基类 + 多态，解耦 GameLevel 与具体生成区域的实现方式
    /// 支持随时替换不同规则的生成区域，而无需修改上层逻辑
    /// 与 GameLevel 组合后，形成：
    ///     Game → GameLevel → SpawnZone 的清晰职责链
    ///
    /// 设计模式：
    /// 策略模式（Strategy）：
    ///     不同 SpawnZone 子类 = 不同的“生成策略”
    /// </summary>
    public abstract class SpawnZone : PersistableObject
    {
        [Serializable]
        public struct SpawnConfiguration
        {
            public enum MovementDirection
            {
                Forward,    // 往前
                Upward,     // 往上
                Outward,    // 往外
                Random      // 随机
            }
            // 创建物体的工厂
            public ShapeFactory[] factories;
            // 创建时物体被赋予的移动方向
            public MovementDirection movementDirection;
            // 创建时物体被赋予的速度
            public FloatRange spawnSpeed;
            // 创建时物体被赋予的角速度
            public FloatRange angularSpeed;
            // 创建时物体的缩放
            public FloatRange scale;
            // 创建时物体的颜色
            public ColorRangeHSV color;
            // 是否使用统一的颜色
            public bool uniformColor;
            // 震荡方向
            public MovementDirection oscillationDirection;
            // 震荡幅度
            public FloatRange oscillationAmplitude;
            // 震荡频率
            public FloatRange oscillationFrequency;
            // 卫星配置
            [Serializable]
            public struct SatelliteConfiguration
            {
                // 卫星的随机数量范围
                public IntRange amount;
                [FloatRangeSlider(0f, 1f)]
                // 相对于焦点形状的缩放
                public FloatRange relativeScale;
                // 卫星的轨道半径
                public FloatRange orbitRadius;
                // 卫星的环绕频率
                public FloatRange orbitFrequency;
            }

            public SatelliteConfiguration Satellite;
        }
        [SerializeField] private SpawnConfiguration spawnConfig;
        /// <summary>
        /// 提供给子类重写的抽象生成点
        /// </summary>
        public abstract Vector3 SpawnPoint { get; }

        #region 生成shape或者satellite

        /// <summary>
        /// 可以被重写的具体生成逻辑，会返回一个Shape
        /// </summary>
        /// <returns></returns>
        public virtual void SpawnShapes()
        {
            int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
            Transform t = shape.transform;
            t.localPosition = SpawnPoint; // Game -> GameLevel -> SpawnZone(abstract) -> SpawnZone(override)
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
            SetupColor(shape);
            float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
            if (angularSpeed != 0f) // 如果随机出来的结果是0的话就不需要再去添加组件了
            {
                var rotation = shape.AddBehavior<RotationShapeBehavior>();
                rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
            }
            // switch表达式的写法，本质上和传统case break没有区别，下划线表示default值
            Vector3 direction = GetDirectionVector(spawnConfig.movementDirection, t);
            float speed = spawnConfig.spawnSpeed.RandomValueInRange;
            if (speed != 0) // 如果随机出来的结果是0的话就不需要再去添加组件了
            {
                var movement = shape.AddBehavior<MovementShapeBehavior>();
                movement.Velocity = direction * speed;
            }
            // 生成时都需要尝试设置震荡行为，如果参数为零就不会实际设置
            SetupOscillation(shape);

            int satelliteCount = spawnConfig.Satellite.amount.RandomValueInRange;

            for (int i = 0; i < satelliteCount; i++)
            {
                CreateSatelliteFor(shape);
            }
            //return shape;
        }

        /// <summary>
        /// 为指定shape创建一个卫星
        /// </summary>
        /// <param name="focalShape">被卫星环绕的物体</param>
        public void CreateSatelliteFor(Shape focalShape)
        {
            int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
            Transform t = shape.transform;
            t.localRotation = Random.rotation;
            t.localScale = focalShape.transform.localScale * 
                           spawnConfig.Satellite.relativeScale.RandomValueInRange;
            SetupColor(shape);
            shape.AddBehavior<SatelliteShapeBehavior>().Initialize(
                shape, focalShape, 
                spawnConfig.Satellite.orbitRadius.RandomValueInRange,
                spawnConfig.Satellite.orbitFrequency.RandomValueInRange);
        }
        
        #endregion

        #region 设置颜色

        private void SetupColor(Shape shape)
        {
            if (spawnConfig.uniformColor)
            {
                shape.SetColor(spawnConfig.color.RandomInRange);    
            }
            else
            {
                for (int i = 0; i < shape.ColorCount; i++)
                {
                    shape.SetColor(spawnConfig.color.RandomInRange, i);
                }
            }
        }

        #endregion
        
        #region 移动方向获取

        /// <summary>
        /// 返回一个方向矢量，由传入参数决定
        /// </summary>
        /// <param name="direction">所有可能的移动方向</param>
        /// <param name="t">方向矢量的原点物体</param>
        /// <returns></returns>
        private Vector3 GetDirectionVector(SpawnConfiguration.MovementDirection direction, Transform t)
        {
            switch (direction)
            {
                case SpawnConfiguration.MovementDirection.Forward:
                    return t.forward;
                case SpawnConfiguration.MovementDirection.Upward:
                    return t.up;
                case SpawnConfiguration.MovementDirection.Outward:
                    return (t.localPosition - transform.position).normalized;
                case SpawnConfiguration.MovementDirection.Random:
                    return Random.onUnitSphere;
                default:
                    return t.forward;
            }
        }

        #endregion

        #region 添加震荡行为

        /// <summary>
        /// 根据配置参数设置震荡行为
        /// </summary>
        /// <param name="shape"></param>
        private void SetupOscillation(Shape shape)
        {
            float amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
            float frequency = spawnConfig.oscillationFrequency.RandomValueInRange;
            if (frequency == 0 || amplitude == 0f) return;
            var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
            oscillation.Offset = GetDirectionVector(spawnConfig.oscillationDirection, shape.transform) * amplitude;
            oscillation.Frequency = frequency;
        }

        #endregion
    }
}