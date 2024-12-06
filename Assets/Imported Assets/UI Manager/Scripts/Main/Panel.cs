using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BG.UI.Main
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Panel : MonoBehaviour
    {
        [SerializeField] private bool _hideOnStart;
        [SerializeField] private float _animDuration = 0.25f;

        private CanvasGroup _group;
        public float AnimDuration => _animDuration;
        public Action onPanelShow = () => { };
        public Action onPanelHide = () => { };


        public void Awake()
        {
            _group = GetComponent<CanvasGroup>();
        }

        public void Start()
        {
            gameObject.SetActive(!_hideOnStart);
            _group.blocksRaycasts = !_hideOnStart;
            if(!_hideOnStart)
                onPanelShow.Invoke();
        }

        public void ShowPanel()
        {
            gameObject.SetActive(true);
            onPanelShow.Invoke();
            _group.alpha = 0f;
            _group.blocksRaycasts = true;
            //transform.localScale = Vector3.one * 1.2f;
            transform.DOScale(1f, _animDuration);
            DOTween.To(
                () => 0f,
                (v) => _group.alpha = v,
                1f, _animDuration);
        }

        public void HidePanel()
        {
            onPanelHide.Invoke();
            transform.localScale = Vector3.one;
            _group.blocksRaycasts = false;
            //transform.DOScale(1.2f, _animDuration);
            DOTween.To(
                () => 1f,
                (v) => _group.alpha = v,
                0f, _animDuration)
                    .OnComplete(() => gameObject.SetActive(false));
        }
    }
}