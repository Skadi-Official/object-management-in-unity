using ObjectManagement;
using UnityEditor;
using UnityEngine;

namespace ObjectManagement
{
    [CustomEditor(typeof(GameLevel))]
    public class GameLevelInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var gameLevel = target as GameLevel;
            if (gameLevel != null && gameLevel.HasMissingLevelObjects)
            {
                EditorGUILayout.HelpBox("Missing Level Objects", MessageType.Error);
                if (GUILayout.Button("Remove Missing Elements"))
                {
                    Undo.RecordObject(gameLevel, "Remove Missing Level Objects");
                    gameLevel.RemoveMissingLevelObjects();
                }
            }
        }
    }

}