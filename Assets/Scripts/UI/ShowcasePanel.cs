using BG.UI.Camera;
using BG.UI.Main;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowcasePanel : MonoBehaviour
{
    [SerializeField] private bool isForBonus;
    [SerializeField] private Button _nextButton, _bonusButton, _skipButton;
    [SerializeField] private TextMeshProUGUI _continueText;
    [SerializeField] private RawImage _showcase;
    [SerializeField] private Image _shineEffect;
    [SerializeField] private Image _unknow;
    [SerializeField] private Transform objectHolder;
    [SerializeField] private TextMeshProUGUI _noThanksButton, _titel;

    private Panel _panel;
    private Sequence _animTween;
    private Tween _shineEffectTween;
    private void Awake()
    {
        _panel = GetComponent<Panel>();
        _panel.onPanelShow += HandleOnPanelShow;
        _panel.onPanelHide += HandleOnPanelHide;
        _nextButton.onClick.AddListener(OnNextButtonClick);
        //_skipButton.onClick.AddListener(OnSkipButtonClick);
        //_bonusButton.onClick.AddListener(OnBonusButtonClick);
    }
    private void OnDestroy()
    {

        _panel.onPanelShow -= HandleOnPanelShow;
        _panel.onPanelHide -= HandleOnPanelHide;
        _nextButton.onClick.RemoveListener(OnNextButtonClick);
        //_skipButton.onClick.RemoveListener(OnSkipButtonClick);
        //_bonusButton.onClick.RemoveListener(OnBonusButtonClick);

    }

    private void HandleOnPanelShow()
    {
        HideButtons();
        Showcase();
    }

    public void SetModel()
    {
        // temp
        try
        {
            Level level;
            if (LevelManager.Default.GetNextLevel(out level))
            {
                _unknow.enabled = false;
                _showcase.enabled = true;
            }
            else
            {
                _showcase.enabled = false;
                _unknow.enabled = true;
            }
        }
        catch
        {
            
        }
    }

    public void SetBonusModel()
    {
        Level level;
        if (LevelManager.Default.GetNextBonus(out level))
        {
            _unknow.enabled = false;
            _showcase.enabled = true;
        }
        else
        {
            _showcase.enabled = false;
            _unknow.enabled = true;
        }
    }
    
    private void Showcase()
    {
        if (objectHolder.childCount > 0)
        {
            for (int i = objectHolder.childCount - 1; i >= 0; i--)
            {
                Destroy(objectHolder.GetChild(i).gameObject);
            }
        }
        if (isForBonus)
        {
            SetBonusModel();
        }
        else
        {
            SetModel();
        }


        _shineEffect.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        _continueText.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        _titel.transform.localScale = Vector3.zero;
        _showcase.transform.localScale = Vector3.zero;
        _shineEffectTween?.Kill();
        _shineEffectTween = _shineEffect.DOColor(new Color(1f, 1f, 1f, 0.2f), 0.5f);

        _animTween?.Kill();
        _animTween = DOTween.Sequence();
        _animTween.Append(DOTween.To(() => 0f, (v) => { }, 0f, _panel.AnimDuration));
        _animTween.Append(_titel.transform.DOScale(1.0f, 1.0f).SetEase(Ease.OutElastic).
            OnComplete(() => {
                _titel.transform.DOScale(1.2f, 3f)
                    .SetEase(Ease.InOutQuad)
                    .SetLoops(-1, LoopType.Yoyo);
            }));
       // _animTween.Append(DOTween.To(() => 0f, (v) => { }, 0f, 0.15f));

        _animTween.Append(_showcase.transform.DOScale(1.0f, 2.0f).SetEase(Ease.OutElastic));
        //_animTween.Append(DOTween.To(() => 0f, (v) => { }, 0f, 0.4f));
        _animTween.OnComplete(ShowButton);
    }
    private void HandleOnPanelHide()
    {
        _showcase.transform.localScale = Vector3.zero;
        HideButtons();
    }
    private void HideButtons()
    {
        _nextButton.transform.localScale = Vector3.zero;
        _bonusButton.transform.localScale = Vector3.zero;
        _nextButton.interactable = false;
        _bonusButton.interactable = false;
        _skipButton.interactable = false;
        _noThanksButton.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }
    private void ShowButton()
    {
        //if (isForBonus)
        //{
        //    _bonusButton.interactable = true;
        //    _skipButton.interactable = true;
        //    _noThanksButton.gameObject.SetActive(true);
        //    _bonusButton.transform.DOScale(1f, 2f).SetEase(Ease.OutElastic);
        //    _noThanksButton.DOColor(new Color(1f, 1f, 1f, 0.5f), 0.25f);
        //}
        //else
        {
            _nextButton.interactable = true;
			_nextButton.transform.localScale = Vector3.one;
            //_nextButton.transform.DOScale(1f, 1f).SetEase(Ease.OutElastic);
            _continueText.DOColor(Color.white, 0.25f).SetEase(Ease.Linear);
        }
    }
    private void OnNextButtonClick()
    {
        //if (!LevelManager.Default.IsBonus && (LevelManager.Default.CurrentLevelCount) % GameData.Default.bonusLevel == 0)
        //{
        //    if (isForBonus)
        //    {
        //        _panel.HidePanel();
        //        System.Action action = () =>
        //        {
        //            LevelManager.Default.NextBonus();
        //            UIManager.Default.CurentState = UIState.Start;
        //            if (CameraSystem.Default)
        //                CameraSystem.Default.CurentState = CameraState.Start;

        //        };
        //        LevelTransitionEffect.Default.DoTransition(action);

        //    }
        //    else
        //    {
        //        UIManager.Default.CurentState = UIState.BonusShowcase;
        //    }
        //}
        //else
        {
            _panel.HidePanel();
            System.Action action = () =>
            {
                LevelManager.Default.NextLevel();
                UIManager.Default.CurrentState = UIState.Start;
                if (CameraSystem.Default)
                    CameraSystem.Default.CurentState = CameraState.Start;

            };
            Transition.Default.DoTransition(action);

        }
    }
    private void OnSkipButtonClick()
    {
        HideButtons();
        _panel.HidePanel();
        System.Action action = () =>
        {
            LevelManager.Default.NextLevel();
            UIManager.Default.CurrentState = UIState.Start;
            if (CameraSystem.Default)
                CameraSystem.Default.CurentState = CameraState.Start;

        };
        Transition.Default.DoTransition(action);
    }
    private void OnBonusButtonClick()
    {
        HideButtons();
        _panel.HidePanel();
        System.Action action = () =>
        {
            LevelManager.Default.NextBonus();
            UIManager.Default.CurrentState = UIState.Start;
            if (CameraSystem.Default)
                CameraSystem.Default.CurentState = CameraState.Start;

        };
        Transition.Default.DoTransition(action);
    }
    private void Update()
    {
        _shineEffect.transform.localRotation *= Quaternion.Euler(Vector3.forward * 90f * Time.deltaTime);
    }
}
