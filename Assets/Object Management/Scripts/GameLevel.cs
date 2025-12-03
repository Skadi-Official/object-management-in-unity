using UnityEngine;

namespace ObjectManagement
{
    public class GameLevel : MonoBehaviour
    {
        [SerializeField]private SpawnZone spawnZone;
        private void Start()
        {
            Debug.Log("设置了新的生成点");
            Game.Instance.SpawnZoneOfLevel = spawnZone;
        }
    }
}
