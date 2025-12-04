using System;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    public class CompositeSpawnZone : SpawnZone
    {
        [SerializeField]
        SpawnZone[] spawnZones;

        [SerializeField] private bool sequential; // 是否启用按顺序生成
        private int nextSequentialIndex = 0;    

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

        private void Start()
        {
            Debug.Log($"{nextSequentialIndex}");
        }

        #region 重写Save Load
        
        public override void Save(GameDataWriter writer)
        {
            // 保存时同时记录顺序
            writer.Write(nextSequentialIndex);
        }
        
        public override void Load(GameDataReader reader)
        {
            nextSequentialIndex = reader.ReadInt();
        }
        
        #endregion
    }
}

