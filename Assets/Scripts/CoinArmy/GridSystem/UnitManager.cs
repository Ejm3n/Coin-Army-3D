using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using BG.UI.Main;

public class UnitManager : MonoBehaviour
{
    #region Singleton

    private static UnitManager _default;
    public static UnitManager Default => _default;

    #endregion

    public static int DamageMultiplier = 1;
    public static float EnemyToughnessMultiplier
    {
        get
        {
            return PlayerPrefs.GetFloat("EnemyToughnessMultiplier", LevelSettings.Default != null ? LevelSettings.Default.Description.NormalEnemyPower : 1f);
        }
        set
        {
            PlayerPrefs.SetFloat("EnemyToughnessMultiplier", value);
        }
    }

    public Grid PlayerGrid;
    public Grid EnemyGrid;

    [NonSerialized]
    public List<Unit> PlayerUnits = new List<Unit>();
    [NonSerialized]
    public List<Unit> EnemyUnits = new List<Unit>();

    [NonSerialized]
    public static List<MoneyLoot> DroppedLoot = new List<MoneyLoot>();
    [NonSerialized]
    public static List<MoneyLoot> LootPool = new List<MoneyLoot>();
    public static Transform PoolParent;
    public ulong TotalLootYield;

    [Serializable]
    private class State
    {
        [SerializeField]
        public List<int> Entries;
    }

    void Awake()
    {
        _default = this;

        if (PoolParent == null)
        {
            PoolParent = new GameObject("GlobalPool").transform;
        }

        LoadState();
    }

    public static void DecreaseEnemyToughness()
    {
        if (LevelSettings.Default == null)
        {
            return;
        }

        var newValue = EnemyToughnessMultiplier - (1f - (LevelSettings.Default.Description.UseCustomEnemyPowerAfterFail ? LevelSettings.Default.Description.EnemyPowerAfterFail : GameData.Default.EnemyPowerAfterFail));

        newValue = Mathf.Max(newValue, LevelSettings.Default.Description.UseCustomEnemyPowerAfterFail ? LevelSettings.Default.Description.MinEnemyPowerAfterFail : GameData.Default.MinEnemyPowerAfterFail);

        if (EnemyToughnessMultiplier == newValue)
        {
            return;
        }

        EnemyToughnessMultiplier = newValue;

        Debug.Log("Enemy power is now " + (UnitManager.EnemyToughnessMultiplier * 100f) + "%");
    }

    public static void ResetEnemyToughness()
    {
        if (LevelSettings.Default != null && !LevelManager.UsesModifiedDifficulty && UnitManager.EnemyToughnessMultiplier != LevelSettings.Default.Description.NormalEnemyPower)
        {
            UnitManager.EnemyToughnessMultiplier = LevelSettings.Default.Description.NormalEnemyPower;
        }

        Debug.Log("Enemy power is now " + (UnitManager.EnemyToughnessMultiplier * 100f) + "%");
    }

    public void SaveHP()
    {
        PlayerPrefs.SetInt("AttackedLevel", LevelManager.Default.DifficultyCounter);

        for (int i = 0; i < EnemyUnits.Count; i++)
        {
            if (EnemyUnits[i] == null)
            {
                continue;
            }

            PlayerPrefs.SetFloat("SavedHP_" + i, EnemyUnits[i].CurrentHP);
        }
    }

    public void LoadHP()
    {
        if (PlayerPrefs.GetInt("AttackedLevel", -1) != LevelManager.Default.DifficultyCounter)
        {
            return;
        }

        for (int i = 0; i < EnemyUnits.Count; i++)
        {
            if (EnemyUnits[i] == null)
            {
                continue;
            }

            EnemyUnits[i].CurrentHP = PlayerPrefs.GetFloat("SavedHP_" + i);

            if (EnemyUnits[i].CurrentHP <= 0f)
            {
                EnemyUnits[i].FastForwardDeath();
            }
        }
    }

