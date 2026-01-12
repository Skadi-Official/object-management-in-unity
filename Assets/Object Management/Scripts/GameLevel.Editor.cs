#if UNITY_EDITOR
using System;
using UnityEngine;

namespace ObjectManagement
{
    public partial class GameLevel
    {
        /// <summary>
        /// levelObjects中是否存在缺失的关卡对象
        /// </summary>
        public bool HasMissingLevelObjects
        {
            get
            {
                if(levelObjects == null) return false;
                foreach (var levelObject in levelObjects)
                {
                    if (levelObject == null) return true;
                }
                return false;
            }
        }

        
        #region 提供给编辑器的方法

        /// <summary>
        /// 移除丢失绑定的level
        /// </summary>
        public void RemoveMissingLevelObjects()
        {
            if (Application.isPlaying)
            {
                Debug.Log("Do not invoke in play mode");
                return;
            }
            int holes = 0;
            for (int i = 0; i < levelObjects.Length - holes; i++)
            {
                if (levelObjects[i] == null)
                {
                    holes++;
                    //从 sourceArray 的 sourceIndex 开始，复制 length 个元素，
                    //到 destinationArray 的 destinationIndex 开始的位置。
                    Array.Copy(levelObjects, i + 1, 
                        levelObjects, i, levelObjects.Length - i - holes);
                    i -= 1;
                }
            }
            // 去除完成后移除多余的空位
            Array.Resize(ref levelObjects, levelObjects.Length - holes);
        }

        /// <summary>
        /// 添加一个GameLevelObject到列表中
        /// </summary>
        /// <param name="o"></param>
        public void RegisterLevelObject(GameLevelObject o)
        {
            if (Application.isPlaying)
            {
                Debug.Log("Do not invoke in play mode");
                return;
            }

            if (HasLevelObject(o)) return;

            if (levelObjects == null)
            {
                levelObjects = new GameLevelObject[] { o };
            }
            else
            {
                Array.Resize(ref levelObjects, levelObjects.Length + 1);
                levelObjects[^1] = o;
            }
        }

        /// <summary>
        /// 查询列表中是否存在某个GameLevelObject
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool HasLevelObject(GameLevelObject o)
        {
            if(levelObjects == null) return false;
            return Array.IndexOf(levelObjects, o) >= 0;
        }
        
        #endregion
    }
}

#endif