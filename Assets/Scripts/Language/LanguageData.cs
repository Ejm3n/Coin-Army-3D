using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

[CreateAssetMenu(fileName = "New Language", menuName = "Localization/Language")]
public class LanguageData : ScriptableObject
{
    public SerializableDictionary<string, string> Strings = new SerializableDictionary<string, string>();
}
