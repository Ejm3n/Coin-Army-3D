using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;

public class LottoPanel : MonoBehaviour
{
    public static bool BigRedButtonWasPressed;

    public CanvasGroup TopGroup;
    public CanvasGroup ProfileCounterGroup;
    public TextMeshProUGUI UserNameText;
    public TextMeshProUGUI UserNumText;
    public Image UserImage;
    public TextMeshProUGUI TargetCounterText;
    public TextMeshProUGUI TargetCounterText2;
    public TextMeshProUGUI TargetCounterText3;
    public TextMeshProUGUI TargetCounterText4;
    public Image[] TargetImage;
    public Image[] TargetImage2;
    public Image[] TargetImage3;
    public RectTransform[] TargetBGGlow;
    public TextMeshProUGUI ProfileCounterText;
    public TextMeshProUGUI MultiplierText;
    public TextMeshProUGUI SpinsCounterText;
    public TextMeshProUGUI MissionTargetDescription;
    public float MaxSliderWidth;
    public RectTransform[] TargetSlider;
    public RectTransform SpinsSlider;
    public RectTransform MoneyDisplay;
    public RectTransform ShieldsDisplay;
    public RectTransform SpinsDisplay;
    public TextMeshProUGUI SpinsTimerText;
    public CanvasGroup SpinsTimerGroup;
    public Sprite[] BallIcons;
    public Sprite[] BallIcons2;
    public TMP_SpriteAsset[] BallIconsText;
    public Sprite[] RewardIcons;

    public CanvasGroup MissionPanelGroup;

    public RectTransform BetButton;
    public RectTransform CloseMissionPanel1;
    public RectTransform CloseMissionPanel2;

    public RectTransform[] MissionAnimSprites;
    public RectTransform MissionAnimEnd;

    public CanvasGroup FloatingHandStatic1;
    public CanvasGroup FloatingHandStatic2;
    public CanvasGroup FloatingHandStatic3;
    public CanvasGroup FloatingHandStatic4;

    public Transform FloatingHandStatic2Target;

    public CanvasGroup TutorialDarkenScreen;
    public CanvasGroup TutorialDarkenScreen2;
    public CanvasGroup TutorialDarkenScreen3;

    public CanvasGroup MissionResultsScreenGroup;
    public RectTransform[] MissionResultsBaloons;
    public TextMeshProUGUI MissionResultsText;

    public RectTransform MissionResultsMiddlePiece;

    public RectTransform MissionResultsCloseButton;

    public Image MissionResultsIcon;

    public CanvasGroup BettingButtonGroup;

    public RectTransform ShieldSprite;
    public RectTransform ShieldAnimEnd;

    public CanvasGroup MissionSliderGroup;
    public CanvasGroup MissionLock;

    public Animator LockChainsBreakAnimator;

    public GameObject ShopButton;

    public CanvasGroup TutorialStage9Panel;

    public CanvasGroup TutorialDarkenScreen4;

    public CanvasGroup OpenFightButtonCanvas;

    public CanvasGroup TutorialStage10Panel;

    public CanvasGroup WatchAdPanel;
    public RectTransform WatchAdCloseButton;
    public RectTransform WatchAdOkButton;
    public TextMeshProUGUI WatchAdRewardAmount;

    public CanvasGroup BuyPackPanel;
    public RectTransform BuyPackCloseButton;
    public RectTransform BuyPackOkButton;
    public TextMeshProUGUI BuyPackRewardAmount1;
    public TextMeshProUGUI BuyPackRewardAmount2;
    public TextMeshProUGUI BuyPackPriceAmount1;
    public TextMeshProUGUI BuyPackPriceAmount2;
    public GameObject BuyPackPriceAmount2Cross;
    public TextMeshProUGUI BuyPackTimeLeft;

    private bool _showProfileCounter;
    private float _showProfileCounterTimeout;

    [NonSerialized]
    public bool MissionPanelOpen;
    [NonSerialized]
    public bool MissionResultsOpen;

    private float _buttonHandTimeout;

    private AudioSource _missionFillSound;
    private float _missionFillVolume;

    private float _targetMissionSliderValue;

    private float _missionLockDelay = 0f;
    private bool _readyToUnlockMissions;

    private float _sideButtonTutorialDelay;

    private float _tutorialStage9Timeout;

    private bool _watchAdOpen;
    private bool _watchAdRewardMode;
    private bool _buyPackRewardMode1;
    private bool _buyPackRewardMode2;

    private bool _buyPackOpen;

    void Awake()
    {
        if (LevelSettings.TutorialStage <= 10)
        {
            _readyToUnlockMissions = true;
        }
    }

