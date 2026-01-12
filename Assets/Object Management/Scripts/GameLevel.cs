using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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
    public partial class GameLevel : PersistableObject
    {
        // 当前激活的关卡实例（全局唯一）
        public static GameLevel Current { get; private set; }
        // 数量限制
        public int PopulationLimit => populationLimit;
        
        [SerializeField] private int populationLimit;
        
        // 关卡内部使用的生成区域
        [SerializeField] private SpawnZone spawnZone;

        // 每个关卡脚本都持有这个关卡下所有需要被持久化保存的对象的引用
        [FormerlySerializedAs("persistableObjects")]
        [SerializeField] private GameLevelObject[] levelObjects;
        
        // 当关卡启用时，自动注册为当前关卡
        private void OnEnable()
        {
            Current = this;
            // 由于对象引用数组可能是为空的，所以在OnEnable里面我们加一层判断
            if (levelObjects == null)
            {
                // persistableObjects = new PersistableObject[0];
                // 使用 'Array.Empty<PersistableObject>()' 以避免数组分配
                levelObjects = Array.Empty<GameLevelObject>();
            }
        }

        /// <summary>
        /// 调用当前激活的关卡内部使用的生成区域的生成逻辑，返回生成区域生成的形状
        /// </summary>
        /// <param name="shape"></param>
        public void ConfigureSpawn()
        {
            spawnZone.SpawnShapes();
        }

        #region 更新逻辑

        public void GameUpdate()
        {
            foreach (var levelObject in levelObjects)
            {
                levelObject.GameUpdate();
            }
        }

        #endregion
        
        #region 重写存读档

        // 对于关卡本身，我们需要记录关卡持有的所有需要被持久化保存的物体的数量，并调用这些物体自己的Save方法
        public override void Save(GameDataWriter writer)
        {
            writer.Write(levelObjects.Length);
            for (int i = 0; i < levelObjects.Length; i++)
            {
                levelObjects[i].Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            int savedCount = reader.ReadInt();
            for (int i = 0; i < savedCount; i++)
            {
                levelObjects[i].Load(reader);
            }
        }

        #endregion
    }
}