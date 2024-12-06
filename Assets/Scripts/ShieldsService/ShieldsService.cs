using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShieldsService : MonoBehaviour
{
    #region Singleton
    private static ShieldsService _default;
    public static ShieldsService Default => _default;
    #endregion

    public Action OnShieldsChanged;

    private void Awake()
    {
        _default = this;
    }

    public int GetShields()
    {
        return PlayerPrefs.GetInt("ShieldCount", GameData.Default.initialShieldAmount);
    }

    public void SetShields(int amount)
    {
        PlayerPrefs.SetInt("ShieldCount", Mathf.Min(amount, GameData.Default.maxShields));
    }

    public void SpendShield()
    {
        SetShields(Mathf.Max(0, GetShields() - 1));
        OnShieldsChanged?.Invoke();
    }

    public void AddShields(int amount)
    {
        int oldShields = GetShields();
        SetShields(oldShields + amount);
        if (oldShields != GetShields())
        {
            OnShieldsChanged?.Invoke();
        }
    }
}