    void Start()
    {
        ProfileCounterText.text = "";
        ProfileCounterGroup.alpha = 0f;
        MissionPanelGroup.alpha = 0f;
        MissionPanelGroup.interactable = false;
    }

    void OnEnable()
    {        
        SpinsService.Default.OnSpinsChanged += PulseSpins;

        var mission = MissionService.Default.GetCurrentMission();
        var missionProgress = Mathf.Min(mission.TargetAmount, MissionService.Default.GetCurrentMissionProgress());

        foreach (var slider in TargetSlider)
        {
            slider.sizeDelta = new Vector2(Mathf.Min(MaxSliderWidth, MaxSliderWidth * ((float)missionProgress / mission.TargetAmount)), slider.sizeDelta.y);
        }

        SpinsSlider.sizeDelta = new Vector2(Mathf.Min(MaxSliderWidth, MaxSliderWidth * ((float)SpinsService.Default.GetSpins() / GameData.Default.maxSpinsAmount)), SpinsSlider.sizeDelta.y);

        SpinsTimerGroup.alpha = IdleController.NextSpinTimerActive ? 1f : 0f;

        ShopButton.SetActive(!GameData.Default.DisableShop && (LevelManager.Default.DifficultyCounter >= 20 || GameData.Default.DisableTutorial));
    }

    void OnDisable()
    {
        SpinsService.Default.OnSpinsChanged -= PulseSpins;

        if (_missionFillSound != null)
        {
            _missionFillSound.volume = 0f;
            _missionFillSound.Stop();
        }

        MoneyCounter.AllowUpdate = true;
        MoneyCounter.UpdateDuration = 1;

        CloseMissionResultScreen();
    }

    void Update()
    {
        TopGroup.alpha = Mathf.MoveTowards(TopGroup.alpha, SlotMachine.IsTransitioning ? 0f : 1f, Time.deltaTime * 10f);
        TopGroup.interactable = TopGroup.alpha == 1f;

        UserNameText.text = OpponentService.Default.Description.Username;
        UserImage.sprite = OpponentService.Default.Description.Avatar;
        UserNumText.text = string.Format(Language.Text("MoneyAmount"), OpponentService.Default.MoneyLeft.ToString("N0"));

        var mission = MissionService.Default.GetCurrentMission();
        var missionProgress = Mathf.Min(mission.TargetAmount, MissionService.Default.GetCurrentMissionProgress());

        TargetCounterText2.text = MoneyService.AmountToString(mission.RewardAmount);
        TargetCounterText4.text = MoneyService.AmountToString(mission.RewardAmount);

        foreach (var im in TargetImage)
        {
            im.sprite = BallIcons[(int)mission.TargetBallType];
        }
        foreach (var im in TargetImage2)
        {
            im.sprite = RewardIcons[(int)mission.RewardType];
        }

        foreach (var im in TargetImage3)
        {
            im.sprite = BallIcons2[(int)mission.TargetBallType];
        }

        foreach (var glow in TargetBGGlow)
        {
            glow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(Time.time * 90f, 360f));
        }

        MultiplierText.text = string.Format(Language.Text("BetButton"), MoneyService.AmountToString(SlotMachine.NextMultiplier));

        SpinsCounterText.text = $"{Mathf.Min(SpinsService.Default.GetSpins(), GameData.Default.maxSpinsAmount)}/{GameData.Default.maxSpinsAmount}";

        _targetMissionSliderValue = Mathf.Min(MaxSliderWidth, MaxSliderWidth * ((float)missionProgress / mission.TargetAmount));

        foreach (var slider in TargetSlider)
        {
            slider.sizeDelta = new Vector2(_targetMissionSliderValue > slider.sizeDelta.x ? Mathf.MoveTowards(slider.sizeDelta.x, _targetMissionSliderValue, Time.deltaTime * 150f) : _targetMissionSliderValue, slider.sizeDelta.y);
        }

        TargetCounterText.text = $"{Mathf.FloorToInt((TargetSlider[0].sizeDelta.x / MaxSliderWidth) * mission.TargetAmount)}/{mission.TargetAmount}";
        TargetCounterText3.text = TargetCounterText.text;

        SpinsSlider.sizeDelta = new Vector2(Mathf.Lerp(SpinsSlider.sizeDelta.x, Mathf.Min(MaxSliderWidth, MaxSliderWidth * ((float)SpinsService.Default.GetSpins() / GameData.Default.maxSpinsAmount)), Time.deltaTime * 5f), SpinsSlider.sizeDelta.y);

        if (_showProfileCounterTimeout <= 0f)
        {
            _showProfileCounter = false;
        }
        else
        {
            _showProfileCounterTimeout -= Time.deltaTime;
        }
        ProfileCounterGroup.alpha = Mathf.MoveTowards(ProfileCounterGroup.alpha, _showProfileCounter ? 1f : 0f, Time.deltaTime * 5f);

