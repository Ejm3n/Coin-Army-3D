using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocalizedTextUI : MonoBehaviour
{
    public string LocalizationKey;

    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = Language.Text(LocalizationKey);
    }
}
