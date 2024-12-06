using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialArrow : MonoBehaviour
{
    public RectTransform RT;

    private Vector2 _startPosition;

    void Start()
    {
        _startPosition = RT.anchoredPosition;
    }

    void Update()
    {
        RT.anchoredPosition = _startPosition - Vector2.up * Mathf.Sin(Time.time * 5f) * 10f;
    }
}
