using System;
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
        }
        [SerializeField] private SpawnConfiguration spawnConfig;
        /// <summary>
        /// 提供给子类重写的抽象生成点
        /// </summary>
        public abstract Vector3 SpawnPoint { get; }
        /// <summary>
        /// 可以被重写的具体生成逻辑，会返回一个Shape
        /// </summary>
        /// <returns></returns>
        public virtual Shape SpawnShape()
        {
            int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
            Transform t = shape.transform;
            t.localPosition = SpawnPoint; // Game -> GameLevel -> SpawnZone(abstract) -> SpawnZone(override)
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
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
            float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
            if (angularSpeed != 0f) // 如果随机出来的结果是0的话就不需要再去添加组件了
            {
                var rotation = shape.AddBehavior<RotationShapeBehavior>();
                rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
            }
            // switch表达式的写法，本质上和传统case break没有区别，下划线表示default值
            Vector3 direction = spawnConfig.movementDirection switch
            {
                SpawnConfiguration.MovementDirection.Forward => transform.forward,
                SpawnConfiguration.MovementDirection.Upward => transform.up,
                SpawnConfiguration.MovementDirection.Outward =>
                    (t.localPosition - transform.position).normalized,
                SpawnConfiguration.MovementDirection.Random => Random.onUnitSphere,
                _ => Vector3.forward
            };
            float speed = spawnConfig.spawnSpeed.RandomValueInRange;
            if (speed != 0) // 如果随机出来的结果是0的话就不需要再去添加组件了
            {
                var movement = shape.AddBehavior<MovementShapeBehavior>();
                movement.Velocity = direction * speed;
            }
            return shape;
        }
    }
}