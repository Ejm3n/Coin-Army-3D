using BG.UI.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using MoreMountains.NiceVibrations;
using BG.UI.Camera;

public class StartPanel : MonoBehaviour
{
    #region Singleton



    private static StartPanel _default;
    public static StartPanel Default => _default;

    #endregion

    public RectTransform prewiewParent;
    private Vector3 prewiewPosition;

    private Panel _panel;

    public CanvasGroup PreGameUI;
    public CanvasGroup InGameUI;

    public CanvasGroup FightButton;

    public CanvasGroup BurgerButton;
    public CanvasGroup UnitsButton;
    public CanvasGroup LottoButton;
    public CanvasGroup BuyButton1;
    public CanvasGroup BuyButton2;
    public CanvasGroup RestartButton;
    public CanvasGroup FloatingHandStatic1;
    public CanvasGroup FloatingHandStatic2;
    public CanvasGroup FloatingHandStatic3;
    public CanvasGroup FloatingHandSwiping1;
    public FloatingHandSwiping FloatingHandSwiping1Script;
    public CanvasGroup TutorialDarkenScreen;
    public CanvasGroup TutorialDarkenScreen2;
    public CanvasGroup TutorialDarkenScreen3;
    public CanvasGroup Money;

    public GameObject BossText;

    public CanvasGroup NewUnitPanel;
    public CanvasGroup UnitCardsList;

    public CanvasGroup NewUnitPanelBG;
    public CanvasGroup UnitCardsListBG;

    public Transform NewUnitPanelClose;
    public Transform UnitCardsListClose;

    public TextMeshProUGUI NewUnitName;
    public TextMeshProUGUI NewUnitHP;
    public TextMeshProUGUI NewUnitDamage;

    public CardList CardListController;

    public Transform NewUnitPreviewParent;

    public RectTransform NewUnitCardGlow;

    public BuyButton BuyButton1Script;
    public BuyButton BuyButton2Script;

    public CanvasGroup TutorialStage1Panel;
    public CanvasGroup TutorialStage1Button;

    public CanvasGroup TutorialStage2Panel;

    public CanvasGroup TutorialStage3Panel;

    public CanvasGroup TutorialStage5Panel;

    public CanvasGroup TutorialStage7Panel;

    public CanvasGroup TutorialStage8Panel;

    public RectTransform MergeUnitsHelpText;
    public RectTransform NotEnoughMoneyHelpText;

    public CanvasGroup MergingTutorialGroup;
    public CanvasGroup MergingTutorialButton;

    private bool _initTutorialDarkenScreen2;

    private bool _newUnitPanelOpen;
    private bool _newUnitPanelOpenBG;
    private bool _cardsListPanelOpen;
    private bool _newUnitPanelReady;
    private bool _cardsListPanelReady;

    private Transform _newUnitExistingPreview;

    private bool _3dModelsEnabled = false;

    private float _unmergedTimeout = 0f;

    private Sequence _seq1;
    private Sequence _seq2;

    private float _sideButtonTutorialDelay;
    private float _sideButtonTutorialDelay2;

    private float _tutorialStage1Timeout;
    private float _tutorialStage2Timeout;
    private float _tutorialStage7Timeout;
    private float _tutorialStage8Timeout;

    private float _mergeHelpTextAnim;

    private float _notEnoughMoneyTimer;

    private bool _mergingTutorialVisible;
    private bool _mergingTutorialTrigger;
    private float _mergingTutorialButtonTimeout;

    private Vector3 _startCamPos;
    private Vector3 _startCamRot;

    private Unit _newCardUnit;

    private void Awake()
    {
        _panel = GetComponent<Panel>();
        _panel.onPanelShow += HandleOnPanelShow;
        _panel.onPanelHide += HandleOnPanelHide;
        prewiewPosition = prewiewParent.anchoredPosition;
    }

    private void Start()
    {
        _default = this;
    }

    private void OnDestroy()
    {
        _panel.onPanelShow -= HandleOnPanelShow;
        _panel.onPanelHide -= HandleOnPanelHide;
    }

