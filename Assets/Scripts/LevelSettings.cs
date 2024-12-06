using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using BG.UI.Main;
using DG.Tweening;
using BG.UI.Camera;

public class LevelSettings : MonoBehaviour
{
    #region Singleton
    private static LevelSettings _default;
    public static LevelSettings Default => _default;
    #endregion

    public static float TutorialStage
    {
        get
        {
            if (GameData.Default.DisableTutorial)
            {
                return 1000;
            }
            return PlayerPrefs.GetFloat("TutorialStage", -10);
        }
        set
        {
            if (GameData.Default.DisableTutorial)
            {
                return;
            }
            PlayerPrefs.SetFloat("TutorialStage", value);
            Debug.Log("Tutorial Stage " + value);
        }
    }

    public LevelDescription Description;
    public bool IsOpponentLocation;

    [NonSerialized]
    public bool IsFightActive;
    [NonSerialized]
    public bool IsPostFight;

    [NonSerialized]
    public bool IsWon;

    private bool _wasPaused;
    private double _pauseTime;

    void Awake()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        _default = this;
    }

    public void StartFighting()
    {
        IsFightActive = true;
        if (CameraSystem.Default)
            CameraSystem.Default.CurentState = CameraState.Process;
    }

    public void StopFighting()
    {
        IsFightActive = false;
        if (CameraSystem.Default)
        {
            CameraSystem.Default.CurentState = CameraState.Win;
        }
    }

    public void Retry()
    {
        IsPostFight = true;
        StopFighting();
        UIManager.Default.GetPanel(UIState.Start).HidePanel();
        Action action = () =>
        {
            LevelManager.Default.ReloadLevel();
            UIManager.Default.GetPanel(UIState.Start).ShowPanel();
        };
        Transition.Default.DoTransition(action);

        LevelManager.Default.SendFail(LevelManager.Default.DifficultyCounter);
        
        MoneyService.Default.ResetTempMoney();
    }

    public void Win()
    {
        if (IsPostFight)
        {
            return;
        }

        foreach (var loot in UnitManager.DroppedLoot)
        {
            loot.FlyIntoMoneyCounter();
        }

        MoneyService.Default.FlushTempMoney(true);

        MoneyCounter.AllowUpdate = false;
        MoneyCounter.UpdateTimeout = 1f;

        IsWon = true;
        IsPostFight = true;
        LevelManager.BlockedFromProceeding = false;
        LevelManager.UsesModifiedDifficulty = false;
        LevelManager.UsesModifiedDifficulty2 = false;
        StopFighting();
        UIManager.Default.CurrentState = UIState.Win;

        SoundHolder.Default.PlayFromSoundPack("Win", allowPitchShift: false);

        GameObject.Find("Confetti").GetComponent<ParticleSystem>().Play();

        if (LevelSettings.TutorialStage == 3)
        {
            LevelSettings.TutorialStage = 4.5f;
        }

        if (LevelSettings.TutorialStage == 14)
        {
            LevelSettings.TutorialStage++;
        }

        if (LevelSettings.TutorialStage == 8)
        {
            LevelSettings.TutorialStage++;
        }

        if (LevelSettings.TutorialStage == 9)
        {
            LevelSettings.TutorialStage++;
        }
    }

    public void Lose()
    {
        if (IsPostFight)
        {
            return;
        }

        foreach (var loot in UnitManager.DroppedLoot)
        {
            loot.FlyIntoMoneyCounter();
        }

        MoneyService.Default.FlushTempMoney(false);

        UnitManager.Default.TotalLootYield += Description.AdditionalFailReward;
        MoneyService.Default.AddMoney(Description.AdditionalFailReward);

        if (LevelSettings.TutorialStage == 0 || LevelSettings.TutorialStage == 4 && UnitManager.Default.EnemyUnits.Count == 1)
        {
            LevelSettings.TutorialStage++;
        }

        IsWon = false;
        IsPostFight = true;
        LevelManager.BlockedFromProceeding = true;
        LevelManager.UsesModifiedDifficulty = true;
        LevelManager.UsesModifiedDifficulty2 = true;
        StopFighting();
        UIManager.Default.CurrentState = UIState.Fail;

        SoundHolder.Default.PlayFromSoundPack("GameOver", allowPitchShift: false);

        LevelManager.Default.SendFail(LevelManager.Default.DifficultyCounter);

        if (LevelManager.Default.DifficultyCounter == 15 && LevelSettings.TutorialStage < 10)
        {
            LevelSettings.TutorialStage = 10;
        }
    }
}