    public void SaveState()
    {
        List<UnitSetting> setup = new List<UnitSetting>();

        for (int i = 0; i < PlayerGrid.Cells.Count; i++)
        {
            if (PlayerGrid.IsCellOccupied(i, out Unit overlap))
            {
                setup.Add(overlap.Description);
            }
            else
            {
                setup.Add(null);
            }
        }

        State setupResult = new State();
        setupResult.Entries = SerializeSetup(setup);
        PlayerPrefs.SetString("PlayerSetup", JsonUtility.ToJson(setupResult));
    }

    public static List<UnitSetting> GetSavedState()
    {
        State baseSetup = new State();
        baseSetup.Entries = SerializeSetup(GameData.Default.InitialPlayerSetup);
        State savedSetup = JsonUtility.FromJson<State>(PlayerPrefs.GetString("PlayerSetup", JsonUtility.ToJson(baseSetup)));
        return DeserializeSetup(savedSetup.Entries);
    }

    public static void SetState(List<UnitSetting> setup)
    {
        State setupResult = new State();
        setupResult.Entries = SerializeSetup(setup);
        PlayerPrefs.SetString("PlayerSetup", JsonUtility.ToJson(setupResult));
    }

    void LoadState()
    {
        ResetState();

        State baseSetup = new State();
        baseSetup.Entries = SerializeSetup(GameData.Default.InitialPlayerSetup);
        State savedSetup = JsonUtility.FromJson<State>(PlayerPrefs.GetString("PlayerSetup", JsonUtility.ToJson(baseSetup)));

        LoadSetup(DeserializeSetup(savedSetup.Entries), false);
    }

    static List<int> SerializeSetup(List<UnitSetting> setup)
    {
        return setup.Select(x => GameData.Default.AllUnits.IndexOf(x)).ToList();
    }

    static List<UnitSetting> DeserializeSetup(List<int> setup)
    {
        return setup.Select(x => x == -1 ? null : GameData.Default.AllUnits[Mathf.Min(x, GameData.Default.AllUnits.Count - 1)]).ToList();
    }

    public void LoadSetup(List<UnitSetting> setup, bool isEnemy)
    {
        if (!isEnemy && LevelManager.BlockedFromProceeding)
        {
            List<UnitSetting> closeRangeUnits = new List<UnitSetting>();
            List<UnitSetting> longRangeUnits = new List<UnitSetting>();

            int closeRangeInBack = 0;
            int longRangeInFront = 0;

            for (int i = 0; i < setup.Count; i++)
            {
                if (setup[i] == null)
                {
                    continue;
                }

                if (setup[i].IsAnimal)
                {
                    closeRangeUnits.Add(setup[i]);
                }
                else
                {
                    longRangeUnits.Add(setup[i]);
                }

                if (i < 5)
                {
                    if (!setup[i].IsAnimal)
                    {
                        longRangeInFront++;
                    }
                }
                else if (i > 9)
                {
                    if (setup[i].IsAnimal)
                    {
                        closeRangeInBack++;
                    }
                }
            }

            if (closeRangeInBack > 0 && longRangeInFront > 0)
            {
                for (int i = 0; i < setup.Count; i++)
                {
                    setup[i] = null;
                }

                for (int i = 0; i < setup.Count; i++)
                {
                    if (i < 5)
                    {
                        if (closeRangeUnits.Count > 0)
                        {
                            setup[i] = closeRangeUnits[0];
                            closeRangeUnits.RemoveAt(0);
                        }
                    }
                    else if (i > 9)
                    {
                        if (longRangeUnits.Count > 0)
                        {
                            setup[i] = longRangeUnits[0];
                            longRangeUnits.RemoveAt(0);
                        }
                    }
                }

                if (closeRangeUnits.Count > 0 || longRangeUnits.Count > 0)
                {
                    for (int i = 0; i < setup.Count; i++)
                    {
                        if (i < 5 || i > 9)
                        {
                            continue;
                        }
                        else if (UnityEngine.Random.Range(0, 2) == 0)
                        {
                            if (closeRangeUnits.Count > 0)
                            {
                                setup[i] = closeRangeUnits[0];
                                closeRangeUnits.RemoveAt(0);
                            }
                        }
                        else
                        {
                            if (longRangeUnits.Count > 0)
                            {
                                setup[i] = longRangeUnits[0];
                                longRangeUnits.RemoveAt(0);
                            }
                        }
                    }
                }
            }
        }

        int minLevel = Mathf.Min((LevelManager.Default.DifficultyCounter + 1) / GameData.Default.UnitRaiseEveryNLevels, 6);

        for (int i = 0; i < setup.Count; i++)
        {
            if (setup[i] == null)
            {
                continue;
            }

            var setting = setup[i];

            if (!isEnemy && setting.UnitLevel != -1)
            {
                while (setting.UnitLevel < minLevel && setting.TurnInto != null)
                {
                    setting = setting.TurnInto;
                }
            }

            SpawnUnit(setting.Prefab, i, isEnemy, false);
        }
    }

