using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SoundHolder))]
public class SoundHolderEditor : Editor
{
    private SoundHolder _soundHolder;
    private ReorderableList _list;
    private Dictionary<string, ReorderableList> _subLists = new Dictionary<string, ReorderableList>();


    private void Awake()
    {
        _soundHolder = target as SoundHolder;
    }

    private void OnEnable()
    {
        _list = new ReorderableList(serializedObject, serializedObject.FindProperty("soundPacks"), true, true, true, true);

        _list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var soundPack = _list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty key = soundPack.FindPropertyRelative("key");
            SerializedProperty sounds = soundPack.FindPropertyRelative("sounds");

            ReorderableList subList;
            string listKey = soundPack.propertyPath;
            if (_subLists.ContainsKey(listKey))
                subList = _subLists[listKey];
            else 
            {
                subList = new ReorderableList(soundPack.serializedObject, sounds, true, true, true, true);
                subList.drawElementCallback = (Rect subrect, int subindex, bool subIsActive, bool subIsFocused) =>
                {
                    SerializedProperty clip = sounds.GetArrayElementAtIndex(subindex).FindPropertyRelative("clip");
                    SerializedProperty volume = sounds.GetArrayElementAtIndex(subindex).FindPropertyRelative("volume");

                    EditorGUI.PropertyField( 
                        new Rect(subrect.x, subrect.y, subrect.width/2, EditorGUIUtility.singleLineHeight),
                        clip, GUIContent.none);

                    volume.floatValue = EditorGUI.Slider(
                        new Rect((subrect.width / 2f) + 70, subrect.y, (subrect.width / 2f) - 10, EditorGUIUtility.singleLineHeight),
                        volume.floatValue, 0f, 1f);
                };
                subList.drawHeaderCallback = (Rect subRect) => 
                {
                    key.stringValue = EditorGUI.TextField(new Rect(subRect.x, subRect.y, subRect.width, subRect.height), key.stringValue);
                };

                _subLists[listKey] = subList;
            }
            subList.DoList(new Rect(rect.x, rect.y, rect.width, rect.height));
        };
        _list.elementHeightCallback = (int index) =>
        {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty sounds = element.FindPropertyRelative("sounds");
            return (sounds.arraySize + 4) * EditorGUIUtility.singleLineHeight;
        };
        _list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Sound Settings");
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        _list.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}