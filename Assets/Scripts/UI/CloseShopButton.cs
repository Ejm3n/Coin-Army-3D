using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BG.UI.Main;
using DG.Tweening;

public class CloseShopButton : MonoBehaviour
{
    public void OnPress()
    {
        UIManager.Default.CurrentState = UIState.Lotto;

        transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
