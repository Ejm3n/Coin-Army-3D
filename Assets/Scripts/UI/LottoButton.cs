using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LottoButton : MonoBehaviour
{
    public void OnPress()
    {
        ScreenManager.Default.GoToSlotScreen(() => { });

        transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
