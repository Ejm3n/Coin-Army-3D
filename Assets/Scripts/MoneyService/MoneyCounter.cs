using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MoneyCounter : MonoBehaviour
{
    public static bool AllowUpdate = true;
    public static float UpdateTimeout;
    public static double DisplayedMoney;
    public static double UpdateDuration = 1;

    public bool ShowReward;

    private TextMeshProUGUI _tmp;

    private bool _scaledUp;

    private double _speed;
    private ulong _speedRef;

    private Vector3 baseScale;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        baseScale = transform.parent.localScale;
    }

    private void Start()
    {
        DisplayedMoney = MoneyService.Default.GetMoney(true);
    }

    private void UpdateText(ref double displayedMoney, ulong currentMoney)
    {

    }

    private void Update()
    {
        if (ShowReward)
        {
            _tmp.text = $"+{MoneyService.AmountToStringTrunicate((ulong)UnitManager.Default.TotalLootYield)} <sprite index=0>";
        }
        else
        {
            if (!AllowUpdate)
            {
                if (UpdateTimeout > 0f)
                {
                    UpdateTimeout -= Time.deltaTime;

                    if (UpdateTimeout <= 0f)
                    {
                        AllowUpdate = true;
                        UpdateTimeout = 0f;
                    }
                }
            }
            
            ulong currentMoney = MoneyService.Default.GetMoney(true);

            if (AllowUpdate && DisplayedMoney != currentMoney)
            {
                if (_speedRef != currentMoney)
                {
                    _speed = System.Math.Abs(currentMoney - DisplayedMoney) / UpdateDuration;
                    _speedRef = currentMoney;
                    UpdateDuration = 1;
                }

                if (DisplayedMoney < currentMoney)
                {
                    DisplayedMoney += _speed * (double)Time.deltaTime;

                    if (DisplayedMoney > currentMoney)
                    {
                        DisplayedMoney = currentMoney;
                    }
                }
                else
                {
                    DisplayedMoney -= _speed * (double)Time.deltaTime;

                    if (DisplayedMoney < currentMoney)
                    {
                        DisplayedMoney = currentMoney;
                    }
                }

                if (!_scaledUp)
                {
                    _scaledUp = true;

                    transform.parent.DOScale(baseScale * 1.1f, 0.2f).SetEase(Ease.InOutBounce);
                }
            }
            else
            {
                if (_scaledUp && transform.parent.localScale.x == 1.1f)
                {
                    _scaledUp = false;

                    transform.parent.DOScale(baseScale, 0.3f).SetEase(Ease.InOutBounce);
                }
            }

            _tmp.text = MoneyService.AmountToStringTrunicate((ulong)DisplayedMoney);
        }
    }
}
