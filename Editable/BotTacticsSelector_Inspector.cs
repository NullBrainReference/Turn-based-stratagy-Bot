using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CanEditMultipleObjects]

[CustomEditor(typeof(BotTacticsSelector), true)]
public class BotTacticSelector_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        var targerClass = (BotTacticsSelector)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("SaveTactics") == true)
        {
            TacticsCollection tacticsCollection = new TacticsCollection();
            var tactics = targerClass.TacticModels;

            tacticsCollection.SetTactics(tactics);
            tacticsCollection.SaveToFIle();
        }

        EditorGUILayout.EndHorizontal();

        base.OnInspectorGUI();
        serializedObject.Update();
    }
}

#endif