    private void OnEnable()
    {
        var bg = NewUnitPanel.transform.Find("BG");
        bg.localScale = Vector3.zero;
        bg = UnitCardsList.transform.Find("BG");
        bg.localScale = Vector3.zero;
        _newUnitPanelReady = true;
        _cardsListPanelReady = true;
        _mergeHelpTextAnim = 0f;
        _notEnoughMoneyTimer = 0f;
    }

    private void OnDisable()
    {
        _newUnitPanelOpen = false;
        _newUnitPanelOpenBG = false;
        _cardsListPanelOpen = false;
        if (_3dModelsEnabled)
        {
            Disable3DModelSupport();
        }
    }

    private void HandleOnPanelShow()
    {

    }

    private void HandleOnPanelHide()
    {

    }

    private void DoGameplayUI()
    {
        if (UnitManager.Default == null)
        {
            return;
        }

        PreGameUI.alpha = Mathf.MoveTowards(PreGameUI.alpha, LevelSettings.Default.IsFightActive || LevelSettings.Default.IsPostFight ? 0f : 1f, Time.deltaTime * 5f);
        PreGameUI.interactable = PreGameUI.alpha >= 1f;
        PreGameUI.blocksRaycasts = PreGameUI.interactable;
        InGameUI.alpha = Mathf.MoveTowards(InGameUI.alpha, LevelSettings.Default.IsFightActive && !LevelSettings.Default.IsPostFight ? 1f : 0f, Time.deltaTime * 5f);
        InGameUI.interactable = InGameUI.alpha >= 1f;
        InGameUI.blocksRaycasts = InGameUI.interactable;

        BuyButton1.alpha = Mathf.MoveTowards(BuyButton1.alpha, LevelSettings.TutorialStage == -9 && _tutorialStage2Timeout > 1f || LevelSettings.TutorialStage > 1 ? 1f : 0f, Time.deltaTime * 5f);
        BuyButton1.interactable = BuyButton1.alpha == 1f;

        BuyButton2.alpha = Mathf.MoveTowards(BuyButton2.alpha, LevelSettings.TutorialStage < 1 || LevelManager.Default.DifficultyCounter == 15 && PlayerPrefs.GetInt("BoughtRangeUnitsOnLevel8") >= 2 ? 0f : 1f, Time.deltaTime * 5f);
        BuyButton2.interactable = BuyButton2.alpha == 1f;

        bool freeSpace = UnitManager.Default.PlayerGrid.HasFreeSpace();

        var price1 = UnitManager.GetUnitPrice(BuyButton1Script.BuyTargets[0]);
        var price2 = UnitManager.GetUnitPrice(BuyButton2Script.BuyTargets[0]);
        var canBuy1 = BuyButton1.alpha > 0f && MoneyService.Default.GetMoney() >= price1;
        var canBuy2 = BuyButton2.alpha > 0f && MoneyService.Default.GetMoney() >= price2;

        FightButton.alpha = Mathf.MoveTowards(FightButton.alpha, (!GameData.Default.AlwaysAllowToFight || UnitManager.Default.PlayerGrid.IsEmpty()) && ((UnitManager.Default.PlayerGrid.IsEmpty() || LevelSettings.TutorialStage == 4.5f || LevelSettings.TutorialStage < 0 || LevelSettings.TutorialStage == 1 || LevelSettings.TutorialStage == 2 || LevelManager.BlockedFromProceeding || LevelSettings.TutorialStage == 3 && _tutorialStage7Timeout < 0.25f) && LevelManager.Default.DifficultyCounter < 15 || LevelManager.Default.DifficultyCounter < 30 && (canBuy1 || canBuy2)) ? 0f : 1f, Time.deltaTime * 5f);
        FightButton.interactable = FightButton.alpha >= 1f;

        canBuy1 &= !LevelSettings.Default.IsFightActive && freeSpace;
        canBuy2 &= !LevelSettings.Default.IsFightActive && freeSpace;
        var secondIsCheaper = price1 > price2 || !canBuy1;

        if (LevelSettings.TutorialStage > 4 && !LevelSettings.Default.IsFightActive && !LevelSettings.Default.IsPostFight && Unit.GrabbedUnitsCount == 0)
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
            //        if ((Input.mousePosition.y - _mousePosition.y) / Screen.height > 0.05f)
            //        {
            //            ScreenManager.Default.GoToSlotScreen(() => {});
            //        }
            //    }
            //}
        }
        else
        {
            //_mousePressed = false;
        }

