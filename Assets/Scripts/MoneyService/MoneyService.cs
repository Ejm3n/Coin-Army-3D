using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyService : MonoBehaviour
{
    #region Singleton
    private static MoneyService _default;
    public static MoneyService Default => _default;
    #endregion
    public Action OnMoneyChanged;

    private static ulong _temporaryMoney;

    private void Awake()
    {
        _default = this;
    }

    public static ulong PrettyRounding(double amount)
    {
        return (ulong)(Math.Floor(amount / 1000.0) * 1000.0);
    }

    public static string AmountToStringTrunicate(ulong amount)
    {
        if (amount >= 1000000000000000000)
        {
            return (amount / 1000000000000000000.0).ToString("0") + "S";
        }
        if (amount >= 1000000000000000)
        {
            return (amount / 1000000000000000.0).ToString("0") + "Q";
        }
        if (amount >= 1000000000000)
        {
            return (amount / 1000000000000.0).ToString("0") + "T";
        }
        if (amount >= 1000000000)
        {
            return (amount / 1000000000.0).ToString("0") + "B";
        }
        if (amount >= 10000000)
        {
            return (amount / 1000000.0).ToString("0") + "M";
        }
        if (amount >= 1000000)
        {
            return (amount / 1000000.0).ToString("0.##") + "M";
        }
        if (amount >= 10000)
        {
            return (amount / 1000.0).ToString("0") + "K";
        }
        if (amount >= 1000)
        {
            return (amount / 1000.0).ToString("0.##") + "K";
        }
        return amount.ToString();
    }

    public static string AmountToStringTrunicate2(ulong amount)
    {
        if (amount >= 1000000000000000000)
        {
            return (amount / 1000000000000000000.0).ToString("0.##") + "S";
        }
        if (amount >= 1000000000000000)
        {
            return (amount / 1000000000000000.0).ToString("0.##") + "Q";
        }
        if (amount >= 1000000000000)
        {
            return (amount / 1000000000000.0).ToString("0.##") + "T";
        }
        if (amount >= 1000000000)
        {
            return (amount / 1000000000.0).ToString("0.##") + "B";
        }
        if (amount >= 1000000)
        {
            return (amount / 1000000.0).ToString("0.##") + "M";
        }
        if (amount >= 1000)
        {
            return (amount / 1000).ToString() + "K";
        }
        return amount.ToString();
    }

    public static string AmountToString(ulong amount)
    {
        if (amount >= 1000000000000000000)
        {
            return (amount / 1000000000000000000.0).ToString("0.##") + "S";
        }
        if (amount >= 1000000000000000)
        {
            return (amount / 1000000000000000.0).ToString("0.##") + "Q";
        }
        if (amount >= 1000000000000)
        {
            return (amount / 1000000000000.0).ToString("0.##") + "T";
        }
        if (amount >= 1000000000)
        {
            return (amount / 1000000000.0).ToString("0.##") + "B";
        }
        if (amount >= 1000000)
        {
            return (amount / 1000000.0).ToString("0.##") + "M";
        }
        if (amount >= 1000)
        {
            return (amount / 1000.0).ToString("0.##") + "K";
        }
        return amount.ToString();
    }

    public static string AmountToStringTrunicate(double amount)
    {
        var abs = Math.Abs(amount);
        if (abs >= 1000000000000000000)
        {
            return (amount / 1000000000000000000.0).ToString("0") + "S";
        }
        if (abs >= 1000000000000000)
        {
            return (amount / 1000000000000000.0).ToString("0") + "Q";
        }
        if (abs >= 1000000000000)
        {
            return (amount / 1000000000000.0).ToString("0") + "T";
        }
        if (abs >= 1000000000)
        {
            return (amount / 1000000000.0).ToString("0") + "B";
        }
        if (abs >= 10000000)
        {
            return (amount / 1000000.0).ToString("0") + "M";
        }
        if (abs >= 1000000)
        {
            return (amount / 1000000.0).ToString("0.##") + "M";
        }
        if (abs >= 10000)
        {
            return (amount / 1000.0).ToString("0") + "K";
        }
        if (abs >= 1000)
        {
            return (amount / 1000.0).ToString("0.##") + "K";
        }
        return amount.ToString("0");
    }

    public static string AmountToString(double amount)
    {
        var abs = Math.Abs(amount);
        if (abs >= 1000000000000000000)
        {
            return (amount / 1000000000000000000.0).ToString("0.##") + "S";
        }
        if (abs >= 1000000000000000)
        {
            return (amount / 1000000000000000.0).ToString("0.##") + "Q";
        }
        if (abs >= 1000000000000)
        {
            return (amount / 1000000000000.0).ToString("0.##") + "T";
        }
        if (abs >= 1000000000)
        {
            return (amount / 1000000000.0).ToString("0.##") + "B";
        }
        if (abs >= 1000000)
        {
            return (amount / 1000000.0).ToString("0.##") + "M";
        }
        if (abs >= 1000)
        {
            return (amount / 1000.0).ToString("0.##") + "K";
        }
        return amount.ToString("0.##");
    }

    public static ulong Combine(int a, int b)
    {
        uint ua = (uint)a;
        ulong ub = (uint)b;
        return ub << 32 | ua;
    }
    public static void Decombine(ulong c, out int a, out int b)
    {
        a = (int)(c & 0xFFFFFFFFUL);
        b = (int)(c >> 32);
    }

    public ulong GetMoney(bool plusTemp = false)
    {
        ulong initialMoney = GameData.Default ? GameData.Default.initialMoneyAmount : 0;

        int default1;
        int default2;
        Decombine(initialMoney, out default1, out default2);

        int part1 = PlayerPrefs.GetInt("MoneyCount1", default1);
        int part2 = PlayerPrefs.GetInt("MoneyCount2", default2);

        ulong result = Combine(part1, part2);

        if (plusTemp && LevelSettings.Default)
        {
            result += (ulong)(_temporaryMoney * Math.Min(LevelSettings.Default.Description.EnemyRewardMultiplierWin, LevelSettings.Default.Description.EnemyRewardMultiplierFail));
        }

        return result;
    }

    public void SetMoney(ulong money)
    {
        int part1;
        int part2;
        Decombine(money, out part1, out part2);
        PlayerPrefs.SetInt("MoneyCount1", part1);
        PlayerPrefs.SetInt("MoneyCount2", part2);
    }
    
    public void AddMoney(ulong count)
    {
        ulong money;
        // cap the money to avoid overflow
        if (count > (ulong.MaxValue - GetMoney()))
        {
            money = ulong.MaxValue;
        }
        else
        {
            money = GetMoney() + count;
        }
        SetMoney(money);
        OnMoneyChanged?.Invoke();
    }

    public void AddTempMoney(ulong count)
    {
        _temporaryMoney += count;
        OnMoneyChanged?.Invoke();
    }

    public void FlushTempMoney(bool win)
    {
        AddMoney((ulong)(_temporaryMoney * (win ? LevelSettings.Default.Description.EnemyRewardMultiplierWin : LevelSettings.Default.Description.EnemyRewardMultiplierFail)));
        _temporaryMoney = 0;
        OnMoneyChanged?.Invoke();
    }

    public void ResetTempMoney()
    {
        _temporaryMoney = 0;
        OnMoneyChanged?.Invoke();
    }

    public void SpendMoney(ulong count) 
    {
        ulong money = Math.Max(GetMoney() - count, 0);
        SetMoney(money);
        OnMoneyChanged?.Invoke();
    }

    [ContextMenu("Add Money - 100")]
    private void AddMoney100() 
    {
        AddMoney(100);
    }

    [ContextMenu("Add Money - 1K")]
    private void AddMoney1K()
    {
        AddMoney(1000);
    }

    [ContextMenu("Add Money - 10K")]
    private void AddMoney10K()
    {
        AddMoney(10000);
    }

    [ContextMenu("Add Money - 100K")]
    private void AddMoney100K()
    {
        AddMoney(100000);
    }

    [ContextMenu("Add Money - 1M")]
    private void AddMoney1M()
    {
        AddMoney(1000000);
    }

    [ContextMenu("Add Money - 10M")]
    private void AddMoney10M()
    {
        AddMoney(10000000);
    }

    [ContextMenu("Add Money - 100M")]
    private void AddMoney100M()
    {
        AddMoney(100000000);
    }

    [ContextMenu("Add Money - 1B")]
    private void AddMoney1B()
    {
        AddMoney(1000000000);
    }
    
    [ContextMenu("Add Money - 10B")]
    private void AddMoney10B()
    {
        AddMoney(10000000000);
    }

    [ContextMenu("Add Money - 100B")]
    private void AddMoney100B()
    {
        AddMoney(100000000000);
    }

    [ContextMenu("Add Money - 1T")]
    private void AddMoney1T()
    {
        AddMoney(1000000000000);
    }

    [ContextMenu("Add Money - 10T")]
    private void AddMoney10T()
    {
        AddMoney(10000000000000);
    }

    [ContextMenu("Add Money - 100T")]
    private void AddMoney100T()
    {
        AddMoney(100000000000000);
    }
}
