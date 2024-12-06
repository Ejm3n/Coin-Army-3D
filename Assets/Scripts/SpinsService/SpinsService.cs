using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpinsService : MonoBehaviour
{
    #region Singleton
    private static SpinsService _default;
    public static SpinsService Default => _default;
    #endregion

    public Action OnSpinsChanged;

    private void Awake()
    {
        _default = this;
    }

    public int GetSpins()
    {
        return PlayerPrefs.GetInt("SpinCount", GameData.Default.initialSpinsAmount);
    }

    public void SetSpins(int amount)
    {
        PlayerPrefs.SetInt("SpinCount", amount);
    }

    public void SpendSpin()
    {
        SetSpins(Mathf.Max(0, GetSpins() - 1));
        OnSpinsChanged?.Invoke();
    }

    public void AddSpins(int amount, bool clamp)
    {
        int oldSpins = GetSpins();
        if (clamp)
        {
            SetSpins(oldSpins + Mathf.Clamp(amount, 0, GameData.Default.maxSpinsAmount - oldSpins));
        }
        else
        {
            SetSpins(oldSpins + amount);
        }
        if (oldSpins != GetSpins())
        {
            OnSpinsChanged?.Invoke();
        }
    }

    [ContextMenu("Add Spins - 10")]
    private void AddSpins10()
    {
        AddSpins(10, false);
    }
}