        SpinsTimerGroup.alpha = Mathf.MoveTowards(SpinsTimerGroup.alpha, IdleController.NextSpinTimerActive || SpinsService.Default.GetSpins() > GameData.Default.maxSpinsAmount ? 1f : 0f, Time.deltaTime * 5f);

        if (SpinsService.Default.GetSpins() > GameData.Default.maxSpinsAmount)
        {
            SpinsTimerText.text = string.Format(Language.Text("SpinsAmount"), (MoneyService.AmountToString(SpinsService.Default.GetSpins() - GameData.Default.maxSpinsAmount)));
        }
        else
        {
            SpinsTimerText.text = string.Format(Language.Text("SpinsTimer"), GameData.Default.NextSpinAmount, string.Format("{0:D2}:{1:D2}", (int)(IdleController.NextSpinTimer / 60), (int)(IdleController.NextSpinTimer % 60)));
        }

        MissionPanelGroup.alpha = Mathf.MoveTowards(MissionPanelGroup.alpha, MissionPanelOpen ? 1f : 0f, Time.deltaTime * 5f);
        MissionPanelGroup.interactable = MissionPanelGroup.alpha > 0f;
        MissionPanelGroup.blocksRaycasts = MissionPanelGroup.interactable;

        MissionTargetDescription.text = string.Format(Language.Text(mission.MissionDescription), mission.TargetAmount);
        MissionTargetDescription.spriteAsset = BallIconsText[(int)mission.TargetBallType];

        if (!SlotMachine.IsTransitioning && !MissionPanelOpen)
        {
            //if (!ScreenManager.IsPointerOverGameObject())
            //{
            //    if (Input.GetMouseButtonDown(0))
            //    {
            //        _mousePressed = true;
            //        _mousePosition = Input.mousePosition;
            //    }
            //}

            //if (_mousePressed)
            //{
            //    if (Input.GetMouseButtonUp(0))
            //    {
            //        _mousePressed = false;
            //        if ((Input.mousePosition.y - _mousePosition.y) / Screen.height < -0.05f)
            //        {
            //            ScreenManager.Default.GoToFightScreen(() => {});
            //        }
            //    }
            //}
        }
        else
        {
            //_mousePressed = false;
        }

        bool handCondition = LevelSettings.TutorialStage == 6 && SpinsService.Default.GetSpins() == 0 && !SlotMachine.IsTransitioning && !SlotMachine.ShowsJackpotAnimationType2 && SlotMachine.IsReady;

        if (handCondition)
        {
            _sideButtonTutorialDelay += Time.deltaTime;
        }
        else
        {
            _sideButtonTutorialDelay = 0f;
        }

        FloatingHandStatic1.alpha = Mathf.MoveTowards(FloatingHandStatic1.alpha, handCondition && _sideButtonTutorialDelay > 1.5f ? 1f : 0f, Time.deltaTime * 5f);

        TutorialDarkenScreen.alpha = Mathf.MoveTowards(TutorialDarkenScreen.alpha, handCondition && _sideButtonTutorialDelay > 1.5f ? 1f : 0f, Time.deltaTime * 5f);
        TutorialDarkenScreen.blocksRaycasts = handCondition;

        MissionResultsScreenGroup.alpha = Mathf.MoveTowards(MissionResultsScreenGroup.alpha, MissionResultsOpen && ((_targetMissionSliderValue - TargetSlider[0].sizeDelta.x) < 0.1f) ? 1f : 0f, Time.deltaTime * 2f);
        MissionResultsScreenGroup.interactable = MissionResultsScreenGroup.alpha > 0f;
        MissionResultsScreenGroup.blocksRaycasts = MissionResultsScreenGroup.interactable;

        BettingButtonGroup.alpha = Mathf.MoveTowards(BettingButtonGroup.alpha, LevelSettings.TutorialStage > 14 ? 1f : 0f, Time.deltaTime * 5f);
        BettingButtonGroup.interactable = BettingButtonGroup.alpha == 1f;
        BettingButtonGroup.blocksRaycasts = BettingButtonGroup.interactable;

        if (LevelSettings.TutorialStage > 9 && _readyToUnlockMissions && !Transition.Default.IsInTransition())
        {
            LockChainsBreakAnimator.SetTrigger("Break");
            _missionLockDelay = 1.5f;
            _readyToUnlockMissions = false;
        }

        MissionSliderGroup.alpha = 1f;
        MissionSliderGroup.interactable = LevelSettings.TutorialStage > 9 && _missionLockDelay <= 0f;
        MissionSliderGroup.blocksRaycasts = MissionSliderGroup.interactable;

