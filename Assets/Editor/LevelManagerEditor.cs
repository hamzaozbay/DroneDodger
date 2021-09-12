using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        LevelManager levelManager = target as LevelManager;


        EditorGUILayout.Space(60f);

        EditorGUI.BeginChangeCheck();
        levelManager.currentLevel = EditorGUILayout.Popup("Level", levelManager.currentLevel, levelManager.GetLevelsName());
        if (EditorGUI.EndChangeCheck()) {
            levelManager.ShowLevel(levelManager.currentLevel);
        }


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Update Level")) {
            levelManager.UpdateLevel(levelManager.currentLevel);
        }
        if (GUILayout.Button("Delete Level")) {
            levelManager.DeleteLevel(levelManager.currentLevel);
        }
        GUILayout.EndHorizontal();


        EditorGUILayout.Space(40f);
        EditorGUILayout.LabelField("Randomize Level", EditorStyles.boldLabel);
        levelManager.randomFloorLength = EditorGUILayout.IntField("Floor Length", levelManager.randomFloorLength);
        if (GUILayout.Button("Create Randomize Level")) {
            levelManager.CreateRandomLevel();
        }
        if (GUILayout.Button("Add New Level")) {
            levelManager.CreateNewLevel();
        }


        if (GUI.changed) {
            EditorUtility.SetDirty(levelManager);
        }
    }

}
