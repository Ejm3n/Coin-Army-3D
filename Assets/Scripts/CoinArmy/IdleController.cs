using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IdleController : MonoBehaviour
{
    public static double NextSpinTimer;
    public static bool NextSpinTimerActive;

    private bool _wasPaused;
    private double _pauseTime;

    private void Start()
    {
        OnResume();
    }

    private void Update()
    {
        NextSpinTimerActive = SpinsService.Default.GetSpins() < GameData.Default.maxSpinsAmount;

        if (NextSpinTimerActive)
        {
            NextSpinTimer -= Time.deltaTime;

            if (NextSpinTimer <= 0.0)
            {
                NextSpinTimer += GameData.Default.NextSpinTimer;
                SpinsService.Default.AddSpins(GameData.Default.NextSpinAmount, true);
            }
        }
        else
        {
            NextSpinTimer = GameData.Default.NextSpinTimer;
        }

        OnPause();
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus)
            OnResume();
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause)
            OnResume();
        PlayerPrefs.Save();
    }

    private static double Combine(int a, int b)
    {
        uint ua = (uint)a;
        ulong ub = (uint)b;
        return BitConverter.ToDouble(BitConverter.GetBytes(ub << 32 | ua), 0);
    }
    private static void Decombine(double c2, out int a, out int b)
    {
        ulong c = BitConverter.ToUInt64(BitConverter.GetBytes(c2), 0);
        a = (int)(c & 0xFFFFFFFFUL);
        b = (int)(c >> 32);
    }

    public static double GetCurrentTime()
    {
        return (DateTime.Now - new DateTime(2022, 01, 01)).TotalSeconds;
    }

    private void OnPause()
    {
        _wasPaused = true;
        _pauseTime = GetCurrentTime();
        PlayerPrefs.SetInt("WasPaused", 1);
        Decombine(_pauseTime, out int dec1, out int dec2);
        PlayerPrefs.SetInt("PauseTime1", dec1);
        PlayerPrefs.SetInt("PauseTime2", dec2);
        Decombine(NextSpinTimer, out dec1, out dec2);
        PlayerPrefs.SetInt("NextSpinTimer1", dec1);
        PlayerPrefs.SetInt("NextSpinTimer2", dec2);
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void OnResume()
    {
        _wasPaused = PlayerPrefs.GetInt("WasPaused") == 1;

        if (!_wasPaused)
        {
            if (NextSpinTimer == 0.0)
            {
                NextSpinTimer = GameData.Default.NextSpinTimer;
            }
            return;
        }

        _pauseTime = Combine(PlayerPrefs.GetInt("PauseTime1"), PlayerPrefs.GetInt("PauseTime2"));
        NextSpinTimer = Combine(PlayerPrefs.GetInt("NextSpinTimer1"), PlayerPrefs.GetInt("NextSpinTimer2"));

        _wasPaused = false;
        PlayerPrefs.SetInt("WasPaused", 0);

        if (SpinsService.Default.GetSpins() >= GameData.Default.maxSpinsAmount)
        {
            return;
        }

        double timePassed = Math.Max(GetCurrentTime() - _pauseTime, 0);

        if (timePassed < 0)
        {
            return;
        }

        if (timePassed > NextSpinTimer)
        {
            int finalSpins = (int)(timePassed / GameData.Default.NextSpinTimer);

            finalSpins *= GameData.Default.NextSpinAmount;

            SpinsService.Default.AddSpins(finalSpins, true);

            NextSpinTimer = timePassed % GameData.Default.NextSpinTimer;
        }
        else
        {
            NextSpinTimer -= timePassed;
        }
    }
}