        MissionLock.alpha = Mathf.MoveTowards(MissionLock.alpha, LevelSettings.TutorialStage > 9 && _missionLockDelay <= 0f && !_readyToUnlockMissions ? 0f : 1f, Time.deltaTime * 5f);

        TutorialDarkenScreen2.alpha = Mathf.MoveTowards(TutorialDarkenScreen2.alpha, LevelSettings.TutorialStage != 10 ? 0f : 1f, Time.deltaTime * 5f);
        TutorialDarkenScreen2.blocksRaycasts = TutorialDarkenScreen2.alpha > 0f;

        FloatingHandStatic2.alpha = Mathf.MoveTowards(FloatingHandStatic2.alpha, LevelSettings.TutorialStage != 10 || _missionLockDelay > 0f || _readyToUnlockMissions ? 0f : 1f, Time.deltaTime * 5f);

        TutorialDarkenScreen2.transform.position = FloatingHandStatic2Target.position;
        FloatingHandStatic2.transform.position = FloatingHandStatic2Target.position;

        if (_missionLockDelay > 0f)
        {
            _missionLockDelay -= Time.deltaTime;
        }

        TutorialDarkenScreen3.alpha = Mathf.MoveTowards(TutorialDarkenScreen3.alpha, LevelSettings.TutorialStage != 15 ? 0f : 1f, Time.deltaTime * 5f);
        TutorialDarkenScreen3.blocksRaycasts = TutorialDarkenScreen3.alpha > 0f;

        FloatingHandStatic3.alpha = Mathf.MoveTowards(FloatingHandStatic3.alpha, LevelSettings.TutorialStage != 15 ? 0f : 1f, Time.deltaTime * 5f);

        if (LevelSettings.TutorialStage < 15)
        {
            if (
                SpinsService.Default.GetSpins() < SlotMachine.NextMultiplier ||
                SlotMachine.IsTransitioning ||
                SlotMachine.ShowsJackpotAnimationType2 ||
                !SlotMachine.IsReady ||
                TutorialDarkenScreen.alpha > 0f ||
                TutorialDarkenScreen2.alpha > 0f ||
                TutorialDarkenScreen3.alpha > 0f ||
                MissionPanelOpen ||
                MissionResultsOpen ||
                Input.GetMouseButton(0)
                )
            {
                _buttonHandTimeout = 0f;
            }
            else
            {
                _buttonHandTimeout += Time.deltaTime;
            }
        }
        else
        {
            _buttonHandTimeout = 0f;
        }

        FloatingHandStatic4.alpha = Mathf.MoveTowards(FloatingHandStatic4.alpha, _buttonHandTimeout < 2f ? 0f : 1f, Time.deltaTime * 5f);

        if (_missionFillSound == null)
        {
            _missionFillSound = SoundHolder.Default.PlayFromSoundPack("MissionFill", true, allowPitchShift: false);
            _missionFillVolume = _missionFillSound.volume;
            _missionFillSound.volume = 0f;
        }

        _missionFillSound.volume = Mathf.MoveTowards(_missionFillSound.volume, (_targetMissionSliderValue - TargetSlider[0].sizeDelta.x) > 1f ? _missionFillVolume : 0f, Time.deltaTime * 5f);

        if (_missionFillSound.volume > 0f && !_missionFillSound.isPlaying)
        {
            _missionFillSound.Play();
        }

        if (SlotMachine.SpecialBet)
        {
            int maxBetAmount = (int)(Mathf.Floor(SpinsService.Default.GetSpins() / 5f / 5f) * 5f);

            if (maxBetAmount < SlotMachine.NextMultiplier)
            {
                SlotMachine.NextMultiplier = maxBetAmount;
            }

            if (SlotMachine.NextMultiplier < 5)
            {
                SlotMachine.SpecialBet = false;
                SlotMachine.NextMultiplier = Mathf.Min(SpinsService.Default.GetSpins(), 3);
            }
        }
        else
        {
            if (SlotMachine.NextMultiplier > Mathf.Max(1, SpinsService.Default.GetSpins()))
            {
                SlotMachine.NextMultiplier = Mathf.Max(1, SpinsService.Default.GetSpins());
            }
        }

        if (SpinsService.Default.GetSpins() == 0)
        {
            BigRedButtonWasPressed = true;
        }

        bool slotReady = !SlotMachine.IsTransitioning && !SlotMachine.ShowsJackpotAnimationType2 && SlotMachine.IsReady;

        TutorialStage9Panel.alpha = Mathf.MoveTowards(TutorialStage9Panel.alpha, LevelSettings.TutorialStage == 6 && !BigRedButtonWasPressed && SpinsService.Default.GetSpins() != 0 ? 1f : 0f, Time.deltaTime * 5f);

