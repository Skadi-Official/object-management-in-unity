using UnityEngine;

namespace ObjectManagement
{
    public class CompositeSpawnZone : SpawnZone
    {
        [SerializeField]
        SpawnZone[] spawnZones;

        public override Vector3 SpawnPoint
            => spawnZones[Random.Range(0, spawnZones.Length)].SpawnPoint;
    }
}

