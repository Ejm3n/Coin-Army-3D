using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MissionService : MonoBehaviour
{
    #region Singleton
    private static MissionService _default;
    public static MissionService Default => _default;
    #endregion

    private void Awake()
    {
        _default = this;
    }

    public GameData.Mission GetCurrentMission()
    {
        return GameData.Default.Missions[PlayerPrefs.GetInt("CurrentMission")];
    }

    public int GetCurrentMissionProgress()
    {
        return PlayerPrefs.GetInt("CurrentMissionProgress");
    }

    public void ProgressMission(int amount, Action onMissionComplete)
    {
        int max = GetCurrentMission().TargetAmount;

        int next = GetCurrentMissionProgress() + amount;

        PlayerPrefs.SetInt("CurrentMissionProgress", next);

        if (next >= max)
        {
            onMissionComplete();
        }
    }

    public void NextMission(Action onMissionComplete)
    {
        var currentMission = GetCurrentMission();

        switch (currentMission.RewardType)
        {
            case GameData.RewardType.Money:
                MoneyService.Default.AddMoney(currentMission.RewardAmount);
                break;
            case GameData.RewardType.Spins:
                SpinsService.Default.AddSpins((int)currentMission.RewardAmount, false);
                break;
        }

        int next = PlayerPrefs.GetInt("CurrentMission") + 1;

        if (next >= GameData.Default.Missions.Length)
        {
            next = 0;
        }

        PlayerPrefs.SetInt("CurrentMission", next);

        int leftover = Mathf.Max(0, GetCurrentMissionProgress() - currentMission.TargetAmount);

        PlayerPrefs.SetInt("CurrentMissionProgress", 0);

        if (leftover > 0)
        {
            ProgressMission(leftover, onMissionComplete);
        }
    }
}
