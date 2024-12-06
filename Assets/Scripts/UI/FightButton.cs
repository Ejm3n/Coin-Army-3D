using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FightButton : MonoBehaviour
{
    public void OnPress()
    {
        if (UnitManager.Default == null || LevelSettings.TutorialStage == 5)
        {
            return;
        }
        
        LevelSettings.Default.StartFighting();

        transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
