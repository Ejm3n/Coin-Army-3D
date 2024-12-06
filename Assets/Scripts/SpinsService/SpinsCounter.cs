using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SpinsCounter : MonoBehaviour
{
    private TextMeshProUGUI _tmp;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        HandleOnSpinsUpdated();
        SpinsService.Default.OnSpinsChanged += HandleOnSpinsUpdated;
    }

    private void OnDisable()
    {
        SpinsService.Default.OnSpinsChanged -= HandleOnSpinsUpdated;
    }

    private void HandleOnSpinsUpdated() 
    {
        string text = MoneyService.AmountToStringTrunicate(SpinsService.Default.GetSpins());
        _tmp.text = text;
    }
}