        if (TutorialStage9Panel.alpha == 1f)
        {
            _tutorialStage9Timeout += Time.deltaTime;
        }
        else
        {
            _tutorialStage9Timeout = 0f;
        }

        TutorialDarkenScreen4.alpha = Mathf.MoveTowards(TutorialDarkenScreen4.alpha, _tutorialStage9Timeout > 1f ? 1f : 0f, Time.deltaTime * 5f);
        TutorialDarkenScreen4.blocksRaycasts = TutorialStage9Panel.alpha > 0f;

        OpenFightButtonCanvas.alpha = Mathf.MoveTowards(OpenFightButtonCanvas.alpha, LevelSettings.TutorialStage != 6 || SpinsService.Default.GetSpins() == 0 && slotReady ? 1f : 0f, Time.deltaTime * 5f);
        OpenFightButtonCanvas.interactable = OpenFightButtonCanvas.alpha == 1f;
        OpenFightButtonCanvas.blocksRaycasts = OpenFightButtonCanvas.interactable;

        TutorialStage10Panel.alpha = Mathf.MoveTowards(TutorialStage10Panel.alpha, LevelSettings.TutorialStage == 6 && SpinsService.Default.GetSpins() == 0 && slotReady ? 1f : 0f, Time.deltaTime * 5f);

        WatchAdPanel.alpha = Mathf.MoveTowards(WatchAdPanel.alpha, _watchAdOpen ? 1f : 0f, Time.deltaTime * 2f);
        WatchAdPanel.interactable = WatchAdPanel.alpha > 0f;
        WatchAdPanel.blocksRaycasts = WatchAdPanel.interactable;

