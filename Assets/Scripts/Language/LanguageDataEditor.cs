#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(LanguageData))]
public class LanguageDataEditor : Editor
{
    private LanguageData data;

    private string newEntryName = "";
    private string removeEntryName = "";

    public void OnEnable()
    {
        data = (LanguageData)target;
    }

    public override void OnInspectorGUI()
    {
        var keys = data.Strings.Keys.ToArray();

        for (int i = 0; i < keys.Length; i++)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(keys[i], GUILayout.Width(200));

            data.Strings[keys[i]] = EditorGUILayout.TextArea(data.Strings[keys[i]]);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                data.Strings.Remove(keys[i]);
                break;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();

        newEntryName = GUILayout.TextField(newEntryName);

        GUI.enabled = !data.Strings.ContainsKey(newEntryName) && !string.IsNullOrEmpty(newEntryName);

        if (GUILayout.Button("Add Entry", GUILayout.Width(100)))
        {
            data.Strings.Add(newEntryName, "");
            newEntryName = "";
        }

        GUI.enabled = true;

        GUILayout.EndHorizontal();

        EditorUtility.SetDirty(data);
    }
}
#endif