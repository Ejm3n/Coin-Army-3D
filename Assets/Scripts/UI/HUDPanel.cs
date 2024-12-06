using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BG.UI.Main;
using DG.Tweening;
using BG.UI.Camera;

public class HUDPanel : MonoBehaviour
{
    //[SerializeField] private CanvasGroup _joyStick;
    [SerializeField] private Button _switchButon,_finishButton;
    [SerializeField] private MoneyAnimation _moneyAnimation;
    private Panel _panel;
    private Sequence _buttonAnim;

    private void Awake()
    {
        _panel = GetComponent<Panel>();
        _panel.onPanelShow += HandleOnPanelShow;
        _panel.onPanelHide += HandleOnPanelHide;
        _switchButon.onClick.AddListener(OnSwitchButtomClick);
        _finishButton.onClick.AddListener(OnFinishButtomClick);
        HideButton();

    }
    private void Start()
    {
        //Fuel.Default.OnFuelOver += OnFuelOver;
    }
    private void OnDestroy()
    {

        _panel.onPanelShow -= HandleOnPanelShow;
        _panel.onPanelHide -= HandleOnPanelHide;
        _switchButon.onClick.RemoveListener(OnSwitchButtomClick);
        _finishButton.onClick.RemoveListener(OnFinishButtomClick);
        //Fuel.Default.OnFuelOver -= OnFuelOver;
    }

    private void HandleOnPanelShow()
    {
        HideButton();

        //Fuel.Default.Show(false);
    }
    private void HandleOnPanelHide()
    {
        HideButton();

    }
    private void HideButton()
    {
        _switchButon.interactable = false;
        _finishButton.interactable = false;
        //_switchButon.gameObject.SetActive(false);
        //_finishButton.gameObject.SetActive(false);
        //_joyStick.interactable = true;
        //_joyStick.blocksRaycasts = true;
        _buttonAnim?.Kill();
        _switchButon.transform.localScale = Vector3.one;
        _finishButton.transform.localScale = Vector3.one;
    }
    private void OnFuelOver()
    {
        //if(UIManager.Default.CurentState == UIState.Process)
        //_switchButon.gameObject.SetActive(true);
        //_switchButon.interactable = true;
        //_joyStick.interactable = false;
        //_joyStick.blocksRaycasts = false;
        //Fuel.Default.Hide();
        //Joystick.Default.ForceDisablenJoystick();
        //OnSwitchButtomClick();
        //_buttonAnim?.Kill();
        //_buttonAnim = DOTween.Sequence();
        //_switchButon.transform.localScale = Vector3.zero;
        //_buttonAnim.Append(_switchButon.transform.DOScale(1f, 0.5f).SetEase(Ease.OutCubic));
        //_buttonAnim.OnComplete(() =>
        //{
        //    _buttonAnim?.Kill();
        //    _buttonAnim = DOTween.Sequence();

        //    _buttonAnim.Append(_switchButon.transform.DOScale(0.8f, 0.6f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo));
        //    _buttonAnim.SetLoops(-1, LoopType.Yoyo);

        //});


    }
    private void OnSwitchButtomClick()
    {
        //WeaponSwitcher.Default.Switch();
        _switchButon.interactable = false;
        //_switchButon.gameObject.SetActive(false);

        //_finishButton.gameObject.SetActive(true);
        _finishButton.interactable = true;
        //_joyStick.interactable = true;
        //_joyStick.blocksRaycasts = true;

        _buttonAnim?.Kill();
        _switchButon.transform.localScale = Vector3.one;
        _buttonAnim = DOTween.Sequence();
        _finishButton.transform.localScale = Vector3.zero;
        _buttonAnim.Append(DOTween.To(() => 0f, (v) => { }, 0f, 2.0f));
        _buttonAnim.Append(_finishButton.transform.DOScale(1f, 0.5f).SetEase(Ease.OutCubic));
        _buttonAnim.OnComplete(() =>
        {
            _buttonAnim?.Kill();
            _buttonAnim = DOTween.Sequence();

            _buttonAnim.Append(_finishButton.transform.DOScale(0.8f, 0.6f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo));
            _buttonAnim.SetLoops(-1, LoopType.Yoyo);

        });
    }
    public void ForceFinishVacuum()
    {
        DOTween.To(() => 0f, (v) => { }, 0f, 0.5f).OnComplete(() =>
        {
            //Joystick.Default.ForceDisablenJoystick();
            OnFinishButtomClick();
        });
    }
    private void OnFinishButtomClick()
    {
        ClearOnVaccumEnd();
        HideButton();
        DOTween.To(() => 0f, (v) => { }, 0f, 0.75f).OnComplete(() =>
        {
            //WeaponSwitcher.Default.Hide();
            System.Action action = () =>
            {
                LevelManager.Default.NextLevel();
                UIManager.Default.CurrentState = UIState.Start;
                if (CameraSystem.Default)
                    CameraSystem.Default.CurentState = CameraState.Start;
            };
            Transition.Default.DoTransition(action);
        });

        //System.Action action = () =>
        //{
        //    LevelManager.Default.RestartLevel();
        //    UIManager.Default.CurentState = UIState.Start;
        //    if (CameraSystem.Default)
        //        CameraSystem.Default.CurentState = CameraState.Start;

        //};
        //LevelTransitionEffect.Default.DoTransition(action);


    }
    private void ClearOnVaccumEnd()
    {
        
    }
}
