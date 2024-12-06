using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BG.UI.Main;
using System;
using UnityEngine.EventSystems;

public class ScreenManager : MonoBehaviour
{
    #region Singleton
    private static ScreenManager _default;
    public static ScreenManager Default => _default;
    #endregion

    public GameObject FightScreen;
    public GameObject SlotScreen;

    public LevelManager LevelManager;

    public bool IsInSlots;
    public bool SlotsAreVisible;

    //public static bool IsPointerOverGameObject()
    //{
    //    return EventSystem.current.IsPointerOverGameObject();
    //}

    void Awake()
    {
        _default = this;
        LevelManager.Initialize();
        Debug.Log("Tutorial Stage " + LevelSettings.TutorialStage);
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("SlotsOpen") == 1)
        {
            GoToSlotScreen(() => {}, false);
        }
        else
        {
            GoToFightScreen(() => {}, false);
        }
    }

    public void GoToFightScreen(Action action, bool doTransition = true)
    {
        Action a = () =>
        {
            action?.Invoke();
            var cs = FightScreen.GetComponentInChildren<BG.UI.Camera.CameraSystem>(true);
            if (cs)
            {
                cs.gameObject.SetActive(true);
            }
            FightScreen.SetActive(true);
            SlotScreen.SetActive(false);
            SlotMachine.NextMultiplier = 1;
            UIManager.Default.CurrentState = UIState.Start;
            
            //LevelManager.DoNormalLevel();

            SoundtrackPlayer.Default.PlaySoundtrack(LevelSettings.Default.Description.IsBoss);

            SlotsAreVisible = false;
        };

        if (doTransition)
        {
            Transition.Default.DoTransition(a);
        }
        else
        {
            a();
        }

        if (LevelSettings.TutorialStage == 6 && SpinsService.Default.GetSpins() == 0)
        {
            LevelSettings.TutorialStage++;
        }

        PlayerPrefs.SetInt("SlotsOpen", 0);
        IsInSlots = false;

        var button = FindObjectOfType<BigRedButton>();
        if (button)
        {
            button.DisableAutospin();
        }
    }

    public void GoToSlotScreen(Action action, bool doTransition = true)
    {
        Action a = () =>
        {
            action?.Invoke();
            var cs = FightScreen.GetComponentInChildren<BG.UI.Camera.CameraSystem>(true);
            if (cs)
            {
                cs.gameObject.SetActive(false);
            }
            FightScreen.SetActive(true);
            SlotScreen.SetActive(true);
            UIManager.Default.CurrentState = UIState.Lotto;
            
            //LevelManager.DoOpponentLevel();

            SoundtrackPlayer.Default.PlaySoundtrack(false);

            SlotsAreVisible = true;
        };

        if (doTransition)
        {
            Transition.Default.DoTransition(a);
        }
        else
        {
            a();
        }

        if (LevelSettings.TutorialStage == 5)
        {
            LevelSettings.TutorialStage++;
        }

        PlayerPrefs.SetInt("SlotsOpen", 1);
        IsInSlots = true;
    }

    public void GoToAttackScreen(Action action)
    {
        Transition.Default.DoTransition(() =>
        {
            //LevelManager.DoOpponentLevel();

            var cs = FightScreen.GetComponentInChildren<BG.UI.Camera.CameraSystem>(true);
            if (cs)
            {
                cs.gameObject.SetActive(true);
            }

            action?.Invoke();
            FightScreen.SetActive(true);
            SlotScreen.SetActive(false);
            UIManager.Default.CurrentState = UIState.Attack;
        });
    }

    public void GoToTheftScreen(Action action)
    {
        Transition.Default.DoTransition(() =>
        {
            //LevelManager.DoOpponentLevel();

            var cs = FightScreen.GetComponentInChildren<BG.UI.Camera.CameraSystem>(true);
            if (cs)
            {
                cs.gameObject.SetActive(true);
            }
            
            action?.Invoke();
            FightScreen.SetActive(true);
            SlotScreen.SetActive(false);
            UIManager.Default.CurrentState = UIState.Theft;
        });
    }
}
