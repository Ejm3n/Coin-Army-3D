using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BG.UI.Main;
using DG.Tweening;
using UnityEngine.UI;
using BG.UI.Camera;
using System;
using TMPro;

public class WinAnim : MonoBehaviour
{
    [SerializeField] private Button _nextButton;
    [SerializeField] private CanvasGroup _buttonGroup;
    [SerializeField] private TextMeshProUGUI _spinsText;
    [SerializeField] private RectTransform _glow;
    [SerializeField] private RectTransform _glow2;
    [SerializeField] private RectTransform _glow3;
    [SerializeField] private GameObject _spinsMain;
    [SerializeField] private GameObject _noSpinsMain;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _rewardText2;

    [SerializeField] private RouletteAnim _rouletteAnim;

    private void Awake()
    {
        GetComponent<Panel>().onPanelShow += HandleOnPanelShow;

        _nextButton.onClick.AddListener(OnNextButtonClick);
    }

    private void OnDestroy()
    {
        _nextButton.onClick.RemoveListener(OnNextButtonClick);
    }

    void OnEnable()
    {
        _buttonGroup.alpha = 0f;
        int spinAmount = GameData.Default.SpinsRewardForPassedLevel;
        if (LevelSettings.Default && LevelSettings.Default.Description && LevelSettings.Default.Description.LocationProgress == LevelSettings.Default.Description.LocationLength)
        {
            spinAmount += GameData.Default.SpinsRewardForPassedLocation;
        }
        if (LevelSettings.Default && LevelSettings.Default.Description && LevelSettings.Default.Description.DontGiveSpins)
        {
            spinAmount = 0;
        }
        _spinsText.text = $"+{spinAmount}";
        _spinsMain.SetActive(spinAmount > 0);
        _noSpinsMain.SetActive(spinAmount == 0 && UnitManager.Default.TotalLootYield > 0);
    }

    void Update()
    {
        _glow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(Time.time * 90f, 360f));
        _glow2.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(Time.time * 90f, 360f));
        _glow3.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(Time.time * 90f, 360f));

        _rewardText.text = $"+{MoneyService.AmountToStringTrunicate((ulong)UnitManager.Default.TotalLootYield)}";
        _rewardText2.text = $"+{MoneyService.AmountToStringTrunicate((ulong)UnitManager.Default.TotalLootYield)}";
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
        UIManager.Default.GetPanel(UIState.Win).HidePanel();
        Action action = () =>
        {
            LevelManager.Default.NextLevel();
            UIManager.Default.CurrentState = UIState.Start;
            if (CameraSystem.Default)
                CameraSystem.Default.CurentState = CameraState.Start;
            ScreenManager.Default.IsInSlots = false;
            ScreenManager.Default.SlotsAreVisible = false;
            SlotMachine.NextMultiplier = 1;
            PlayerPrefs.SetInt("SlotsOpen", 0);
            var button = FindObjectOfType<BigRedButton>(true);
            if (button)
            {
                button.DisableAutospin();
            }
        };
        Transition.Default.DoTransition(action);
        _rouletteAnim.gameObject.SetActive(true);
        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
