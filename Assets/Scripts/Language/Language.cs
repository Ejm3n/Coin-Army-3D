using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class Language : MonoBehaviour
{
    private static bool initialized = false;

    private static LanguageData currentLanguage;

    public void Awake()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        currentLanguage = GameData.Default.AllLanguages[GameData.Default.CurrentLanguage];
    }

    public static string Text(string key)
    {
        if (currentLanguage == null)
        {
            return key;
        }
        if (currentLanguage.Strings.TryGetValue(key, out string result))
        {
            return result;
        }
        return key;
    }
}
