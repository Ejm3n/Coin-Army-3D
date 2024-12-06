using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BG.UI.Main;

public class BuyButton : MonoBehaviour
{
    public UnitSetting[] BuyTargets;
    public Image BG;
    public TextMeshProUGUI PriceText;
    public bool IsRangeButton;
    public CanvasGroup RegularContent;
    public CanvasGroup NoSpaceContent;

    private float _anim;

    private Material _mat;

    void Start()
    {
        _mat = new Material(BG.material);
        BG.material = _mat;
    }

    void OnDestroy()
    {
        Destroy(_mat);
    }

    UnitSetting GetCurrentTarget()
    {
        return BuyTargets[Mathf.Min((LevelManager.Default.DifficultyCounter + 1) / GameData.Default.UnitRaiseEveryNLevels, BuyTargets.Length - 1)];
    }

    void Update()
    {
        if (UnitManager.Default == null)
        {
            return;
        }

        PriceText.text = string.Format(Language.Text("MoneyAmount"), MoneyService.AmountToStringTrunicate(UnitManager.GetUnitPrice(BuyTargets[0])));

        bool hasFreeSpace = UnitManager.Default.PlayerGrid.HasFreeSpace();

        _anim = Mathf.MoveTowards(_anim, MoneyService.Default.GetMoney() >= UnitManager.GetUnitPrice(BuyTargets[0]) && hasFreeSpace ? 1f : 0f, Time.deltaTime * 5f);
        _mat.SetFloat("_Desaturation", 1f - _anim);

        RegularContent.alpha = Mathf.MoveTowards(RegularContent.alpha, hasFreeSpace ? 1f : 0f, Time.deltaTime * 5f);
        NoSpaceContent.alpha = Mathf.MoveTowards(NoSpaceContent.alpha, hasFreeSpace ? 0f : 1f, Time.deltaTime * 5f);
    }

    public void OnPress()
    {
        if (UnitManager.Default == null)
        {
            return;
        }

        bool enoughMoney = MoneyService.Default.GetMoney() >= UnitManager.GetUnitPrice(BuyTargets[0]);

        if (!enoughMoney)
        {
            UIManager.Default.GetPanel(UIState.Start).GetComponent<StartPanel>().ShowNotEnoughMoney();
            return;
        }

        if (!UnitManager.Default.PlayerGrid.HasFreeSpace())
        {
            return;
        }

        UnitManager.Default.BuyUnit(GetCurrentTarget(), BuyTargets[0]);

        SoundHolder.Default.PlayFromSoundPack("BuyUnit");
        
        transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        if (LevelSettings.TutorialStage == 1)
        {
            LevelSettings.TutorialStage++;

            if (UnitManager.Default.PlayerUnits[0].GridCellIndex < 5)
            {
                LevelSettings.TutorialStage++;
            }
        }

        if (LevelSettings.TutorialStage == -9)
        {
            LevelSettings.TutorialStage++;
        }

        LevelManager.BlockedFromProceeding = false;

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");

        if (IsRangeButton)
        {
            if (LevelManager.Default.DifficultyCounter == 15)
            {
                PlayerPrefs.SetInt("BoughtRangeUnitsOnLevel8", PlayerPrefs.GetInt("BoughtRangeUnitsOnLevel8") + 1);
            }
        }
    }
}