        BurgerButton.alpha = Mathf.MoveTowards(BurgerButton.alpha, LevelSettings.TutorialStage <= 3 ? 0f : 1f, Time.deltaTime * 5f);
        BurgerButton.interactable = BurgerButton.alpha == 1f;

        UnitsButton.alpha = Mathf.MoveTowards(UnitsButton.alpha, LevelSettings.TutorialStage <= 3 ? 0f : 1f, Time.deltaTime * 5f);
        UnitsButton.interactable = UnitsButton.alpha == 1f;

        LottoButton.alpha = Mathf.MoveTowards(LottoButton.alpha, LevelSettings.TutorialStage <= 4 ? 0f : 1f, Time.deltaTime * 5f);
        LottoButton.interactable = LottoButton.alpha == 1f;

        RestartButton.alpha = Mathf.MoveTowards(RestartButton.alpha, LevelSettings.TutorialStage <= 4 ? 0f : 1f, Time.deltaTime * 5f);
        RestartButton.interactable = RestartButton.alpha == 1f;

        Money.alpha = Mathf.MoveTowards(Money.alpha, LevelSettings.TutorialStage <= 0 ? 0f : 1f, Time.deltaTime * 5f);

        FloatingHandStatic1.alpha = Mathf.MoveTowards(FloatingHandStatic1.alpha, !GameData.Default.DisableAdditionalHands && ((canBuy1 && !secondIsCheaper) || LevelSettings.TutorialStage == -9 && _tutorialStage2Timeout > 1f) ? 1f : 0f, Time.deltaTime * 5f);
        FloatingHandStatic3.alpha = Mathf.MoveTowards(FloatingHandStatic3.alpha, GameData.Default.DisableAdditionalHands || !(canBuy2 && secondIsCheaper) ? 0f : 1f, Time.deltaTime * 5f);

        TutorialDarkenScreen3.alpha = Mathf.MoveTowards(TutorialDarkenScreen3.alpha, LevelSettings.TutorialStage == -9 && _tutorialStage2Timeout > 1f ? 1f : 0f, Time.deltaTime * 5f);
        TutorialDarkenScreen3.blocksRaycasts = TutorialDarkenScreen3.alpha > 1;

        bool bgCondition = LevelSettings.TutorialStage == 5 && !LevelSettings.Default.IsPostFight;
        bool handCondition = LevelSettings.TutorialStage == 5 || LevelManager.BlockedFromProceeding && (LevelManager.Default.DifficultyCounter >= 13 && LevelManager.Default.DifficultyCounter <= 29 && SpinsService.Default.GetSpins() > 0);

        if (bgCondition)
        {
            _sideButtonTutorialDelay += Time.deltaTime;
        }
        else
        {
            _sideButtonTutorialDelay = 0f;
        }

        if (handCondition)
        {
            _sideButtonTutorialDelay2 += Time.deltaTime;
        }
        else
        {
            _sideButtonTutorialDelay2 = 0f;
        }

        FloatingHandStatic2.alpha = Mathf.MoveTowards(FloatingHandStatic2.alpha, handCondition && _sideButtonTutorialDelay2 > 1.5f ? 1f : 0f, Time.deltaTime * 5f);

        TutorialDarkenScreen.alpha = Mathf.MoveTowards(TutorialDarkenScreen.alpha, bgCondition && _sideButtonTutorialDelay > 1.5f ? 1f : 0f, Time.deltaTime * 5f);
        TutorialDarkenScreen.blocksRaycasts = LevelSettings.TutorialStage == 5;