    public Unit SpawnUnit(GameObject prefab, int targetCell, bool isEnemy, bool doFanfare)
    {
        Grid targetGrid = isEnemy ? EnemyGrid : PlayerGrid;
        var unitList = isEnemy ? EnemyUnits : PlayerUnits;

        var newUnit = Instantiate(prefab, transform).GetComponent<Unit>();
        newUnit.IsEnemy = isEnemy;
        newUnit.ParentGrid = targetGrid;
        newUnit.GridCellIndex = targetCell;
        newUnit.transform.position = targetGrid.GetCellPosition(targetCell);
        if (isEnemy)
        {
            newUnit.transform.Rotate(0f, 180f, 0f);
        }
        unitList.Add(newUnit);

        bool newCard = isEnemy ? false : CardsService.Default.AddCard(GameData.Default.AllUnits.IndexOf(newUnit.Description));

        if (doFanfare)
        {
            newUnit.DoSpawnAnimation(newCard);

            if (newCard)
            {
                UIManager.Default.GetPanel(UIState.Start).GetComponent<StartPanel>().OpenNewCardPanel(newUnit);
            }
        }

        return newUnit;
    }

    public static void MaybeResetUnitPrice()
    {
        if (PlayerPrefs.GetInt("LastPriceResetLevel") == LevelManager.Default.DifficultyCounter)
        {
            return;
        }

        bool doReset = false;

        if (LevelManager.Default.DifficultyCounter > 18)
        {
            doReset = (LevelManager.Default.DifficultyCounter - 18) % 10 == 0;
        }
        else if (LevelManager.Default.DifficultyCounter == 18)
        {
            doReset = true;
        }

        if (!doReset)
        {
            return;
        }

        PlayerPrefs.SetInt("LastPriceResetLevel", LevelManager.Default.DifficultyCounter);

        Debug.Log("reset prices");

        foreach (var unit in GameData.Default.AllUnits)
        {
            ulong price = GetUnitPrice(unit);
            if (price > GameData.Default.UnitPriceResetAmount)
            {
                price = GameData.Default.UnitPriceResetAmount;
            }
            int a, b;
            MoneyService.Decombine(price, out a, out b);
            PlayerPrefs.SetInt("UnitPrice1_" + unit.name, a);
            PlayerPrefs.SetInt("UnitPrice2_" + unit.name, b);
        }
    }

    public static ulong GetUnitPrice(UnitSetting unitSetting)
    {
        int a, b;
        MoneyService.Decombine(unitSetting.Price, out a, out b);
        return MoneyService.Combine(PlayerPrefs.GetInt("UnitPrice1_" + unitSetting.name, a), PlayerPrefs.GetInt("UnitPrice2_" + unitSetting.name, b));
    }

