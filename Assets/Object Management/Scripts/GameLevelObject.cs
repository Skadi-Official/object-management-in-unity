using UnityEngine;

namespace ObjectManagement
{
    public class GameLevelObject : PersistableObject
    {
        //调用链：Game - GameLevel - GameLevelObject - 各重写子类
        public virtual void GameUpdate() { }
    }
}
