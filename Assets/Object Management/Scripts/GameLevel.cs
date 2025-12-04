using System;
using UnityEngine;

namespace ObjectManagement
{
    /// <summary>
    /// GameLevel 关卡类 
    /// 表示当前正在运行的关卡实例  
    /// 通过静态 Current 提供“全局可访问的当前关卡”  
    /// 作为 SpawnZone 的“门面（Facade）”，对外仅暴露生成点 SpawnPoint  
    /// 解耦 Game 与 SpawnZone，避免 Game 直接依赖关卡内部结构  
    /// 为后续“关卡状态的持久化保存”提供结构基础  
    /// </summary>
    public class GameLevel : PersistableObject
    {
        // 当前激活的关卡实例（全局唯一）
        public static GameLevel Current { get; private set; }

        // 对外暴露的生成点（由 SpawnZone 实际提供）
        public Vector3 SpawnPoint => spawnZone.SpawnPoint;

        // 关卡内部使用的生成区域
        [SerializeField] private SpawnZone spawnZone;

        // 当关卡启用时，自动注册为当前关卡
        private void OnEnable()
        {
            Current = this;
        }
    }
}