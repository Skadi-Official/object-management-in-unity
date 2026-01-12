using System;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    public class CompositeSpawnZone : SpawnZone
    {
        public override Vector3 SpawnPoint
        {
            get
            {
                int index;
                if (sequential)
                {
                    index = nextSequentialIndex++;
                    nextSequentialIndex %= spawnZones.Length;
                }
                else
                {
                    index = Random.Range(0, spawnZones.Length);
                }
                return spawnZones[index].SpawnPoint;
            }
        }

        [SerializeField] private bool overrideConfig;   // 由于复合生成区自己也是一个SpawnZone，可以有自己的配置
        [SerializeField] private SpawnZone[] spawnZones;
        [SerializeField] private bool sequential; // 是否启用按顺序生成
        private int nextSequentialIndex = 0;

        #region 重写Save Load
        
        public override void Save(GameDataWriter writer)
        {
            base.Save(writer);
            // 保存时同时记录顺序
            writer.Write(nextSequentialIndex);
        }
        
        public override void Load(GameDataReader reader)
        {
            if (reader.Version > 7)
            {
                base.Load(reader);
            }
            nextSequentialIndex = reader.ReadInt();
        }
        
        #endregion

        #region 重写生成逻辑

        public override void SpawnShapes()
        {
            // 勾选overrideConfig后就不会使用自己绑定的spawnZones的生成逻辑，而是用CompositeSpawnZone自己的
            if (overrideConfig)
            {
                base.SpawnShapes();
            }
            else
            {
                int index = 0;
                if (sequential)
                {
                    index = nextSequentialIndex++;
                    if(nextSequentialIndex >= spawnZones.Length) nextSequentialIndex = 0;
                }
                else
                {
                    index = Random.Range(0, spawnZones.Length);
                }
                spawnZones[index].SpawnShapes();
            }
        }

        #endregion
    }
}

