using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShieldsCounter : MonoBehaviour
{
    private TextMeshProUGUI _tmp;

    void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        HandleOnShieldsUpdated();
        ShieldsService.Default.OnShieldsChanged += HandleOnShieldsUpdated;
    }

    private void OnDisable()
    {
        ShieldsService.Default.OnShieldsChanged -= HandleOnShieldsUpdated;
    }

    private void HandleOnShieldsUpdated() 
    {
        string text = $"{ShieldsService.Default.GetShields()}/{GameData.Default.maxShields}";
        _tmp.text = text;
    }
}