        BuyPackPanel.alpha = Mathf.MoveTowards(BuyPackPanel.alpha, _buyPackOpen ? 1f : 0f, Time.deltaTime * 2f);
        BuyPackPanel.interactable = BuyPackPanel.alpha > 0f;
        BuyPackPanel.blocksRaycasts = BuyPackPanel.interactable;
    }

    public void ChangeBet()
    {
        if (LevelManager.Default.DifficultyCounter >= 27)
        {
            int specialBetMaxAmount = (int)(Mathf.Floor(SpinsService.Default.GetSpins() / 5f / 5f) * 5f);

            if (SlotMachine.SpecialBet)
            {
                SlotMachine.NextMultiplier += 5;

                if (SlotMachine.NextMultiplier > specialBetMaxAmount)
                {
                    SlotMachine.SpecialBet = false;
                    SlotMachine.NextMultiplier = 1;
                }
            }
            else
            {
                SlotMachine.NextMultiplier++;

                if (specialBetMaxAmount >= 5)
                {
                    if (SlotMachine.NextMultiplier > 3)
                    {
                        SlotMachine.SpecialBet = true;
                        SlotMachine.NextMultiplier = 5;
                    }
                }
                else
                {
                    if (SlotMachine.NextMultiplier > Mathf.Min(SpinsService.Default.GetSpins(), 3))
                    {
                        SlotMachine.NextMultiplier = 1;
                    }
                }
            }
        }
        else
        {
            SlotMachine.NextMultiplier++;
            SlotMachine.SpecialBet = false;

            if (LevelSettings.TutorialStage < 15)
            {
                if (SlotMachine.NextMultiplier > Mathf.Min(SpinsService.Default.GetSpins(), 2))
                {
                    SlotMachine.NextMultiplier = 1;
                }
            }
            else
            {
                if (SlotMachine.NextMultiplier > Mathf.Min(SpinsService.Default.GetSpins(), 3))
                {
                    SlotMachine.NextMultiplier = 1;
                }
            }
        }

        BetButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            BetButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        if (LevelSettings.TutorialStage == 15)
        {
            LevelSettings.TutorialStage++;
        }

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void DisplayIncome(ulong amount, bool longer)
    {
        ProfileCounterText.text = string.Format(Language.Text("MoneyAmount"), amount.ToString("N0"));
        _showProfileCounter = true;
        _showProfileCounterTimeout = 3f;
        MoneyCounter.AllowUpdate = false;
        MoneyCounter.UpdateTimeout = 1f;
        MoneyCounter.UpdateDuration = longer ? 2.5 : 1;
    }

    public void HideIncome()
    {
        _showProfileCounter = false;
        _showProfileCounterTimeout = 0f;
    }

    public void PulseShields()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        ShieldsDisplay.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            ShieldsDisplay.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutBounce);
        });
    }

    public void PulseSpins()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        SpinsDisplay.DOScale(Vector3.one * 1.3f * 1.1f, 0.2f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            SpinsDisplay.DOScale(Vector3.one * 1.3f, 0.3f).SetEase(Ease.InOutBounce);
        });
    }

    public void OpenMissionPanel()
    {
        MissionPanelOpen = true;

        if (LevelSettings.TutorialStage == 10)
        {
            LevelSettings.TutorialStage++;
        }
    }
    
    public void CloseMissionPanel()
    {
        MissionPanelOpen = false;

        if (LevelSettings.TutorialStage == 11)
        {
            LevelSettings.TutorialStage++;
        }

        if (LevelSettings.TutorialStage == 12)
        {
            LevelSettings.TutorialStage++;
        }

        if (LevelSettings.TutorialStage == 13 && PlayerPrefs.GetInt("DidTheft") == 1)
        {
            LevelSettings.TutorialStage++;
        }
    }

    public void PulseMissionButton1()
    {
        CloseMissionPanel1.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            CloseMissionPanel1.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void PulseMissionButton2()
    {
        CloseMissionPanel2.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            CloseMissionPanel2.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void DoShieldAnim(LottoBall ball, Action onComplete)
    {
        var target = ShieldSprite;
        var targetGraphic = target.GetComponent<Image>();

        var seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            target.gameObject.SetActive(true);
            target.position = Camera.main.WorldToScreenPoint(ball.transform.position);
            target.localScale = Vector3.one * 2.2f;
            targetGraphic.color = new Color(1f, 1f, 1f, 0f);
        });

        seq.Append(targetGraphic.DOFade(1f, 0.2f).SetEase(Ease.InOutCubic));

        seq.Append(target.DOMove(ShieldAnimEnd.position, 0.5f).SetEase(Ease.InCubic));

        seq.Join(target.DOScale(Vector3.one, 0.5f).SetEase(Ease.InCubic));

        seq.Join(targetGraphic.DOFade(0f, 0.1f).SetEase(Ease.InCubic).SetDelay(0.4f));

        seq.OnComplete(() =>
        {
            target.gameObject.SetActive(false);
            onComplete();
        });
    }

    public void DoMissionAnim(LottoBall ball, int index, Action onComplete)
    {
        var target = MissionAnimSprites[index];
        var targetGraphic = target.GetComponent<Image>();

        var seq = DOTween.Sequence();

        seq.AppendInterval(0.5f);

        seq.AppendCallback(() =>
        {
            target.gameObject.SetActive(true);
            target.position = Camera.main.WorldToScreenPoint(ball.transform.position);
            target.localScale = Vector3.one * 2.5f;
            targetGraphic.color = new Color(1f, 1f, 1f, 0f);
        });

        seq.Append(targetGraphic.DOFade(1f, 0.2f).SetEase(Ease.InOutCubic));

        seq.Append(target.DOMove(MissionAnimEnd.position, 0.5f).SetEase(Ease.InCubic));

        seq.Join(target.DOScale(Vector3.one, 0.5f).SetEase(Ease.InCubic));

        seq.Join(targetGraphic.DOFade(0f, 0.1f).SetEase(Ease.InCubic).SetDelay(0.4f));

        seq.OnComplete(() =>
        {
            target.gameObject.SetActive(false);
            onComplete();
        });
    }

    public void DoMissionResultScreen()
    {
        if (MissionResultsOpen)
        {
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        StartCoroutine(MissionResultScreenWait());
    }

    IEnumerator MissionResultScreenWait()
    {
        MissionResultsOpen = true;
        _watchAdRewardMode = false;
        _buyPackRewardMode1 = false;
        _buyPackRewardMode2 = false;

        yield return 0f;

        yield return new WaitUntil(() => (_targetMissionSliderValue - TargetSlider[0].sizeDelta.x) < 0.1f);

        if (!MissionResultsOpen)
        {
            yield break;
        }

        var mission = MissionService.Default.GetCurrentMission();
        MissionResultsIcon.sprite = RewardIcons[(int)mission.RewardType];
        MissionResultsText.text = MoneyService.AmountToString(mission.RewardAmount);

        MissionResultsMiddlePiece.localScale = Vector3.zero;
        MissionResultsMiddlePiece.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic);

        MissionResultsBaloons[0].anchoredPosition = new Vector2(-140f, -153f);
        MissionResultsBaloons[1].anchoredPosition = new Vector2(-276f, -311f);
        MissionResultsBaloons[2].anchoredPosition = new Vector2(279f, -228f);
        MissionResultsBaloons[3].anchoredPosition = new Vector2(-10f, -185f);
        MissionResultsBaloons[4].anchoredPosition = new Vector2(141f, -81f);

        foreach (var baloon in MissionResultsBaloons)
        {
            baloon.DOAnchorPos(new Vector2(baloon.anchoredPosition.x, baloon.anchoredPosition.y + UnityEngine.Random.Range(1000f, 1200f)), 5f);
        }

        SoundHolder.Default.PlayFromSoundPack("MissionReward", allowPitchShift: false);
    }

    public void CloseMissionResultScreen()
    {
        if (!MissionResultsOpen)
        {
            return;
        }

        MissionResultsOpen = false;

        if (!_watchAdRewardMode && !_buyPackRewardMode1 && !_buyPackRewardMode2)
        {
            MissionService.Default.NextMission(() =>
            {
                if (!gameObject.activeInHierarchy)
                {
                    return;
                }

                DoMissionResultScreen();
            });

            ResetMissionSlider();
        }

        MissionResultsCloseButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            MissionResultsCloseButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");

        if (_buyPackRewardMode1)
        {
            DoBuyPackRewardScreen2();
        }
    }

    public void DoAdRewardScreen()
    {
        if (MissionResultsOpen)
        {
            return;
        }

        MissionResultsOpen = true;
        _watchAdRewardMode = true;
        _buyPackRewardMode1 = false;
        _buyPackRewardMode2 = false;

        MissionResultsIcon.sprite = RewardIcons[(int)GameData.RewardType.Spins];
        MissionResultsText.text = MoneyService.AmountToString(GameData.Default.AdsSpinsAmount);

        MissionResultsMiddlePiece.localScale = Vector3.zero;
        MissionResultsMiddlePiece.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic);

        MissionResultsBaloons[0].anchoredPosition = new Vector2(-140f, -153f);
        MissionResultsBaloons[1].anchoredPosition = new Vector2(-276f, -311f);
        MissionResultsBaloons[2].anchoredPosition = new Vector2(279f, -228f);
        MissionResultsBaloons[3].anchoredPosition = new Vector2(-10f, -185f);
        MissionResultsBaloons[4].anchoredPosition = new Vector2(141f, -81f);

        foreach (var baloon in MissionResultsBaloons)
        {
            baloon.DOAnchorPos(new Vector2(baloon.anchoredPosition.x, baloon.anchoredPosition.y + UnityEngine.Random.Range(1000f, 1200f)), 5f);
        }

        SoundHolder.Default.PlayFromSoundPack("MissionReward", allowPitchShift: false);
    }

    public void RequestMouseProxy(IMouseEventProxyTarget target)
    {
        var proxy = new GameObject("proxy", typeof(RectTransform), typeof(MouseEventProxy), typeof(Image));
        proxy.transform.SetParent(transform);
        proxy.transform.SetAsFirstSibling();
        proxy.GetComponent<MouseEventProxy>().Target = target;
        proxy.GetComponent<Image>().color = Color.clear;
    }

    public void ResetMissionSlider()
    {
        foreach (var slider in TargetSlider)
        {
            slider.sizeDelta = new Vector2(0f, slider.sizeDelta.y);
        }
    }

    public void OpenWatchAdPanel()
    {
        if (_watchAdOpen || _buyPackOpen)
        {
            return;
        }

        if (LevelManager.Default.DifficultyCounter < 20 && !GameData.Default.DisableTutorial)
        {
            return;
        }

        if (SlotMachine.IsTransitioning || SlotMachine.ShowsJackpotAnimationType2 || !SlotMachine.IsReady)
        {
            return;
        }

        if (!RewardedAdsService.Default.AdsAreAvailable)
        {
            return;
        }

        _watchAdOpen = true;

        WatchAdRewardAmount.text = MoneyService.AmountToString(GameData.Default.AdsSpinsAmount);
    }

    public void CloseWatchAdPanel()
    {
        if (!_watchAdOpen)
        {
            return;
        }

        _watchAdOpen = false;
    }

    public void PulseCloseWatchAdButton()
    {
        WatchAdCloseButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            WatchAdCloseButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void WatchAd()
    {
        CloseWatchAdPanel();

        RewardedAdsService.Default.ShowAd(() =>
        {
            DoAdRewardScreen();

            SpinsService.Default.AddSpins(GameData.Default.AdsSpinsAmount, false);
        });
    }

    public void DoNoSpinsPromotion()
    {
        if (IAPService.Default.IAPIsAvailable)
        {
            OpenBuyPackPanel();
        }
        else
        {
            OpenWatchAdPanel();
        }
    }

    public void OpenBuyPackPanel()
    {
        OpenBuyPackPanel(GameData.Default.IAPMoneyReward, GameData.Default.IAPSpinsReward, GameData.Default.IAPPrice, 0, 0);
    }

    public void OpenBuyPackPanel(ulong moneyReward, int spinsReward, double price, double originalPrice, double endTime)
    {
        if (_buyPackOpen || _watchAdOpen)
        {
            return;
        }

        if (LevelManager.Default.DifficultyCounter < 20 && !GameData.Default.DisableTutorial)
        {
            return;
        }

        if (SlotMachine.IsTransitioning || SlotMachine.ShowsJackpotAnimationType2 || !SlotMachine.IsReady)
        {
            return;
        }

        if (!IAPService.Default.IAPIsAvailable)
        {
            return;
        }

        _buyPackOpen = true;

        BuyPackRewardAmount1.text = MoneyService.AmountToString(moneyReward);
        BuyPackRewardAmount2.text = MoneyService.AmountToString(spinsReward);

        BuyPackPriceAmount1.text = string.Format(Language.Text("PriceAmount"), MoneyService.AmountToString(price));

        if (price == originalPrice || originalPrice == 0)
        {
            BuyPackPriceAmount2.text = "";
            BuyPackPriceAmount2Cross.SetActive(false);
        }
        else
        {
            BuyPackPriceAmount2.text = string.Format(Language.Text("PriceAmount"), MoneyService.AmountToString(originalPrice));
            BuyPackPriceAmount2Cross.SetActive(true);
        }

        if (endTime == 0)
        {
            BuyPackTimeLeft.text = "";
        }
        else
        {
            BuyPackTimeLeft.text = string.Format(Language.Text("TimeLeft"), string.Format("{0:D2}:{1:D2}", (int)(endTime - IdleController.GetCurrentTime())));
        }
    }

    public void CloseBuyPackPanel()
    {
        if (!_buyPackOpen)
        {
            return;
        }

        _buyPackOpen = false;

        OpenWatchAdPanel();
    }

    public void PulseCloseBuyPackButton()
    {
        BuyPackCloseButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            BuyPackCloseButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void BuyPack()
    {
        _buyPackOpen = false;

        IAPService.Default.DoPurchase("starter_pack", () =>
        {
            DoBuyPackRewardScreen1();

            MoneyService.Default.AddMoney(GameData.Default.IAPMoneyReward);
            SpinsService.Default.AddSpins(GameData.Default.IAPSpinsReward, false);
        });
    }

    public void DoBuyPackRewardScreen1()
    {
        if (MissionResultsOpen)
        {
            return;
        }

        MissionResultsOpen = true;
        _watchAdRewardMode = false;
        _buyPackRewardMode1 = true;
        _buyPackRewardMode2 = false;

        MissionResultsIcon.sprite = RewardIcons[(int)GameData.RewardType.Money];
        MissionResultsText.text = MoneyService.AmountToString(GameData.Default.IAPMoneyReward);

        MissionResultsMiddlePiece.localScale = Vector3.zero;
        MissionResultsMiddlePiece.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic);

        MissionResultsBaloons[0].anchoredPosition = new Vector2(-140f, -153f);
        MissionResultsBaloons[1].anchoredPosition = new Vector2(-276f, -311f);
        MissionResultsBaloons[2].anchoredPosition = new Vector2(279f, -228f);
        MissionResultsBaloons[3].anchoredPosition = new Vector2(-10f, -185f);
        MissionResultsBaloons[4].anchoredPosition = new Vector2(141f, -81f);

        foreach (var baloon in MissionResultsBaloons)
        {
            baloon.DOAnchorPos(new Vector2(baloon.anchoredPosition.x, baloon.anchoredPosition.y + UnityEngine.Random.Range(1000f, 1200f)), 5f);
        }

        SoundHolder.Default.PlayFromSoundPack("MissionReward", allowPitchShift: false);
    }

    public void DoBuyPackRewardScreen2()
    {
        if (MissionResultsOpen)
        {
            return;
        }

        MissionResultsOpen = true;
        _watchAdRewardMode = false;
        _buyPackRewardMode1 = false;
        _buyPackRewardMode2 = true;

        MissionResultsIcon.sprite = RewardIcons[(int)GameData.RewardType.Spins];
        MissionResultsText.text = MoneyService.AmountToString(GameData.Default.IAPSpinsReward);

        MissionResultsMiddlePiece.localScale = Vector3.zero;
        MissionResultsMiddlePiece.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic);

        MissionResultsBaloons[0].anchoredPosition = new Vector2(-140f, -153f);
        MissionResultsBaloons[1].anchoredPosition = new Vector2(-276f, -311f);
        MissionResultsBaloons[2].anchoredPosition = new Vector2(279f, -228f);
        MissionResultsBaloons[3].anchoredPosition = new Vector2(-10f, -185f);
        MissionResultsBaloons[4].anchoredPosition = new Vector2(141f, -81f);

        foreach (var baloon in MissionResultsBaloons)
        {
            baloon.DOAnchorPos(new Vector2(baloon.anchoredPosition.x, baloon.anchoredPosition.y + UnityEngine.Random.Range(1000f, 1200f)), 5f);
        }

        SoundHolder.Default.PlayFromSoundPack("MissionReward", allowPitchShift: false);
    }
}
