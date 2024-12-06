using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class StorePanel : MonoBehaviour
{
    public StoreItemUI[] Items;

    public CanvasGroup ResultCanvasGroup;
    public GameObject OneRewardResultInfo;
    public GameObject TwoRewardsResultInfo;

    public RectTransform[] Spinners;

    public Image[] ResultIcons;
    public TextMeshProUGUI[] ResultTexts;

    public Sprite[] RewardIcons;

    public Transform ResultsCloseButton;

    private bool _resultOverlayOpen;

    void OnEnable()
    {
        foreach (var item in Items)
        {
            item.gameObject.SetActive(item.IsAvailable());
        }
    }

    void Update()
    {
        ResultCanvasGroup.alpha = Mathf.MoveTowards(ResultCanvasGroup.alpha, _resultOverlayOpen ? 1f : 0f, Time.deltaTime * 2f);
        ResultCanvasGroup.interactable = ResultCanvasGroup.alpha > 0f;
        ResultCanvasGroup.blocksRaycasts = ResultCanvasGroup.interactable;

        foreach (var spinner in Spinners)
        {
            spinner.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(Time.time * 90f, 360f));
        }
    }

    public void DoResultScreen(int storeDbIndex)
    {
        if (_resultOverlayOpen)
        {
            return;
        }

        _resultOverlayOpen = true;

        var item = StoreDB.Default.Items[storeDbIndex];

        OneRewardResultInfo.SetActive(item.Rewards.Length == 1);
        TwoRewardsResultInfo.SetActive(item.Rewards.Length > 1);

        if (item.Rewards.Length > 0)
        {
            ulong amount = item.Rewards[0].RewardAmount;

            if (item.Rewards[0].DependsOnAttackReward)
            {
                amount *= GameData.Default.AttackReward + GameData.Default.AttackRewardRaise * (ulong)LevelManager.Default.DifficultyCounter;
            }

            ResultIcons[0].sprite = RewardIcons[(int)item.Rewards[0].Type];
            ResultTexts[0].text = MoneyService.AmountToString(amount);
            ResultIcons[1].sprite = RewardIcons[(int)item.Rewards[0].Type];
            ResultTexts[1].text = MoneyService.AmountToString(amount);

            if (item.Rewards.Length > 1)
            {
                amount = item.Rewards[1].RewardAmount;

                if (item.Rewards[1].DependsOnAttackReward)
                {
                    amount *= GameData.Default.AttackReward + GameData.Default.AttackRewardRaise * (ulong)LevelManager.Default.DifficultyCounter;
                }

                ResultIcons[2].sprite = RewardIcons[(int)item.Rewards[1].Type];
                ResultTexts[2].text = MoneyService.AmountToString(amount);
            }
        }

        OneRewardResultInfo.transform.localScale = Vector3.zero;
        OneRewardResultInfo.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic);

        TwoRewardsResultInfo.transform.localScale = Vector3.zero;
        TwoRewardsResultInfo.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic);

        SoundHolder.Default.PlayFromSoundPack("MissionReward", allowPitchShift: false);
    }

    public void CloseResultScreen()
    {
        if (!_resultOverlayOpen)
        {
            return;
        }

        _resultOverlayOpen = false;

        ResultsCloseButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            ResultsCloseButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
