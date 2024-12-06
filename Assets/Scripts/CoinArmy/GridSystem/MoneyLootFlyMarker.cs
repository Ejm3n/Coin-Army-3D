using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyLootFlyMarker : MonoBehaviour
{
    public static MoneyLootFlyMarker Default { get; private set; }

    void Awake()
    {
        Default = this;
    }
}
