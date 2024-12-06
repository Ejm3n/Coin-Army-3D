using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Scriptables/StoreDB")]
public class StoreDB : DataHolder
{
    #region Singleton

    private static StoreDB _default;
    public static StoreDB Default => _default;

    #endregion

    [Serializable]
    public class Item
    {
        public string ProductName;

        [Serializable]
        public class PurchaseReward
        {
            public ulong RewardAmount;

            public enum RewardType
            {
                Money,
                Spins
            }

            public RewardType Type;
            public bool DependsOnAttackReward;
        }

        [SerializeField]
        public PurchaseReward[] Rewards;

        public enum PurchaseCurrency
        {
            Money,
            Video
        }
        
        public double Price;
        public PurchaseCurrency BuyWith;
        public int PurchasesPerDay;
        public bool HideIfNotAvailable;
    }

    [SerializeField]
    public Item[] Items;

    public override void Init()
    {
        _default = this;
    }

    public void GiveReward(int index)
    {
        var item = Items[index];

        foreach (var reward in item.Rewards)
        {
            ulong amount = reward.RewardAmount;

            if (reward.DependsOnAttackReward)
            {
                amount *= GameData.Default.AttackReward + GameData.Default.AttackRewardRaise * (ulong)LevelManager.Default.DifficultyCounter;
            }

            switch (reward.Type)
            {
                case Item.PurchaseReward.RewardType.Money:
                    MoneyService.Default.AddMoney(amount);
                    break;
                case Item.PurchaseReward.RewardType.Spins:
                    SpinsService.Default.AddSpins((int)amount, false);
                    break;
            }
        }

        if (item.PurchasesPerDay >= 0)
        {
            PlayerPrefs.SetInt(index + "_BoughtToday", PlayerPrefs.GetInt(index + "_BoughtToday") + 1);
            PlayerPrefs.SetInt(index + "_BoughtDay", GetCurrentDay());
        }
    }

    public bool CanBuy(int index)
    {
        var item = Items[index];

        if (item.PurchasesPerDay < 0)
        {
            return true;
        }

        return PurchasesAvailable(index) > 0;
    }

    public int PurchasesAvailable(int index)
    {
        int lastBoughtDay = PlayerPrefs.GetInt(index + "_BoughtDay");
        int currentDay = GetCurrentDay();

        if (lastBoughtDay != currentDay)
        {
            PlayerPrefs.SetInt(index + "_BoughtToday", 0);
        }
        
        return Items[index].PurchasesPerDay - PlayerPrefs.GetInt(index + "_BoughtToday");
    }

    public Item GetByProductName(string productName)
    {
        foreach (var item in Items)
        {
            if (item.ProductName == productName)
            {
                return item;
            }
        }

        return null;
    }

    public int GetIndex(Item item)
    {
        return Array.IndexOf(Items, item);
    }

    private int GetCurrentDay()
    {
        return (int)((DateTime.Now - new DateTime(2023, 1, 1)).TotalDays * 2.0);
    }
}
