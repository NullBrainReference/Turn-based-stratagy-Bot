using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CanEditMultipleObjects]

[CustomEditor(typeof(TacticsCreator), true)]
public class TacticsCreator_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        var targerClass = (TacticsCreator)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save") == true)
        {
            targerClass.SaveCollection();
        }

        if (GUILayout.Button("Load") == true)
        {
            targerClass.LoadCollection();
        }

        EditorGUILayout.EndHorizontal();

        base.OnInspectorGUI();
        serializedObject.Update();
    }
}

#endif
