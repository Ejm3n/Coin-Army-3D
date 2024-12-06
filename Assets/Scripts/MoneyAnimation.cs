using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MoneyAnimation : MonoBehaviour
{

    [SerializeField] private RectTransform _moneyTarget;
    [SerializeField] private RectTransform _arcAnchor;
    [SerializeField] private GameObject _moneyImage;
    [Space]
    [SerializeField] private AnimationCurve _scaleOverLifetime;
    [SerializeField] private AnimationCurve _alphaOverLifetime;
    private Queue<GameObject> coins = new Queue<GameObject>();

    public Action onAnimCompleted;

    public void PlayMoneyUpAnim(ulong moneyAmount, Vector3 position, Vector3 end, float scale, Action onComplete, bool worldPosition = true, bool arcTrajectory = false)
    {
        int seed = UnityEngine.Random.Range(0, 10000);

        DoDing();

        GameObject money = default;
        if (coins.Count <= 0)
            money = Instantiate(_moneyImage);
        else
        {
            money = coins.Dequeue();

            money.gameObject.SetActive(true);
        }

        money.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(Language.Text("MoneyAmount"), MoneyService.AmountToString(moneyAmount));

        money.transform.localScale = Vector3.zero;
        money.transform.SetParent(_moneyTarget);
        Vector3 start = money.transform.position = worldPosition ? Camera.main.WorldToScreenPoint(position) : position;
        end = worldPosition ? Camera.main.WorldToScreenPoint(end) : end;
        DOTween.To(
            () => 0f,
            (v) =>
            {
                Vector3 pos, sca;
                pos = Vector2.Lerp(start, end, v);
                sca = Vector3.Lerp(Vector3.zero, Vector3.one * scale,v);
                money.transform.position = pos;
                money.transform.localScale = sca;
            },
            1f, 0.25f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    DOTween.To(
                        () => 1f,
                        (v) =>
                        {
                            Vector3 pos, sca;
                            pos = Vector2.Lerp(start, end, v);
                            sca = Vector3.Lerp(Vector3.zero, Vector3.one * scale, v);
                            money.transform.position = pos;
                            money.transform.localScale = sca;
                        },
                        0f, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
                        {
                            onAnimCompleted?.Invoke();
                            onComplete?.Invoke();
                            money.gameObject.SetActive(false);
                            coins.Enqueue(money);
                            DoDing();
                        });
                });
    }
    public void DoDing()
    {
        
    }
    private Vector2 QuadraticLerp(Vector2 A, Vector2 B, Vector2 C, float t)
    {
        Vector2 AB = Vector2.Lerp(A, B, t);
        Vector2 BC = Vector2.Lerp(B, C, t);
        return Vector2.Lerp(AB, BC, t);
    }
}