        bool hasUnmergedUnits = false;
        Unit unit1 = null;
        Unit unit2 = null;

        if (LevelSettings.TutorialStage > 2 && (LevelManager.Default.DifficultyCounter < 17 || !freeSpace) && !LevelSettings.Default.IsFightActive && !LevelSettings.Default.IsPostFight && !_newUnitPanelOpen && !_cardsListPanelOpen)
        {
            for (int i = 0; i < UnitManager.Default.PlayerUnits.Count - 1; i++)
            {
                unit1 = UnitManager.Default.PlayerUnits[i];

                if (unit1 == null || unit1.Description.TurnInto == null)
                {
                    continue;
                }

                for (int j = i + 1; j < UnitManager.Default.PlayerUnits.Count; j++)
                {
                    unit2 = UnitManager.Default.PlayerUnits[j];

                    if (unit2 == null || unit2.Description != unit1.Description)
                    {
                        continue;
                    }

                    hasUnmergedUnits = true;

                    break;
                }

                if (hasUnmergedUnits)
                {
                    break;
                }
            }

            if (hasUnmergedUnits)
            {
                _unmergedTimeout += Time.deltaTime;

                if (_unmergedTimeout < 0.5f)
                {
                    hasUnmergedUnits = false;
                }
            }
            else
            {
                _unmergedTimeout = 0f;
            }
        }
        else
        {
            _unmergedTimeout = 0f;
        }

        if (!_newUnitPanelOpen && !_cardsListPanelOpen && (hasUnmergedUnits || LevelSettings.TutorialStage == 2 && UnitManager.Default.PlayerUnits.Count >= 2))
        {
            FloatingHandSwiping1Script.Active = true;
            FloatingHandSwiping1.alpha = Mathf.MoveTowards(FloatingHandSwiping1.alpha, 1f, Time.deltaTime * 5f);
        }
        else
        {
            FloatingHandSwiping1Script.Active = false;
            FloatingHandSwiping1.alpha = Mathf.MoveTowards(FloatingHandSwiping1.alpha, 0f, Time.deltaTime * 5f);
        }

        if (!_newUnitPanelOpen && !_cardsListPanelOpen && (hasUnmergedUnits && !GameData.Default.DisableAdditionalHands || LevelSettings.TutorialStage == 2 && UnitManager.Default.PlayerUnits.Count >= 2))
        {
            if (hasUnmergedUnits)
            {
                if (unit1.transform.localPosition.magnitude > unit2.transform.localPosition.magnitude)
                {
                    FloatingHandSwiping1Script.Position1.position = Camera.main.WorldToScreenPoint(unit1.transform.position);
                    FloatingHandSwiping1Script.Position2.position = Camera.main.WorldToScreenPoint(unit2.transform.position);
                }
                else
                {
                    FloatingHandSwiping1Script.Position1.position = Camera.main.WorldToScreenPoint(unit2.transform.position);
                    FloatingHandSwiping1Script.Position2.position = Camera.main.WorldToScreenPoint(unit1.transform.position);
                }
                FloatingHandSwiping1Script.DrawLines = true;
            }
            else
            {
                var targetUnit = UnitManager.Default.PlayerUnits[UnitManager.Default.PlayerUnits[0].Description.IsAnimal ? 0 : 1];
                FloatingHandSwiping1Script.Position1.position = Camera.main.WorldToScreenPoint(targetUnit.transform.position);
                FloatingHandSwiping1Script.Position2.position = Camera.main.WorldToScreenPoint(UnitManager.Default.PlayerGrid.GetCellPosition(targetUnit.GridCellIndex % 5));
                FloatingHandSwiping1Script.DrawLines = true;
            }
        }

        TutorialDarkenScreen2.alpha = Mathf.MoveTowards(TutorialDarkenScreen2.alpha, LevelSettings.TutorialStage != 4.5f || UnitManager.Default.PlayerUnits.Count < 3 ? 0f : 1f, Time.deltaTime * 5f);

