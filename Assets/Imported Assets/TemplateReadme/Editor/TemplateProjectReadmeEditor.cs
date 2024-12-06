using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(TemplateProjectReadme))]
public class TemplateProjectReadmeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.alignment = TextAnchor.MiddleLeft;
        style.wordWrap = true;
        //style.normal.textColor = Color.red;

        GUIStyle boldstyle = new GUIStyle();
        boldstyle.fontSize = 16;
        boldstyle.alignment = TextAnchor.MiddleLeft;
        boldstyle.wordWrap = true;
        //boldstyle.normal.textColor = Color.red;
        boldstyle.fontStyle = FontStyle.Bold;

        GUIStyle italicstyle = new GUIStyle();
        italicstyle.fontSize = 15;
        italicstyle.alignment = TextAnchor.MiddleLeft;
        italicstyle.wordWrap = true;
        //italicstyle.normal.textColor = Color.red;
        italicstyle.fontStyle = FontStyle.Italic;

        GUILayout.BeginVertical(GUILayout.Width(350));

        GUILayout.Label("Полезные ссылки", boldstyle);
        if (GUILayout.Button("Требования к прототипам"))
            Application.OpenURL("https://docs.google.com/document/d/1swz9r2I-CzOFtsNVrkX5nL8mhs2PlD4AUIk05cIm9To/edit");
        if (GUILayout.Button("Хранилище ассетов"))
            Application.OpenURL("https://docs.google.com/spreadsheets/d/1Fitr_FcFMX4d4d5S_OII9c_QMvOVNECYGIR7fgyFmwQ/edit#gid=0");
        EditorGUILayout.Space(20f);

        GUILayout.Label("Level Manager", boldstyle);
        GUILayout.Label("Level Manager - система контроля уровней, исполняющий скрипт висит на префабе [SETUP]. " +
            "Доступ из кода можно получить через Singleton.", style);
        if (GUILayout.Button("Подробнее о LevelManager")) 
        {
            Application.OpenURL("https://docs.google.com/document/d/1-AsBQFhczIxTPW5-7SPv4m4AufyuaCU3ACrl2R68OG8/edit");
        }
        EditorGUILayout.Space(20f);

        GUILayout.Label("UI Manager", boldstyle);
        GUILayout.Label("UI Manager - система контроля интерфейса, находится в префабе [UI].", style);
        GUILayout.Label("Помимо основных панелей имеет вложенные в Canvas и Process Panel соответственно 'FPS Counter' и 'Tutorial Hand'.", style);
        GUILayout.Label("Данные функции включаются и выключаются активацией их GameObject'a", style);
        GUILayout.Label("Для переключения текущей панели из кода можно использовать конструкцию типа:", style);
        GUILayout.Label("UIManager.Default.CurentState = UIState.Start", italicstyle);
        EditorGUILayout.Space(10f);
        GUILayout.Label("Для удобства отслеживания момента переключения панелей присутствует событие:", style);
        GUILayout.Label("Action<UIState, UIState> OnStateChanged", italicstyle);
        EditorGUILayout.Space(20f);

        GUILayout.Label("Data Holders", boldstyle);
        GUILayout.Label("Data Holders - скрипты настроек проекта, по стандарту имеется 2 Scriptable Object, экземпляры которых находятся в дирректории Assets/Settings", style);
        EditorGUILayout.Space(10f);
        GUILayout.Label("   GameData - используется в проектах для хранения глобальных настроек.", style);
        GUILayout.Label("   Настройки добавляются в качестве полей скрипта, для последующего изменения через конфиг.", style);
        EditorGUILayout.Space(10f);
        GUILayout.Label("   SoundHolder - используется в проектах для хранения, настройки и удобного вызова аудиофайлов.", style);
        if (GUILayout.Button("Подробнее о SoundHolder"))
        {
            Application.OpenURL("https://drive.google.com/file/d/1vHvSCk-SXn7-yrr-ZEUzskgTnOmBAuou/view?usp=sharing");
        }
        EditorGUILayout.Space(10f);
        GUILayout.Label("Оба скрипта используют подход Singleton для доступа к ним из кода. " +
            "Для создания нового скрипта, работающего по данному принципу можно использовать наследование от DataHolder, и поместить созданный экземпляр в " +
            "ScriptableInitializer, висящий на объекте [SETUP].", style);
        EditorGUILayout.Space(20f);

        GUILayout.Label("Resolution Handler", boldstyle);
        GUILayout.Label("Resolution Handler - скрипт, подгоняющий field of view камеры к равноценному значению при разных форматах.", style);
        GUILayout.Label("   При использовании пакета Cinemachine необходимо использовать CinemachineResolutionHandler, помещая его на объект с CinemachineVirtualCamera.", style);
        GUILayout.Label("   В противном случае используется ResolutionHandler, который вешается на объект с камерой", style);
        EditorGUILayout.Space(20f);

        GUILayout.Label("Epic Toon FX", boldstyle);
        GUILayout.Label("Epic Toon FX - импортированный пакет, находящийся в дирректории Assets/Imported Assets.", style);
        GUILayout.Label("Содержит в себе огромное количество часто используемых в проектах эффектов, в том числе конфетти.", style);
        EditorGUILayout.Space(20f);

        GUILayout.EndVertical();
    }
}