    public static void SetUnitPrice(UnitSetting unitSetting, ulong price)
    {
        int a, b;
        MoneyService.Decombine(price, out a, out b);
        PlayerPrefs.SetInt("UnitPrice1_" + unitSetting.name, a);
        PlayerPrefs.SetInt("UnitPrice2_" + unitSetting.name, b);
    }

    public void BuyUnit(UnitSetting setting, UnitSetting baseSetting)
    {
        ulong price = GetUnitPrice(baseSetting);

        if (LevelSettings.TutorialStage >= 0)
        {
            MoneyService.Default.SpendMoney(price);
            
            int boughtUnits = PlayerPrefs.GetInt("BoughtUnits_" + baseSetting.name, 1);

            price += (ulong)(GameData.Default.UnitPriceRaiseMultiplier * boughtUnits);

            if (LevelSettings.TutorialStage == 7 && PlayerPrefs.GetInt("RaisedPriceStage7_" + baseSetting.name) == 0)
            {
                PlayerPrefs.SetInt("RaisedPriceStage7_" + baseSetting.name, 1);
                price = (ulong)(price * GameData.Default.PriceMultiplierAfterFirstSpin);
                Debug.Log("Multiplied price by " + GameData.Default.PriceMultiplierAfterFirstSpin);
            }

            SetUnitPrice(baseSetting, price);
            PlayerPrefs.SetInt("BoughtUnits_" + baseSetting.name, boughtUnits + 1);
        }

        int cell = PlayerGrid.PickAnyFreeCell(setting.IsAnimal);

        if (PlayerGrid.IsEmpty())
        {
            cell = 12;
        }
        else if (LevelSettings.TutorialStage == 1)
        {
            cell = 11;
        }

        SpawnUnit(setting.Prefab, cell, false, true);

        SaveState();
    }

    public void DropLoot(Unit unit)
    {
        ulong totalReward = unit.Description.BaseRewardForKill + unit.Description.RewardForKillRaise * (ulong)LevelManager.Default.DifficultyCounter;
        TotalLootYield += totalReward;
        if (LootPool.Count > 0)
        {
            var loot = LootPool[LootPool.Count - 1];
            loot.transform.position = unit.transform.position;
            loot.transform.rotation = Quaternion.identity;
            LootPool.RemoveAt(LootPool.Count - 1);
            loot.Money = totalReward;
            loot.enabled = true;
            DroppedLoot.Add(loot);
        }
        else
        {
            var loot = Instantiate(GameData.Default.MoneyLootPrefab, unit.transform.position, Quaternion.identity, PoolParent).GetComponent<MoneyLoot>();
            loot.Money = totalReward;
            DroppedLoot.Add(loot);
        }
    }

    public void ResetState()
    {
        foreach (var unit in PlayerUnits)
        {
            Destroy(unit.gameObject);
        }
        foreach (var unit in EnemyUnits)
        {
            Destroy(unit.gameObject);
        }
        foreach (var loot in DroppedLoot)
        {
            loot.gameObject.SetActive(false);
            LootPool.Add(loot);
        }
        PlayerUnits.Clear();
        EnemyUnits.Clear();
        DroppedLoot.Clear();
        TotalLootYield = 0;
        if (LevelSettings.Default.IsOpponentLocation)
        {
            LoadSetup(OpponentService.Default.Description.UnitsSetup, true);
        }
        else
        {
            LoadSetup(LevelSettings.Default.Description.UnitsSetup, true);

            LoadHP();
        }
    }

    public void OnUnitDied()
    {
        if (AreAllUnitsDead(EnemyUnits))
        {
            LevelSettings.Default.Win();
        }
        else if (AreAllUnitsDead(PlayerUnits))
        {
            LevelSettings.Default.Lose();
        }
    }

    bool AreAllUnitsDead(List<Unit> units)
    {
        foreach (var unit in units)
        {
            if (!unit.IsDead)
            {
                return false;
            }
        }

        return true;
    }
}