        if (TutorialDarkenScreen2.alpha > 0f && Unit.GrabbedUnitsCount == 0 && !UnitManager.Default.PlayerUnits[0].IsAnimated && !UnitManager.Default.PlayerUnits[1].IsAnimated && !UnitManager.Default.PlayerUnits[2].IsAnimated)
        {
            var targetPosition = Vector3.Lerp(FloatingHandSwiping1Script.Position1.position, FloatingHandSwiping1Script.Position2.position, 0.5f);
            var targetScale = Vector3.one * Mathf.Clamp(Vector3.Distance(FloatingHandSwiping1Script.Position1.position, FloatingHandSwiping1Script.Position2.position) * 0.0055f, 1f, 2f);
            if (_initTutorialDarkenScreen2)
            {
                TutorialDarkenScreen2.transform.position = Vector3.Lerp(TutorialDarkenScreen2.transform.position, targetPosition, Time.deltaTime);
                TutorialDarkenScreen2.transform.localScale = Vector3.Lerp(TutorialDarkenScreen2.transform.localScale, targetScale, Time.deltaTime);
            }
            else
            {
                _initTutorialDarkenScreen2 = true;
                TutorialDarkenScreen2.transform.position = targetPosition;
                TutorialDarkenScreen2.transform.localScale = targetScale;
            }
        }
        else
        {
            _initTutorialDarkenScreen2 = false;
        }

        if (BossText.activeSelf != LevelSettings.Default.Description.IsBoss)
        {
            BossText.SetActive(LevelSettings.Default.Description.IsBoss);
        }

        NewUnitPanelBG.alpha = Mathf.MoveTowards(NewUnitPanelBG.alpha, _newUnitPanelOpenBG ? 1f : 0f, Time.deltaTime * 5f);
        NewUnitPanel.alpha = 1f;
        NewUnitPanel.interactable = _newUnitPanelOpen;
        NewUnitPanel.blocksRaycasts = NewUnitPanel.interactable;

        UnitCardsListBG.alpha = Mathf.MoveTowards(UnitCardsListBG.alpha, _cardsListPanelOpen ? 1f : 0f, Time.deltaTime * 5f);
        UnitCardsList.alpha = 1f;
        UnitCardsList.interactable = UnitCardsListBG.alpha > 0f;
        UnitCardsList.blocksRaycasts = UnitCardsList.interactable;

        if (_3dModelsEnabled && NewUnitPanelBG.alpha == 0f && UnitCardsListBG.alpha == 0f)
        {
            Disable3DModelSupport();
        }

