using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Cheats : MonoBehaviour
{
    [Header("Money")]
    public ulong MoneyAmount;
    public bool AddMoney;
    [Header("Spins")]
    public int SpinAmount;
    public bool AddSpins;
    [Header("Progress")]
    public bool ResetProgress;

    void Update()
    {
        if (AddMoney)
        {
            AddMoney = false;
            MoneyService.Default.AddMoney(MoneyAmount);
        }

        if (AddSpins)
        {
            AddSpins = false;
            SpinsService.Default.AddSpins(SpinAmount, false);
        }

        if (ResetProgress)
        {
            ResetProgress = false;
            PlayerPrefs.DeleteAll();
        }
    }
}
