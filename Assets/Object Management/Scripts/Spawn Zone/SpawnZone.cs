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
    public abstract class SpawnZone : GameLevelObject
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
                // 卫星的生命周期是否要和主体保持一致，值为true时保持一致
                public bool uniformLifecycles;
            }

            public SatelliteConfiguration Satellite;
            
            [Serializable]
            // 生命周期配置
            public struct LifecycleConfiguration
            {
                // 成长期时间
                [FloatRangeSlider(0f, 2f)] public FloatRange growingDuration;
                // 成年期时间
                [FloatRangeSlider(0f, 100f)] public FloatRange adultDuration;
                // 死亡期持续时间，不是开始死亡的时间，开始死亡时间由生长和成年期持续时间决定
                [FloatRangeSlider(0f, 2f)] public FloatRange dyingDuration;
                public Vector3 RandomDurations =>
                    new(growingDuration.RandomValueInRange,
                        adultDuration.RandomValueInRange,
                        dyingDuration.RandomValueInRange);
            }

            public LifecycleConfiguration lifecycle;
        }
        [SerializeField] private SpawnConfiguration spawnConfig;
        [SerializeField, Range(0f, 50f)] private float spawnSpeed;
        private float spawnProgress;
        /// <summary>
        /// 提供给子类重写的抽象生成点
        /// </summary>
        public abstract Vector3 SpawnPoint { get; }
        #region 更新逻辑

        public override void GameUpdate()
        {
            spawnProgress += Time.deltaTime * spawnSpeed;
            while (spawnProgress >= 1f)
            {
                spawnProgress -= 1f;
                SpawnShapes();
            }
        }

        #endregion

        #region 生成shape或者satellite

        /// <summary>
        /// 可以被重写的具体生成逻辑，会返回一个Shape
        /// </summary>
        /// <returns></returns>
        public virtual void SpawnShapes()
        {
            int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
            shape.gameObject.layer = gameObject.layer;
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
            Vector3 lifecycleDurations = spawnConfig.lifecycle.RandomDurations;
            for (int i = 0; i < satelliteCount; i++)
            {
                CreateSatelliteFor(shape,
                    spawnConfig.Satellite.uniformLifecycles ?
                    lifecycleDurations : spawnConfig.lifecycle.RandomDurations);
            }
            // 给卫星添加了生长也需要给自身添加这个行为
            SetupLifecycle(shape, lifecycleDurations);
            //return shape;
        }

        /// <summary>
        /// 为指定shape创建一个卫星，更新后卫星也应该有生长行为
        /// </summary>
        /// <param name="focalShape">被卫星环绕的物体</param>
        public void CreateSatelliteFor(Shape focalShape, Vector3 lifecycleDurations)
        {
            int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
            shape.gameObject.layer = gameObject.layer;
            Transform t = shape.transform;
            t.localRotation = Random.rotation;
            t.localScale = focalShape.transform.localScale * 
                           spawnConfig.Satellite.relativeScale.RandomValueInRange;
            SetupColor(shape);
            shape.AddBehavior<SatelliteShapeBehavior>().Initialize(
                shape, focalShape, 
                spawnConfig.Satellite.orbitRadius.RandomValueInRange,
                spawnConfig.Satellite.orbitFrequency.RandomValueInRange);
            
            SetupLifecycle(shape, lifecycleDurations);
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

        #region 添加生长行为

        /// <summary>
        /// 根据生长 / 成熟 / 死亡三个阶段的持续时间，
        /// 决定给 Shape 挂哪些生命周期相关的 Behavior。
        /// 
        /// durations.x -> 生长时间（Growing）
        /// durations.y -> 成熟时间（Adult）
        /// durations.z -> 死亡时间（Dying）
        /// </summary>
        private void SetupLifecycle(Shape shape, Vector3 durations)
        {
            // ====== 情况一：存在生长阶段 ======
            if (durations.x > 0f) 
            {
                // 如果生长之后还有成熟阶段 或 死亡阶段，
                // 说明这是一个“多阶段生命周期”，需要 LifecycleBehavior 来调度
                if (durations.y > 0f || durations.z > 0f)
                {
                    // LifecycleBehavior 会：
                    // 1. 立刻添加 GrowingBehavior
                    // 2. 在合适时间进入 DyingBehavior
                    shape.AddBehavior<LifecycleShapeBehavior>().Initialize(
                        shape, durations.x, durations.y, durations.z
                    );
                }
                else 
                {
                    // 只有生长阶段，生长完成后一直存活
                    // 不存在阶段切换，直接使用 GrowingBehavior 即可
                    shape.AddBehavior<GrowingShapeBehavior>().Initialize(
                        shape, durations.x
                    );
                }
            }
            // ====== 情况二：没有生长，但有成熟阶段 ======
            else if (durations.y > 0f) 
            {
                // Shape 一开始就是完整状态
                // 经过一段成熟期后进入死亡阶段
                // 仍然存在阶段切换，因此使用 LifecycleBehavior
                shape.AddBehavior<LifecycleShapeBehavior>().Initialize(
                    shape, durations.x, durations.y, durations.z
                );
            }
            // ====== 情况三：只有死亡阶段 ======
            else if (durations.z > 0f) 
            {
                // Shape 立刻开始死亡，不需要任何阶段调度
                // 直接使用 DyingBehavior，避免 LifecycleBehavior 的额外开销
                shape.AddBehavior<DyingShapeBehavior>().Initialize(
                    shape, durations.z
                );
            }
            // ====== 情况四：三个阶段全为 0 ======
            // 什么都不做：
            // Shape 立即出现、永久存活、也不会死亡
        }

        #endregion

        #region 存读档逻辑相关

        public override void Save(GameDataWriter writer)
        {
            writer.Write(spawnProgress);
        }

        public override void Load(GameDataReader reader)
        {
            spawnProgress = reader.ReadFloat();
        }

        #endregion
    }
}