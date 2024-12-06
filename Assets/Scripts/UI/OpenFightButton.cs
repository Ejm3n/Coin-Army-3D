using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OpenFightButton : MonoBehaviour
{
    public UnitSetting[] BuyableUnits;

    public GameObject AlarmIcon;

    public void OnPress()
    {
        if (SlotMachine.IsTransitioning || !SlotMachine.IsReady)
        {
            return;
        }

        ScreenManager.Default.GoToFightScreen(() => { });

        transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    void Update()
    {
        bool canBuy = false;

        foreach (var unit in BuyableUnits)
        {
            if (MoneyService.Default.GetMoney() >= UnitManager.GetUnitPrice(unit))
            {
                canBuy = true;
                break;
            }
        }

        AlarmIcon.SetActive(canBuy);
    }
}
