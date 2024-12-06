using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BG.UI.Main;
using DG.Tweening;
using UnityEngine.UI;
using BG.UI.Camera;
using System;
using TMPro;

public class FailAnim : MonoBehaviour
{
    [SerializeField] private Button _nextButton;
    [SerializeField] private CanvasGroup _buttonGroup;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private RectTransform _glow;
    [SerializeField] private GameObject _noSpinsMain;

    public CanvasGroup TutorialStage4Panel;

    private void Awake()
    {
        GetComponent<Panel>().onPanelShow += HandleOnPanelShow;

        _nextButton.onClick.AddListener(OnNextButtonClick);
    }

    private void OnDestroy()
    {
        _nextButton.onClick.RemoveListener(OnNextButtonClick);
    }

    void Update()
    {
        TutorialStage4Panel.alpha = Mathf.MoveTowards(TutorialStage4Panel.alpha, LevelSettings.TutorialStage == 1 ? 1f : 0f, Time.deltaTime * 5f);

        _glow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(Time.time * 90f, 360f));

        _rewardText.text = $"+{MoneyService.AmountToStringTrunicate((ulong)UnitManager.Default.TotalLootYield)}";
    }

    void OnEnable()
    {
        _buttonGroup.alpha = 0f;
        _nextButton.GetComponentInChildren<TextMeshProUGUI>().text = LevelSettings.TutorialStage < 6 ? Language.Text("Retry") : Language.Text("Earn");
        TutorialStage4Panel.alpha = 0f;
        if (UnitManager.Default)
        {
            _noSpinsMain.SetActive(UnitManager.Default.TotalLootYield > 0);
        }
    }

    private void HandleOnPanelShow()
    {
        StartCoroutine(WinAnimCoroutine());
    }

    private IEnumerator WinAnimCoroutine()
    {
        yield return new WaitForSeconds(1f);

        ShowButton();
    }

    private void ShowButton()
    {
        _nextButton.interactable = true;
        _buttonGroup.DOFade(1f, 0.5f).SetEase(Ease.InOutCubic);
    }

    private void OnNextButtonClick()
    {
        _nextButton.transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            _nextButton.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });
        _nextButton.interactable = false;
        LevelManager.Default.isWin = false;
        UIManager.Default.GetPanel(UIState.Fail).HidePanel();

        if (LevelSettings.TutorialStage < 6)
        {
            Action action = () =>
            {
                LevelManager.Default.ReloadLevel();
                UIManager.Default.CurrentState = UIState.Start;
                if (CameraSystem.Default)
                    CameraSystem.Default.CurentState = CameraState.Start;
            };
            Transition.Default.DoTransition(action);
        }
        else
        {
            ScreenManager.Default.GoToSlotScreen(() => LevelManager.Default.ReloadLevel());
        }

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
