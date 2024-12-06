using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class IdleResults : MonoBehaviour
{
    public TextMeshProUGUI MoneyText;
    public Image BG;

    private bool _isOpen;

    public void Open(ulong moneyAmount)
    {
        MoneyText.text = string.Format(Language.Text("PlusMoneyAmount"), MoneyService.AmountToString(moneyAmount));

        if (_isOpen)
        {
            return;
        }

        _isOpen = true;

        gameObject.SetActive(true);
        BG.gameObject.SetActive(true);

        transform.localScale = Vector3.zero;
        BG.color = new Color(0f, 0f, 0f, 0f);

        transform.DOScale(1f, 0.5f).SetEase(Ease.OutCubic);
        BG.DOColor(new Color(0f, 0f, 0f, 0.5f), 0.5f).SetEase(Ease.OutCubic);
    }

    public void Close()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;

        transform.DOScale(0f, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            if (!_isOpen)
            {
                gameObject.SetActive(false);
            }
        });
        BG.DOColor(new Color(0f, 0f, 0f, 0f), 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            if (!_isOpen)
            {
                BG.gameObject.SetActive(false);
            }
        });
    }
}
