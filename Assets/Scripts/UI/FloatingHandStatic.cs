using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FloatingHandStatic : MonoBehaviour
{
    private RectTransform _rt;

    void Start()
    {
        _rt = GetComponent<RectTransform>();
        Anim();
    }

    public void Anim()
    {
        _rt.DOPivotY(0.91f, 0.1f).SetEase(Ease.InOutBounce);
        _rt.DOScale(0.9f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            _rt.DOPivotY(0.72f, 0.15f).SetEase(Ease.InOutBounce);
            _rt.DOScale(1f, 0.15f).SetEase(Ease.InOutBounce).OnComplete(() =>
            {
                Invoke("Anim", 0.5f);
            });
        });
    }
}
