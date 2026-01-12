using UnityEditor;
using UnityEngine;

namespace ObjectManagement
{
    public static class RegisterLevelObjectMenuItem
    {
        const string menuItem = "GameObject/Register Level Object";
        // 验证函数 会控制菜单项是否可用（enabled/disabled）。
        // 当返回 true 时，菜单项可点击；返回 false 时，菜单项会变灰不可用。
        [MenuItem(menuItem, true)]
        static bool ValidateRegisterLevelObject()
        {
            // 没选中任何东西
            if (Selection.objects.Length == 0) return false;
            foreach (var o in Selection.objects)
            {
                if (!(o is GameObject)) return false;
            }
            return true;
        }
        
        // 1.将方法注册到GameObject菜单下
        [MenuItem(menuItem)]
        static void RegisterLevelObject()
        {
            // 2.获取当前选中的对象
            foreach (var o in Selection.objects)
            {
                Register(o as GameObject);
            }
        }

        static void Register(GameObject o)
        {
            // 3.判断是否为 Prefab 资源（而不是场景对象）
            // if (PrefabUtility.GetPrefabType(o) != PrefabType.Prefab)这样的用法被标记为废弃
            if (PrefabUtility.GetPrefabAssetType(o) != PrefabAssetType.NotAPrefab)
            {
                Debug.LogWarning(o.name + " is a prefab asset.", o);
                return;
            }
            
            // 4.尝试从被选中的对象上获取GameLevelObject组件
            var levelObject = o.GetComponent<GameLevelObject>();
            if (levelObject == null)
            {
                Debug.LogWarning(o.name + " does not have a GameLevelObject component.", o);
                return;
            }

            // 5.查找当前场景的GameLevel组件
            foreach (var rootObject in o.scene.GetRootGameObjects())
            {
                var gameLevel = rootObject.GetComponent<GameLevel>();
                if (gameLevel != null) 
                {
                    if (gameLevel.HasLevelObject(levelObject))
                    {
                        Debug.LogWarning(o.name + " is already registered in level " + gameLevel.name, o);
                        return;
                    }
                    Undo.RecordObject(gameLevel, "Register Level Object");
                    // 如果找到了gameLevel组件并且没有添加这个levelObject就添加
                    gameLevel.RegisterLevelObject(levelObject);
                    Debug.Log(
                        o.name + " registered to game level " +
                        gameLevel.name + " in scene " + o.scene.name + ".", o
                    );
                    return;
                }
            }
            Debug.LogWarning(o.name + " isn't part of a game level.", o);
        }
    }
}
