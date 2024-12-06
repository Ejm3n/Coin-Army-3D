using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StoreItemUI : MonoBehaviour
{
    public StorePanel Panel;
    public int StoreDBIndex;
    public TextMeshProUGUI Reward1;
    public TextMeshProUGUI Reward2;
    public TextMeshProUGUI Price;
    public TextMeshProUGUI AmountLeft;

    public bool IsAvailable()
    {
        return
            (IAPService.Default != null && IAPService.Default.IAPIsAvailable || StoreDB.Default.Items[StoreDBIndex].BuyWith != StoreDB.Item.PurchaseCurrency.Money) &&
            (!StoreDB.Default.Items[StoreDBIndex].HideIfNotAvailable || StoreDB.Default.CanBuy(StoreDBIndex));
    }

    void Update()
    {
        if (Reward1 != null)
        {
            ulong amount = StoreDB.Default.Items[StoreDBIndex].Rewards[0].RewardAmount;

            if (StoreDB.Default.Items[StoreDBIndex].Rewards[0].DependsOnAttackReward)
            {
                amount *= GameData.Default.AttackReward + GameData.Default.AttackRewardRaise * (ulong)LevelManager.Default.DifficultyCounter;
            }

            Reward1.text = MoneyService.AmountToString(amount);
        }

        if (Reward2 != null)
        {
            ulong amount = StoreDB.Default.Items[StoreDBIndex].Rewards[1].RewardAmount;

            if (StoreDB.Default.Items[StoreDBIndex].Rewards[1].DependsOnAttackReward)
            {
                amount *= GameData.Default.AttackReward + GameData.Default.AttackRewardRaise * (ulong)LevelManager.Default.DifficultyCounter;
            }

            Reward2.text = MoneyService.AmountToString(amount);
        }

        if (Price != null)
        {
            Price.text = string.Format(Language.Text("PriceAmount"), MoneyService.AmountToString(StoreDB.Default.Items[StoreDBIndex].Price));
        }

        if (AmountLeft != null)
        {
            int amountLeft = StoreDB.Default.PurchasesAvailable(StoreDBIndex);
            AmountLeft.text = string.Format(Language.Text("AvailableItems"), amountLeft, StoreDB.Default.Items[StoreDBIndex].PurchasesPerDay);
        }
    }

    public void Press()
    {
        if (!StoreDB.Default.CanBuy(StoreDBIndex))
        {
            return;
        }

        if (StoreDB.Default.Items[StoreDBIndex].BuyWith == StoreDB.Item.PurchaseCurrency.Video)
        {
            RewardedAdsService.Default.ShowAd(() =>
            {
                StoreDB.Default.GiveReward(StoreDBIndex);

                if (!IsAvailable())
                {
                    gameObject.SetActive(false);
                }

                Panel.DoResultScreen(StoreDBIndex);
            });
        }
        else
        {
            IAPService.Default.DoPurchase(StoreDB.Default.Items[StoreDBIndex].ProductName, () =>
            {
                StoreDB.Default.GiveReward(StoreDBIndex);

                if (!IsAvailable())
                {
                    gameObject.SetActive(false);
                }

                Panel.DoResultScreen(StoreDBIndex);
            });
            
            transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
            {
                transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
            });

            SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
        }
    }
}
