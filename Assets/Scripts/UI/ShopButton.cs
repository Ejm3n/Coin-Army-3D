using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BG.UI.Main;
using DG.Tweening;

public class ShopButton : MonoBehaviour
{
    public void OnPress()
    {
        if (SlotMachine.IsTransitioning)
        {
            return;
        }
        
        UIManager.Default.CurrentState = UIState.Store;

        transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
