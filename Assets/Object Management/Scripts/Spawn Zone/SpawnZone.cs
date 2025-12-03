using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    public abstract class SpawnZone : MonoBehaviour
    {
        public abstract Vector3 SpawnPoint { get; }
    }
}
