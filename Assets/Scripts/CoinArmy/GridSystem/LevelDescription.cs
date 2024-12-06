using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Scriptables/Level")]
public class LevelDescription : ScriptableObject
{
    public List<UnitSetting> UnitsSetup;
    public bool IsBoss;
    public bool DontGiveSpins;
    public int LocationProgress;
    public int LocationLength;
    public double EnemyRewardMultiplierWin = 1.0;
    public double EnemyRewardMultiplierFail = 1.0;
    public ulong AdditionalFailReward;
    public bool UseCustomEnemyPowerAfterFail;
    public float EnemyPowerAfterFail;
    public float MinEnemyPowerAfterFail = 0.1f;
    public float NormalEnemyPower = 1f;

    [Header("Testing"), SerializeField]
    public TestSetupData TestSetup = new TestSetupData();

    [Serializable]
    public class TestSetupData
    {
        public bool SetUp;
        public ulong MoneyAmount;
        public int SpinsAmount;
        public float TutorialStage;
        public List<UnitSetting> PlayerUnits;
        public bool BlockedFromProceeding;
        public bool UsesModifiedDifficulty;
        public bool UsesModifiedDifficulty2;
        public int SpinsCount;
        public ulong Price1;
        public int BoughtCount1;
        public ulong Price2;
        public int RaisedPriceStage7_1;
        public int BoughtCount2;
        public int RaisedPriceStage7_2;
        public int CurrentMissionProgress;
        public int CurrentMission;
        public int ShieldCount;
        public int BoughtRangeUnitsOnLevel8;
        public int DidTheft;
    }
}