        NewUnitCardGlow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(Time.time * 90f, 360f));

        if (_cardsListPanelOpen && UnitCardsListBG.alpha < 1f)
        {
            UnitCardsList.GetComponentInChildren<Scrollbar>().value = 1f;
        }

        TutorialStage1Panel.alpha = Mathf.MoveTowards(TutorialStage1Panel.alpha, LevelSettings.TutorialStage == -10 ? 1f : 0f, Time.deltaTime * 5f);
        TutorialStage1Panel.interactable = TutorialStage1Panel.alpha > 0f;
        TutorialStage1Panel.blocksRaycasts = TutorialStage1Panel.interactable;

        if (LevelSettings.TutorialStage == -10 && TutorialStage1Panel.alpha == 1f)
        {
            _tutorialStage1Timeout += Time.deltaTime;
        }
        else
        {
            _tutorialStage1Timeout = 0f;
        }

        TutorialStage1Button.alpha = Mathf.MoveTowards(TutorialStage1Button.alpha, _tutorialStage1Timeout > 2f ? 1f : 0f, Time.deltaTime * 5f);
        TutorialStage1Button.interactable = TutorialStage1Button.alpha > 0f;
        TutorialStage1Button.blocksRaycasts = TutorialStage1Button.interactable;

        TutorialStage2Panel.alpha = Mathf.MoveTowards(TutorialStage2Panel.alpha, LevelSettings.TutorialStage == -9 ? 1f : 0f, Time.deltaTime * 5f);

        if (LevelSettings.TutorialStage == -9 && TutorialStage2Panel.alpha == 1f)
        {
            _tutorialStage2Timeout += Time.deltaTime;
        }
        else
        {
            _tutorialStage2Timeout = 0f;
        }

        TutorialStage3Panel.alpha = Mathf.MoveTowards(TutorialStage3Panel.alpha, LevelSettings.TutorialStage == 0 && !LevelSettings.Default.IsFightActive ? 1f : 0f, Time.deltaTime * 5f);

        TutorialStage5Panel.alpha = Mathf.MoveTowards(TutorialStage5Panel.alpha, LevelSettings.TutorialStage == 1 && !LevelSettings.Default.IsFightActive ? 1f : 0f, Time.deltaTime * 5f);

        if (LevelSettings.TutorialStage == 3 && !LevelSettings.Default.IsFightActive && !_newUnitPanelOpen)
        {
            _tutorialStage7Timeout += Time.deltaTime;
        }
        else
        {
            _tutorialStage7Timeout = 0f;
        }

        TutorialStage7Panel.alpha = Mathf.MoveTowards(TutorialStage7Panel.alpha, _tutorialStage7Timeout > 0.25f ? 1f : 0f, Time.deltaTime * 5f);

        if (LevelSettings.TutorialStage == 5)
        {
            _tutorialStage8Timeout += Time.deltaTime;
        }
        else
        {
            _tutorialStage8Timeout = 0f;
        }

        TutorialStage8Panel.alpha = Mathf.MoveTowards(TutorialStage8Panel.alpha, _tutorialStage8Timeout > 0.25f ? 1f : 0f, Time.deltaTime * 5f);

        _mergeHelpTextAnim = Mathf.MoveTowards(_mergeHelpTextAnim, (!freeSpace || FightButton.alpha == 0f && !canBuy1 && !canBuy2 && LevelManager.Default.DifficultyCounter < 10 && LevelSettings.TutorialStage != 4.5f) && hasUnmergedUnits ? 1f : 0f, Time.deltaTime * 2f);
        MergeUnitsHelpText.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f + Mathf.Sin(Time.time * 5f) * 0.1f * _mergeHelpTextAnim, _mergeHelpTextAnim);

        if (_notEnoughMoneyTimer > 0f)
        {
            _notEnoughMoneyTimer -= Time.deltaTime;
        }

        NotEnoughMoneyHelpText.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_notEnoughMoneyTimer <= 0.5f ? _notEnoughMoneyTimer * 2f : 1f - (_notEnoughMoneyTimer - 2.5f) * 2f));

        /*if (FightButton.alpha == 0f && !canBuy1 && !canBuy2 && !hasUnmergedUnits && LevelManager.Default.DifficultyCounter < 10)
        {
            if (price1 > price2)
            {
                MoneyService.Default.AddMoney((ulong)Mathf.Max(0, price2 - MoneyService.Default.GetMoney()));
            }
            else
            {
                MoneyService.Default.AddMoney((ulong)Mathf.Max(0, price1 - MoneyService.Default.GetMoney()));
            }
        }*/

        if (!_mergingTutorialTrigger && LevelSettings.TutorialStage == 4.5f && UnitManager.Default.PlayerUnits.Count >= 3)
        {
            OpenMergingTutorial();
        }

        MergingTutorialGroup.alpha = Mathf.MoveTowards(MergingTutorialGroup.alpha, _mergingTutorialVisible ? 1f : 0f, Time.deltaTime * 2f);

        if (MergingTutorialGroup.gameObject.activeSelf)
        {
            if (MergingTutorialGroup.alpha == 0f)
            {
                MergingTutorialGroup.gameObject.SetActive(false);
            }

            _mergingTutorialButtonTimeout += Time.deltaTime;

            MergingTutorialButton.alpha = Mathf.MoveTowards(MergingTutorialButton.alpha, _mergingTutorialVisible && _mergingTutorialButtonTimeout > 2f ? 1f : 0f, Time.deltaTime * 5f);
            MergingTutorialButton.interactable = MergingTutorialButton.alpha > 0f;
        }
        else
        {
            if (MergingTutorialGroup.alpha > 0f)
            {
                MergingTutorialGroup.gameObject.SetActive(true);
            }
        }
    }

    private void Update()
    {
        DoGameplayUI();
    }

    public void OpenNewCardPanel(Unit unit)
    {
        if (_newUnitPanelOpen || !_newUnitPanelReady)
        {
            return;
        }

        _newCardUnit = unit;

        _newUnitPanelOpen = true;
        _newUnitPanelOpenBG = false;
        _newUnitPanelReady = false;
        NewUnitName.text = Language.Text(unit.Description.UnitName);
        NewUnitHP.text = MoneyService.AmountToString(unit.Description.HP);
        NewUnitDamage.text = MoneyService.AmountToString(unit.Damage);
        if (_newUnitExistingPreview != null)
        {
            Destroy(_newUnitExistingPreview.gameObject);
        }
        var preview = CardList.SpawnUnitPreview(unit.Description, true, null, false);

        preview.SetParent(NewUnitPreviewParent, false);
        preview.localPosition = unit.Description.CardPreviewOffset;
        preview.localScale = Vector3.one * unit.Description.CardPreviewScale;

        preview.GetComponent<Animator>().SetTrigger("Attack");
        _newUnitExistingPreview = preview;
        if (unit.Description.IsAnimal)
        {

            prewiewParent.anchoredPosition = (Vector2)prewiewPosition + new Vector2(0, 50);
        }
        else
        {
            prewiewParent.position = prewiewPosition;
        }
        var bg = NewUnitPanel.transform.Find("BG");

        if (_seq2 != null)
        {
            _seq2.Kill();
        }

        var cam = CameraSystem.Default.transform.Find("StartPosition");
        var cam2 = CameraSystem.Default.transform.Find("DefaultPosition");
        _startCamPos = cam.position;
        _startCamRot = cam.eulerAngles;

        var newCamPos = unit.transform.position + Vector3.back * 3f + Vector3.up * 3f;

        _seq2 = DOTween.Sequence();
        _seq2.Append(cam.DOMove(newCamPos, 1f).SetEase(Ease.InOutCubic));
        _seq2.Join(cam.DORotate(Quaternion.LookRotation((unit.transform.position + Vector3.up - newCamPos).normalized).eulerAngles, 1f).SetEase(Ease.InOutCubic));
        _seq2.Join(cam2.DOMove(newCamPos, 1f).SetEase(Ease.InOutCubic));
        _seq2.Join(cam2.DORotate(Quaternion.LookRotation((unit.transform.position + Vector3.up - newCamPos).normalized).eulerAngles, 1f).SetEase(Ease.InOutCubic));
        _seq2.AppendInterval(0.25f);
        _seq2.AppendCallback(() => _newUnitPanelOpenBG = true);
        _seq2.AppendCallback(() => Enable3DModelSupport());
        _seq2.Append(bg.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => _newUnitPanelReady = true));
    }

    public void CloseNewCardPanel()
    {
        if (!_newUnitPanelOpenBG)
        {
            return;
        }

        _newUnitPanelOpen = false;
        _newUnitPanelOpenBG = false;
        _newUnitPanelReady = false;
        var bg = NewUnitPanel.transform.Find("BG");

        if (_seq2 != null)
        {
            _seq2.Kill();
        }

        var cam = CameraSystem.Default.transform.Find("StartPosition");
        var cam2 = CameraSystem.Default.transform.Find("DefaultPosition");

        _seq2 = DOTween.Sequence();
        _seq2.AppendCallback(() => _newCardUnit.FinishSpawnNewUnitAnimation());
        _seq2.Append(cam.DOMove(_startCamPos, 0.5f).SetEase(Ease.InOutCubic));
        _seq2.Join(cam.DORotate(_startCamRot, 0.5f).SetEase(Ease.InOutCubic));
        _seq2.Join(cam2.DOMove(_startCamPos, 0.5f).SetEase(Ease.InOutCubic));
        _seq2.Join(cam2.DORotate(_startCamRot, 0.5f).SetEase(Ease.InOutCubic));
        _seq2.Join(bg.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InCubic).OnComplete(() => _newUnitPanelReady = true));

        if (LevelSettings.TutorialStage == -8)
        {
            LevelSettings.TutorialStage = 0;
        }
    }

    public void CardListOpen()
    {
        if (_cardsListPanelOpen || !_cardsListPanelReady)
        {
            return;
        }

        _cardsListPanelOpen = true;
        _cardsListPanelReady = false;
        CardListController.UpdateList();
        Enable3DModelSupport();
        var bg = UnitCardsList.transform.Find("BG");

        if (_seq1 != null)
        {
            _seq1.Kill();
        }
        _seq1 = DOTween.Sequence();
        _seq1.Append(bg.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => _cardsListPanelReady = true));
    }

    public void CardListClose()
    {
        if (!_cardsListPanelOpen)
        {
            return;
        }

        _cardsListPanelOpen = false;
        _cardsListPanelReady = false;
        var bg = UnitCardsList.transform.Find("BG");

        if (_seq1 != null)
        {
            _seq1.Kill();
        }
        _seq1 = DOTween.Sequence();
        _seq1.Append(bg.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InCubic).OnComplete(() => _cardsListPanelReady = true));
    }

    public void BounceNewCardPanelCloseButton()
    {
        NewUnitPanelClose.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            NewUnitPanelClose.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });
    }

    public void BounceCardListCloseButton()
    {
        UnitCardsListClose.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            UnitCardsListClose.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });
    }

    public void Enable3DModelSupport()
    {
        var c = GetComponentInParent<Canvas>();
        if (c == null)
        {
            return;
        }
        var rootCanvas = c.rootCanvas;
        rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        rootCanvas.worldCamera = Camera.main;
        rootCanvas.planeDistance = 1f;
        _3dModelsEnabled = true;
    }

    public void Disable3DModelSupport()
    {
        var c = GetComponentInParent<Canvas>();
        if (c == null)
        {
            return;
        }
        var rootCanvas = c.rootCanvas;
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.worldCamera = null;
        rootCanvas.planeDistance = 100f;
        _3dModelsEnabled = false;
    }

    public void RequestMouseProxy(IMouseEventProxyTarget target, bool onTop = false)
    {
        var proxy = new GameObject("proxy", typeof(RectTransform), typeof(MouseEventProxy), typeof(Image));
        proxy.transform.SetParent(transform);
        if (!onTop)
        {
            proxy.transform.SetAsFirstSibling();
        }
        proxy.GetComponent<MouseEventProxy>().Target = target;
        proxy.GetComponent<Image>().color = Color.clear;
    }

    public void TutorialStage1Next()
    {
        if (LevelSettings.TutorialStage == -10)
        {
            LevelSettings.TutorialStage++;

            TutorialStage1Button.transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
            {
                TutorialStage1Button.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
            });

            SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
        }
    }

    public void ShowNotEnoughMoney()
    {
        if (LevelSettings.TutorialStage < 7)
        {
            return;
        }

        if (!UnitManager.Default.PlayerGrid.HasFreeSpace())
        {
            return;
        }

        if (_notEnoughMoneyTimer > 0f)
        {
            if (_notEnoughMoneyTimer > 0.5f && _notEnoughMoneyTimer < 2.5f)
            {
                _notEnoughMoneyTimer = 2.5f;
            }
            return;
        }

        _notEnoughMoneyTimer = 3f;
    }

    public void OpenMergingTutorial()
    {
        _mergingTutorialVisible = true;
        _mergingTutorialTrigger = true;
        _mergingTutorialButtonTimeout = 0f;
    }

    public void CloseMergingTutorial()
    {
        _mergingTutorialVisible = false;
    }
}
