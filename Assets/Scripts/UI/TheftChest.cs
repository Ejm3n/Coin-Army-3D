using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BG.UI.Main;
using System;

public class TheftChest : MonoBehaviour, IMouseEventProxyTarget
{
    public TheftPanel panel;

    public Animator OpeningAnimator;

    public ParticleSystem ChestOpenParticles;

    [NonSerialized]
    public bool _isOpen;

    [NonSerialized]
    public bool Result;

    [NonSerialized]
    public float RewardPercent;

    void Start()
    {
        panel.RequestMouseProxy(this);
    }

    public bool Exists()
    {
        return this != null && gameObject != null;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector2 GetSize()
    {
        return new Vector2(200f, 200f);
    }

    public void OnPointerDown()
    {
        if (_isOpen)
        {
            return;
        }

        _isOpen = true;

        OpeningAnimator.SetBool("IsOpen", true);

        ChestOpenParticles.Play();

        SoundHolder.Default.PlayFromSoundPack("Chest", allowPitchShift: false);

        int option = UnityEngine.Random.Range(0, panel.UnclaimedPrizes.Count == 4 ? 3 : panel.UnclaimedPrizes.Count);
        int prize = panel.UnclaimedPrizes[option];
        panel.UnclaimedPrizes.RemoveAt(option);

        switch (prize)
        {
            case 0:
                Result = true;
                RewardPercent = GameData.Default.TheftRewardPercent1;
                break;
            case 1:
                Result = true;
                RewardPercent = GameData.Default.TheftRewardPercent2;
                break;
            case 2:
                Result = true;
                RewardPercent = GameData.Default.TheftRewardPercent3;
                break;
            case 3:
                Result = false;
                RewardPercent = 0f;
                break;
        }

        panel.OnTicketPress(Array.IndexOf(panel._spawnedChests, this));
    }

    public void OnPointerDrag()
    {

    }

    public void OnPointerUp()
    {

    }
}
