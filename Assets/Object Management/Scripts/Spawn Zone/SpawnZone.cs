using System;
using UnityEngine;
using UnityEngine.EventSystems;
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
            public enum SpawnMovementDirection
            {
                Forward,    // 往前
                Upward,     // 往上
                Outward,    // 往外
                Random      // 随机
            }
            // 创建时物体被赋予的移动方向
            public SpawnMovementDirection spawnMovementDirection;
            // 创建时物体被赋予的速度
            public FloatRange spawnSpeed;
            // 创建时物体被赋予的角速度
            public FloatRange angularSpeed;
            // 创建时物体的缩放
            public FloatRange scale;
            // 创建时物体的颜色
            public ColorRangeHSV color;
        }
        [SerializeField] private SpawnConfiguration spawnConfig;
        /// <summary>
        /// 对外暴露的生成点（由子类决定生成规则）
        /// 每次访问时，都可以返回一个新的随机位置或规则位置
        /// </summary>
        public abstract Vector3 SpawnPoint { get; }
        public virtual void ConfigureSpawn(Shape shape)
        {
            Transform t = shape.transform;
            t.localPosition = SpawnPoint; // Game -> GameLevel -> SpawnZone(abstract) -> SpawnZone(override)
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
            shape.SetColor(spawnConfig.color.RandomInRange);
            shape.AngularVelocity = Vector3.one * spawnConfig.angularSpeed.RandomValueInRange;
            // switch表达式的写法，本质上和传统case break没有区别，下划线表示default值
            Vector3 direction = spawnConfig.spawnMovementDirection switch
            {
                SpawnConfiguration.SpawnMovementDirection.Forward => transform.forward,
                SpawnConfiguration.SpawnMovementDirection.Upward => transform.up,
                SpawnConfiguration.SpawnMovementDirection.Outward =>
                    (t.localPosition - transform.position).normalized,
                SpawnConfiguration.SpawnMovementDirection.Random => Random.onUnitSphere,
                _ => Vector3.forward
            };
            shape.Velocity = direction * spawnConfig.spawnSpeed.RandomValueInRange;
        }
    }
}