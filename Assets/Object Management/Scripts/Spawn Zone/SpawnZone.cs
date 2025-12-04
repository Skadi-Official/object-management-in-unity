using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// 【SpawnZone - 生成区域抽象基类】
    ///
    /// 核心职责：
    /// 1️⃣ 抽象定义“生成点”的获取规则
    /// 2️⃣ 作为所有具体生成区域（如圆形、方形、多边形区域等）的统一父类
    /// 3️⃣ 通过 SpawnPoint 属性，对外提供一个“合法的生成位置”
    ///
    /// 架构意义：
    /// ✅ 通过抽象基类 + 多态，解耦 GameLevel 与具体生成区域的实现方式
    /// ✅ 支持随时替换不同规则的生成区域，而无需修改上层逻辑
    /// ✅ 与 GameLevel 组合后，形成：
    ///     Game → GameLevel → SpawnZone 的清晰职责链
    ///
    /// 设计模式：
    /// ✅ 策略模式（Strategy）：
    ///     不同 SpawnZone 子类 = 不同的“生成策略”
    /// </summary>
    public abstract class SpawnZone : MonoBehaviour
    {
        /// <summary>
        /// 对外暴露的生成点（由子类决定生成规则）
        /// 每次访问时，都可以返回一个新的随机位置或规则位置
        /// </summary>
        public abstract Vector3 SpawnPoint { get; }
    }
}