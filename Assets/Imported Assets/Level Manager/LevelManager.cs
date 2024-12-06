
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class LevelManager : MonoBehaviour
{
    #region Singletone
    private static LevelManager _default;
    public static LevelManager Default { get => _default; }
    public LevelManager() => _default = this;
    #endregion

    const string PREFS_KEY_LEVEL_ID = "CurrentLevelCount";
    const string PREFS_KEY_LAST_INDEX = "LastLevelIndex";

    const string PREFS_KEY_IS_BONUS = "IsBonus";
    const string PREFS_KEY_BONUS_ID = "CurrentBonusCount";
    const string PREFS_KEY_LAST_BONUS_INDEX = "LastBonusIndex";

    public bool editorMode = false;
    public int CurrentLevelCount => PlayerPrefs.GetInt(PREFS_KEY_LEVEL_ID, 0) + 1;
    public int CurrentLevelIndex;
    public List<Level> Levels = new List<Level>();
    public int CurrentBonusCount => PlayerPrefs.GetInt(PREFS_KEY_BONUS_ID, 0) + 1;
    public bool IsBonus { get => PlayerPrefs.GetInt(PREFS_KEY_IS_BONUS, 0) == 1; set => PlayerPrefs.SetInt(PREFS_KEY_IS_BONUS, value ? 1 : 0); }
    public bool isWin;
    public bool levelIsActive;

    public int DifficultyCounter
    {
        get
        {
            int result = PlayerPrefs.GetInt("DifficultyCounter");
            if (PlayerPrefs.GetInt(PREFS_KEY_LEVEL_ID, 0) < Levels.Count)
            {
                result = PlayerPrefs.GetInt(PREFS_KEY_LEVEL_ID, 0);
            }
            return result;
        }
        set
        {
            PlayerPrefs.SetInt("DifficultyCounter", value);
        }
    }

    private bool _prevWin;

    public bool isBonus;
    public int CurrentBonusIndex;
    public List<Level> Bonus = new List<Level>();
    public Action OnLevelStarted, OnLevelComplete, OnLevelLoad;

    [NonSerialized]
    public bool IsOpponentLevel;

    public static bool BlockedFromProceeding
    {
        get
        {
            return PlayerPrefs.GetInt("BlockedFromProceeding") == 1;
        }
        set
        {
            PlayerPrefs.SetInt("BlockedFromProceeding", value ? 1 : 0);
        }
    }

    public static bool UsesModifiedDifficulty
    {
        get
        {
            return PlayerPrefs.GetInt("NeedsDifficultyChange") == 1;
        }
        set
        {
            PlayerPrefs.SetInt("NeedsDifficultyChange", value ? 1 : 0);
        }
    }

    public static bool UsesModifiedDifficulty2
    {
        get
        {
            return PlayerPrefs.GetInt("NeedsDifficultyChange2") == 1;
        }
        set
        {
            PlayerPrefs.SetInt("NeedsDifficultyChange2", value ? 1 : 0);
        }
    }

    public bool AnalyticsReady;

#if UNITY_EDITOR
    void Awake()
    {
        if (!Application.isPlaying)
        {
            Initialize();
        }
    }
#endif

    public void Initialize()
    {
#if !UNITY_EDITOR
        editorMode = false;

#endif
        if (!editorMode)
        {


            CurrentBonusIndex = PlayerPrefs.GetInt(PREFS_KEY_LAST_BONUS_INDEX);
            CurrentLevelIndex = PlayerPrefs.GetInt(PREFS_KEY_LAST_INDEX);
            isBonus = IsBonus;

            //Debug.LogError($"**** DEBUG LevelManager Awake | CurrentBonusIndex = {CurrentBonusIndex} CurrentLevelIndex = {CurrentLevelIndex} isBonus = {isBonus}");

            if (isBonus)
                SelectBonus(PlayerPrefs.GetInt(PREFS_KEY_LAST_BONUS_INDEX), true);
            else
                SelectLevel(PlayerPrefs.GetInt(PREFS_KEY_LAST_INDEX), true, true, true);
        }
        else
        {
            //Debug.LogError($"**** DEBUG LevelManager Awake | editorMode = true");

            //isBonus = IsBonus;
            PlayerPrefs.SetInt(PREFS_KEY_IS_BONUS, isBonus ? 1 : 0);
            PlayerPrefs.SetInt(PREFS_KEY_LEVEL_ID, CurrentLevelIndex);
            PlayerPrefs.SetInt(PREFS_KEY_BONUS_ID, CurrentBonusIndex);
            isBonus = IsBonus;
            if (isBonus)
                SelectBonus(CurrentBonusIndex, true);
            else
                SelectLevel(CurrentLevelIndex, true, true, true);


        }

#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            if (PlayerPrefs.GetInt("GameStarted") == 0)
            {
                PlayerPrefs.SetInt("GameStarted", 1);
                SendStart(DifficultyCounter);
            }
        }
    }

    private void Update()
    {
        if (isWin != _prevWin)
        {
            _prevWin = isWin;
            if (isWin)
            {
                OnLevelComplete?.Invoke();
                levelIsActive = false;
            }
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt(PREFS_KEY_IS_BONUS, isBonus ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_KEY_LAST_INDEX, CurrentLevelIndex);
        PlayerPrefs.SetInt(PREFS_KEY_LAST_BONUS_INDEX, CurrentBonusIndex);
        //Debug.LogError($"**** DEBUG LevelManager OnDestroy | PREFS_KEY_IS_BONUS = {PlayerPrefs.GetInt(PREFS_KEY_IS_BONUS)} PREFS_KEY_LAST_INDEX = {PlayerPrefs.GetInt(PREFS_KEY_LAST_INDEX)} PREFS_KEY_LAST_BONUS_INDEX = {PlayerPrefs.GetInt(PREFS_KEY_LAST_BONUS_INDEX)} ");

    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt(PREFS_KEY_IS_BONUS, isBonus ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_KEY_LAST_INDEX, CurrentLevelIndex);
        PlayerPrefs.SetInt(PREFS_KEY_LAST_BONUS_INDEX, CurrentBonusIndex);
        //Debug.LogError($"**** DEBUG LevelManager OnApplicationQuit | PREFS_KEY_IS_BONUS = {PlayerPrefs.GetInt(PREFS_KEY_IS_BONUS)} PREFS_KEY_LAST_INDEX = {PlayerPrefs.GetInt(PREFS_KEY_LAST_INDEX)} PREFS_KEY_LAST_BONUS_INDEX = {PlayerPrefs.GetInt(PREFS_KEY_LAST_BONUS_INDEX)} ");

    }


    public void StartLevel()
    {
        levelIsActive = true;
        StartCoroutine(WaitUntilLevelIsReady());
    }

    IEnumerator WaitUntilLevelIsReady()
    {
        yield return 0;
        OnLevelStarted?.Invoke();
    }

    public void NextBonus()
    {
        PlayerPrefs.SetInt(PREFS_KEY_BONUS_ID, (PlayerPrefs.GetInt(PREFS_KEY_BONUS_ID) + 1));
        SelectBonus(CurrentBonusIndex + 1);
    }

    public void NextLevel()
    {
        int spinAmount = 0;
        if (GameData.Default && LevelSettings.Default)
        {
            spinAmount = GameData.Default.SpinsRewardForPassedLevel;
            if (LevelSettings.Default.Description.LocationProgress == LevelSettings.Default.Description.LocationLength)
            {
                spinAmount += GameData.Default.SpinsRewardForPassedLocation;
            }
            if (LevelSettings.Default.Description.DontGiveSpins)
            {
                spinAmount = 0;
            }
            SendComplete(DifficultyCounter);
        }
        DifficultyCounter++;
        PlayerPrefs.SetInt(PREFS_KEY_LEVEL_ID, (PlayerPrefs.GetInt(PREFS_KEY_LEVEL_ID) + 1));
        SelectLevel(CurrentLevelIndex + 1, recordState: true);
        if (SpinsService.Default)
        {
            SpinsService.Default.AddSpins(spinAmount, false);
        }
        if (SoundtrackPlayer.Default && LevelSettings.Default)
        {
            SoundtrackPlayer.Default.PlaySoundtrack(LevelSettings.Default.Description.IsBoss);
        }
        if (GameData.Default && LevelSettings.Default)
        {
            SendStart(DifficultyCounter);
        }
    }

    public void ReloadLevel()
    {
        SelectLevel(CurrentLevelIndex);
    }

    public void ClearListAtIndex(int levelIndex)
    {
        Levels[levelIndex].LevelPrefab = null;
    }
    public void ClearBonusAtIndex(int levelIndex)
    {
        Bonus[levelIndex].LevelPrefab = null;
    }
    public bool GetNextLevel(out Level level)
    {
        if (CurrentLevelIndex + 1 < Levels.Count)
        {
            level = Levels[CurrentLevelIndex + 1];
        }
        else
        {
            level = Levels[0];

        }
        return true;
        //if(CurrentLevelIndex + 1 < Levels.Count)
        //{
        //    level = Levels[CurrentLevelIndex + 1];
        //    return true;
        //}
        //else
        //{
        //    level = null;
        //    return false;
        //}
    }
    public bool GetNextBonus(out Level level)
    {
        if (CurrentBonusIndex + 1 < Bonus.Count)
        {
            level = Bonus[CurrentBonusIndex + 1];

        }
        else
        {
            level = Bonus[0];
        }
        return true;
    }
    public void SelectLevel(int levelIndex, bool indexCheck = true, bool restoreState = false, bool recordState = false)
    {
        isWin = false;
        //Debug.LogError($"**** DEBUG LevelManager SelectLevel | levelIndex = {levelIndex}");

        //if (indexCheck)
        //    levelIndex = GetCorrectedIndex(levelIndex);
        //Debug.LogError($"**** DEBUG LevelManager SelectLevel | levelIndex >= Levels.Count = {levelIndex >= Levels.Count}");

        if (levelIndex >= Levels.Count || levelIndex < 0)
            levelIndex = 0;
        IsBonus = isBonus = false;
        //PlayerPrefs.SetInt(PREFS_KEY_LEVEL_ID, levelIndex);

        if (Levels[levelIndex].LevelPrefab == null)
        {
            Debug.Log("<color=red>There is no prefab attached!</color>");
            return;
        }
        var level = Levels[levelIndex];

        if (level.LevelPrefab)
        {
            SelLevelParams(level);
            CurrentLevelIndex = levelIndex;

        }
        PlayerPrefs.SetInt(PREFS_KEY_IS_BONUS, isBonus ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_KEY_LAST_INDEX, CurrentLevelIndex);
        PlayerPrefs.SetInt(PREFS_KEY_LAST_BONUS_INDEX, CurrentBonusIndex);
        //Debug.LogError($"**** DEBUG LevelManager SelectLevel | PREFS_KEY_IS_BONUS = {PlayerPrefs.GetInt(PREFS_KEY_IS_BONUS)} PREFS_KEY_LAST_INDEX = {PlayerPrefs.GetInt(PREFS_KEY_LAST_INDEX)} PREFS_KEY_LAST_BONUS_INDEX = {PlayerPrefs.GetInt(PREFS_KEY_LAST_BONUS_INDEX)} ");

        if (Application.isPlaying)
        {
            UnitManager.MaybeResetUnitPrice();

            UnitManager.ResetEnemyToughness();

#if UNITY_EDITOR
            if ((!restoreState || !LevelSettings.Default.Description.TestSetup.SetUp) && recordState && GameData.Default.RecordTestSetup)
            {
                Debug.Log("Recorded save state for testing");

                LevelSettings.Default.Description.TestSetup.MoneyAmount = MoneyService.Default.GetMoney();
                LevelSettings.Default.Description.TestSetup.SpinsAmount = SpinsService.Default.GetSpins();
                LevelSettings.Default.Description.TestSetup.TutorialStage = LevelSettings.TutorialStage;
                LevelSettings.Default.Description.TestSetup.PlayerUnits = UnitManager.GetSavedState();
                LevelSettings.Default.Description.TestSetup.BlockedFromProceeding = BlockedFromProceeding;
                LevelSettings.Default.Description.TestSetup.UsesModifiedDifficulty = UsesModifiedDifficulty;
                LevelSettings.Default.Description.TestSetup.UsesModifiedDifficulty2 = UsesModifiedDifficulty2;
                LevelSettings.Default.Description.TestSetup.SpinsCount = PlayerPrefs.GetInt("SpinsCount");
                LevelSettings.Default.Description.TestSetup.Price1 = UnitManager.GetUnitPrice(FindObjectsOfType<BuyButton>(true)[0].BuyTargets[0]);
                LevelSettings.Default.Description.TestSetup.BoughtCount1 = PlayerPrefs.GetInt("BoughtUnits_" + FindObjectsOfType<BuyButton>(true)[0].BuyTargets[0].name);
                LevelSettings.Default.Description.TestSetup.RaisedPriceStage7_1 = PlayerPrefs.GetInt("RaisedPriceStage7_" + FindObjectsOfType<BuyButton>(true)[0].BuyTargets[0].name);
                LevelSettings.Default.Description.TestSetup.Price2 = UnitManager.GetUnitPrice(FindObjectsOfType<BuyButton>(true)[1].BuyTargets[0]);
                LevelSettings.Default.Description.TestSetup.BoughtCount2 = PlayerPrefs.GetInt("BoughtUnits_" + FindObjectsOfType<BuyButton>(true)[1].BuyTargets[0].name);
                LevelSettings.Default.Description.TestSetup.RaisedPriceStage7_2 = PlayerPrefs.GetInt("RaisedPriceStage7_" + FindObjectsOfType<BuyButton>(true)[1].BuyTargets[0].name);
                LevelSettings.Default.Description.TestSetup.CurrentMissionProgress = MissionService.Default.GetCurrentMissionProgress();
                LevelSettings.Default.Description.TestSetup.CurrentMission = PlayerPrefs.GetInt("CurrentMission");
                LevelSettings.Default.Description.TestSetup.ShieldCount = ShieldsService.Default.GetShields();
                LevelSettings.Default.Description.TestSetup.BoughtRangeUnitsOnLevel8 = PlayerPrefs.GetInt("BoughtRangeUnitsOnLevel8");
                LevelSettings.Default.Description.TestSetup.DidTheft = PlayerPrefs.GetInt("DidTheft");
                LevelSettings.Default.Description.TestSetup.SetUp = true;
                UnityEditor.EditorUtility.SetDirty(LevelSettings.Default.Description);
            }
            else if (
                restoreState &&
                GameData.Default.UseTestSetup &&
                LevelSettings.Default.Description.TestSetup.SetUp
                )
            {
                Debug.Log("Restored save state for testing");

                MoneyService.Default.SetMoney(LevelSettings.Default.Description.TestSetup.MoneyAmount);
                SpinsService.Default.SetSpins(LevelSettings.Default.Description.TestSetup.SpinsAmount);
                LevelSettings.TutorialStage = LevelSettings.Default.Description.TestSetup.TutorialStage;
                UnitManager.SetState(LevelSettings.Default.Description.TestSetup.PlayerUnits);
                BlockedFromProceeding = LevelSettings.Default.Description.TestSetup.BlockedFromProceeding;
                UsesModifiedDifficulty = LevelSettings.Default.Description.TestSetup.UsesModifiedDifficulty;
                UsesModifiedDifficulty2 = LevelSettings.Default.Description.TestSetup.UsesModifiedDifficulty2;
                PlayerPrefs.SetInt("SpinsCount", LevelSettings.Default.Description.TestSetup.SpinsCount);
                UnitManager.SetUnitPrice(FindObjectsOfType<BuyButton>(true)[0].BuyTargets[0], LevelSettings.Default.Description.TestSetup.Price1);
                PlayerPrefs.SetInt("BoughtUnits_" + FindObjectsOfType<BuyButton>(true)[0].BuyTargets[0].name, LevelSettings.Default.Description.TestSetup.BoughtCount1);
                PlayerPrefs.SetInt("RaisedPriceStage7_" + FindObjectsOfType<BuyButton>(true)[0].BuyTargets[0].name, LevelSettings.Default.Description.TestSetup.RaisedPriceStage7_1);
                UnitManager.SetUnitPrice(FindObjectsOfType<BuyButton>(true)[1].BuyTargets[0], LevelSettings.Default.Description.TestSetup.Price2);
                PlayerPrefs.SetInt("BoughtUnits_" + FindObjectsOfType<BuyButton>(true)[1].BuyTargets[0].name, LevelSettings.Default.Description.TestSetup.BoughtCount2);
                PlayerPrefs.SetInt("RaisedPriceStage7_" + FindObjectsOfType<BuyButton>(true)[1].BuyTargets[0].name, LevelSettings.Default.Description.TestSetup.RaisedPriceStage7_2);
                PlayerPrefs.SetInt("CurrentMissionProgress", LevelSettings.Default.Description.TestSetup.CurrentMissionProgress);
                PlayerPrefs.SetInt("CurrentMission", LevelSettings.Default.Description.TestSetup.CurrentMission);
                ShieldsService.Default.SetShields(LevelSettings.Default.Description.TestSetup.ShieldCount);
                PlayerPrefs.SetInt("BoughtRangeUnitsOnLevel8", LevelSettings.Default.Description.TestSetup.BoughtRangeUnitsOnLevel8);
                PlayerPrefs.SetInt("DidTheft", LevelSettings.Default.Description.TestSetup.DidTheft);
            }
#endif
        }
    }

    public void SelectBonus(int levelIndex, bool indexCheck = true)
    {
        isWin = false;
        //Debug.LogError($"**** DEBUG LevelManager SelectBonus | levelIndex = {levelIndex}");

        // if (indexCheck)
        //levelIndex = GetCorrectedBonusIndex(levelIndex);
        //PlayerPrefs.SetInt(PREFS_KEY_BONUS_ID, levelIndex);
        //Debug.LogError($"**** DEBUG LevelManager SelectBonus | levelIndex >= Bonus.Count = {levelIndex >= Bonus.Count}");

        if (levelIndex >= Bonus.Count)
            levelIndex = 0;
        IsBonus = isBonus = true;
        if (Bonus[levelIndex].LevelPrefab == null)
        {
            Debug.Log("<color=red>There is no prefab attached!</color>");
            return;
        }

        var level = Bonus[levelIndex];

        if (level.LevelPrefab)
        {
            SelLevelParams(level);
            CurrentBonusIndex = levelIndex;
        }
        PlayerPrefs.SetInt(PREFS_KEY_IS_BONUS, isBonus ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_KEY_LAST_INDEX, CurrentLevelIndex);
        PlayerPrefs.SetInt(PREFS_KEY_LAST_BONUS_INDEX, CurrentBonusIndex);
        //Debug.LogError($"**** DEBUG LevelManager SelectLevel | PREFS_KEY_IS_BONUS = {PlayerPrefs.GetInt(PREFS_KEY_IS_BONUS)} PREFS_KEY_LAST_INDEX = {PlayerPrefs.GetInt(PREFS_KEY_LAST_INDEX)} PREFS_KEY_LAST_BONUS_INDEX = {PlayerPrefs.GetInt(PREFS_KEY_LAST_BONUS_INDEX)} ");

    }
    public void PrevLevel() =>
        SelectLevel(CurrentLevelIndex - 1);

    private int GetCorrectedIndex(int levelIndex)
    {
        if (editorMode)
            return levelIndex > Levels.Count - 1 || levelIndex <= 0 ? 0 : levelIndex;
        else
        {
            int levelId = PlayerPrefs.GetInt(PREFS_KEY_LEVEL_ID);
            if (levelId > Levels.Count - 1)
            {
                if (Levels.Count > 1)
                {
                    while (true)
                    {
                        levelId = UnityEngine.Random.Range(0, Levels.Count);
                        if (levelId != CurrentLevelIndex) return levelId;
                    }
                }
                else return UnityEngine.Random.Range(0, Levels.Count);
            }
            return levelId;
        }
    }
    private int GetCorrectedBonusIndex(int levelIndex)
    {
        if (editorMode)
            return levelIndex > Bonus.Count - 1 || levelIndex <= 0 ? 0 : levelIndex;
        else
        {
            int levelId = PlayerPrefs.GetInt(PREFS_KEY_BONUS_ID);
            if (levelId > Bonus.Count - 1)
            {
                if (Bonus.Count > 1)
                {
                    while (true)
                    {
                        levelId = UnityEngine.Random.Range(0, Bonus.Count);
                        if (levelId != CurrentBonusIndex) return levelId;
                    }
                }
                else return UnityEngine.Random.Range(0, Bonus.Count);
            }
            return levelId;
        }
    }

    public void DoOpponentLevel()
    {
        if (!IsOpponentLevel)
        {
            IsOpponentLevel = true;
            var level = new Level();
            level.LevelPrefab = OpponentService.Default.Description.HomeLocation.GetComponent<LevelSettings>();
            SelLevelParams(level);
        }
    }

    public void DoNormalLevel()
    {
        if (IsOpponentLevel)
        {
            IsOpponentLevel = false;
            SelectLevel(CurrentLevelIndex, true);
        }
    }

    private void SelLevelParams(Level level)
    {
        if (level.LevelPrefab)
        {
            ClearChilds();
#if UNITY_EDITOR
            LevelSettings l;
            if (Application.isPlaying)
            {
                l = Instantiate(level.LevelPrefab, transform);
            }
            else
            {
                l = PrefabUtility.InstantiatePrefab(level.LevelPrefab, transform) as LevelSettings;
            }
            l.Initialize();
            foreach (IEditorModeSpawn child in GetComponentsInChildren<IEditorModeSpawn>())
                child.EditorModeSpawn();
#else
            var l = Instantiate(level.LevelPrefab, transform);
#endif
            //Debug.LogError($"**** DEBUG LevelManager SelLevelParams |");

        }
        OnLevelLoad?.Invoke();
    }

    private void ClearChilds()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject destroyObject = transform.GetChild(i).gameObject;
            DestroyImmediate(destroyObject);
        }
    }



    #region Analitics Events

    public void SendStart(int progression)
    {
        //GAManager.Default.StartCoroutine(SendStartQueue(progression + 1));
    }

    IEnumerator SendStartQueue(int progression)
    {
        yield return new WaitUntil(() => AnalyticsReady);
        Debug.Log("START LEVEL " + progression);
        //SupersonicWisdom.Api.NotifyLevelStarted(progression, null);
    }

    public void SendComplete(int progression)
    {
        //GAManager.Default.StartCoroutine(SendCompleteQueue(progression + 1));
    }

    IEnumerator SendCompleteQueue(int progression)
    {
        yield return new WaitUntil(() => AnalyticsReady);
        Debug.Log("COMPLETE LEVEL " + progression);
        //SupersonicWisdom.Api.NotifyLevelCompleted(progression, null);
    }

    public void SendFail(int progression)
    {
        //GAManager.Default.StartCoroutine(SendFailQueue(progression + 1));
    }

    IEnumerator SendFailQueue(int progression)
    {
        yield return new WaitUntil(() => AnalyticsReady);
        Debug.Log("FAIL LEVEL " + progression);
        //SupersonicWisdom.Api.NotifyLevelFailed(progression, null);
    }

    #endregion
}

[System.Serializable]
public class Level
{
    public LevelSettings LevelPrefab;
}