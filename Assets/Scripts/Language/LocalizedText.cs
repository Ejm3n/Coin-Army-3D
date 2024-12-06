using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string LocalizationKey;

    void Start()
    {
        GetComponent<TextMeshPro>().text = Language.Text(LocalizationKey);
    }
}
