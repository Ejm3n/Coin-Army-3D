using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PulseAnim : MonoBehaviour
{
    private void Start() 
    {
        transform.DOScale(1.2f, 3f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
