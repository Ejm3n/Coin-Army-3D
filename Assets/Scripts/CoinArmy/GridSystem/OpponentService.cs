using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OpponentService : MonoBehaviour
{
    #region Singleton
    private static OpponentService _default;
    public static OpponentService Default => _default;
    #endregion

    [NonSerialized]
    public OpponentDescription Description;
    [NonSerialized]
    public int ShieldsLeft;
    [NonSerialized]
    public ulong MoneyLeft;

    private void Awake()
    {
        _default = this;
        ChangeOpponent();
    }

    public void ChangeOpponent()
    {
        OpponentDescription newOpponent;

        int currentOpponentIndex = Array.IndexOf(GameData.Default.AllOpponents, Description);

        currentOpponentIndex++;

        if (currentOpponentIndex > GameData.Default.AllOpponents.Length - 1)
        {
            currentOpponentIndex = 0;
        }

        newOpponent = GameData.Default.AllOpponents[currentOpponentIndex];

        SetOpponent(newOpponent);

        //LevelManager.Default.DoNormalLevel();
    }

    public void SetOpponent(OpponentDescription opponent)
    {
        Description = opponent;
        ShieldsLeft = Description.ShieldsAmount;
        MoneyLeft = Description.MoneyAmount;
    }
}
