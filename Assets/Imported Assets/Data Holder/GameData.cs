using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Scriptables/GameData")]
public class GameData : DataHolder
{
    #region Singleton

    private static GameData _default;
    public static GameData Default => _default;

    #endregion

    [Header("Gameplay")]
    public ulong initialMoneyAmount = 200;
    public int initialSpinsAmount = 10;
    public int maxSpinsAmount = 25;
    public int initialShieldAmount = 0;
    public int maxShields = 3;
    public float GameSpeed = 1;
    [Space]
    public bool DisableTutorial;
    public bool DisableAdditionalHands;
    public bool AlwaysAllowToFight;
    public bool DisableShop;
    [Space]
    public bool UseTestSetup;
    public bool RecordTestSetup;
    [Space]
    public List<UnitSetting> InitialPlayerSetup;
    [Space]
    public List<UnitSetting> AllUnits;
    public GameObject MoneyLootPrefab;
    public double UnitPriceRaiseMultiplier = 11;
    public int UnitRaiseEveryNLevels = 21;
    public ulong UnitPriceResetAmount = 250000;
    [Space]
    public bool useCameraAngles;

    [Space]
    public bool showPromoCursor;
    public bool doPromoMerging;
    public bool doPromoDamageOnEnemy;
    public bool doPromoDamageOnPlayer;

    [Header("UI")]
    public Color UIColor;

    [Serializable]
    public enum BallType
    {
        Shield = 0,
        Power = 1,
        Money = 2,
        Gold = 3,
        Damage = 4,
        Ticket = 5,
        Heart = 6,
        Nothing = -1
    }

    [Serializable]
    public enum RewardType
    {
        Money = 0,
        Spins = 1,
        Attack = 2,
        Defend = 3,
        Theft = 4,
        Nothing = 5
    }

    [Serializable]
    public class LottoCombo
    {
        public string Name;
        public GameData.BallType[] Combination;
        public float Probability;
        public float ProbabilityRaise;
        public float MaxProbability;
        [SerializeField]
        public RewardType RewardType;
        public ulong RewardAmount;
        public ulong RewardRaise;
        public string AnimationName;
        public bool Enabled;

        public float GetCurrentProbability()
        {
            return PlayerPrefs.GetFloat("Probability_" + Name, Probability);
        }

        public void UpdateProbability(bool successfulRoll)
        {
            if (successfulRoll)
            {
                PlayerPrefs.SetFloat("Probability_" + Name, Probability);
            }
            else
            {
                PlayerPrefs.SetFloat("Probability_" + Name, Mathf.Min(GetCurrentProbability() + ProbabilityRaise, MaxProbability));
            }
        }
    }

    [Serializable]
    public class LottoBallReward
    {
        public string Name;
        public BallType BallType;
        [SerializeField]
        public RewardType RewardType;
        public ulong RewardAmount;
        public ulong RewardRaise;
        public bool Enabled;
    }

    [Header("Lotto")]
    public float CombinationProbability;
    public float CombinationProbabilityRaise;
    public float MaxCombinationProbability;
    public bool FakeFirstSpins;

    [Serializable]
    public class FakeCombination
    {
        public BallType FirstBall;
        public BallType SecondBall;
        public BallType ThirdBall;
    }

    [SerializeField]
    public FakeCombination[] FakeFirstSpinsValues;
    [Space]
    public bool UseTestCombination;
    public FakeCombination TestCombination;
    [Space]
    [SerializeField]
    public LottoCombo[] LottoCombinations;
    [SerializeField]
    public LottoBallReward[] LottoRewards;

    [Space]
    public int SpinsRewardForPassedLevel;
    public int SpinsRewardForPassedLocation;

    [Space]
    public float SlotMachineMinDelay;
    public float SlotMachineMaxDelay;

    [Space]
    public ulong AttackReward;
    public ulong AttackRewardRaise;
    public double FailedAttackPercent;

    [Space]
    public float TheftRewardPercent1 = 0.7f;
    public float TheftRewardPercent2 = 0.15f;
    public float TheftRewardPercent3 = 0.15f;

    [Space]
    public double NextSpinTimer = 3600;
    public int NextSpinAmount = 5;

    [Space]
    public float EnemyPowerAfterFail = 0.5f;
    public float MinEnemyPowerAfterFail = 0.1f;

    [Space]
    public float PriceMultiplierAfterFirstSpin = 2f;

    [Space]
    public int MissionProgressOnJackpot = 10;

    [Serializable]
    public class Mission
    {
        public BallType TargetBallType;
        public int TargetAmount;
        public RewardType RewardType;
        public ulong RewardAmount;
        public string MissionDescription;
    }

    [SerializeField]
    public Mission[] Missions;

    [Space]
    public Color TextAccentColor;

    [Space]
    public OpponentDescription[] AllOpponents;


    [Header("Localization")]
    public LanguageData[] AllLanguages;
    public int CurrentLanguage;

    [Header("Ads")]
    public int AdsSpinsAmount;

    [Header("IAP")]
    public ulong IAPMoneyReward;
    public int IAPSpinsReward;
    public double IAPPrice;

    public override void Init()
    {
        _default = this;
    }
}
