using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreMountains.NiceVibrations;

public class Cell : MonoBehaviour
{
    public GameObject OutlineRenderer;

    [NonSerialized]
    public bool IsHighlighted;

    void Update()
    {
        if (!OutlineRenderer.activeSelf && IsHighlighted)
        {
            MMVibrationManager.Haptic(HapticTypes.LightImpact, false, true, this);
        }
        OutlineRenderer.SetActive(IsHighlighted);
    }
}
