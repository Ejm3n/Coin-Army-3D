using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using BG.UI.Camera;
using DG.Tweening;
using TMPro;

public class TheftPanel : MonoBehaviour
{
    #region Singleton

    private static TheftPanel _default;
    public static TheftPanel Default => _default;

    #endregion

    [NonSerialized]
    public float MoneySpeed = 1f;

    public GameObject ChestPrefab;

    public TextMeshProUGUI ResultsText;
    public CanvasGroup ResultsGroup;

    public Image Avatar;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI UserNumber;

    public RectTransform OkButton;

    public Transform Bomb;
    public ParticleSystem BombExplosion;

    public RectTransform HeaderBg;
    public GameObject AvatarParent;

    [NonSerialized]
    public TheftChest[] _spawnedChests;

    public GameObject Crack;

    private bool _showResults;

    [NonSerialized]
    public List<int> UnclaimedPrizes;

    private Vector3[] _chestPositions = new Vector3[]
    {
        new Vector3(-0.5f, 0f, 4.5f),
        new Vector3(0.5f, 0f, 4.5f),
        new Vector3(-0.5f, 0f, 3.5f),
        new Vector3(0.5f, 0f, 3.5f),
    };

    [NonSerialized]
    public bool AcceptPresses;

    public ParticleSystem[] MoneyFlow;

    public CanvasGroup[] MoneyTexts;

    private int _pressedTickets;

    [NonSerialized]
    public double TotalReward;

    public TextMeshProUGUI FillBarText;
    public TextMeshProUGUI FillBarMultiplier;
    public Image FillBarFill;
    public RectTransform FillScaled;

    private double moneyLeft;

    private float visualReward;

    private float[] barTimer = new float[4];

    private double[] lastReward = new double[4];
    private double[] lastReward2 = new double[4];

    void OnEnable()
    {
        _default = this;

        if (UnitManager.Default == null || UnitManager.Default.PlayerGrid == null)
        {
            return;
        }

        UnitManager.Default.PlayerGrid.gameObject.SetActive(false);
        UnitManager.Default.EnemyGrid.gameObject.SetActive(false);

        foreach (var playerUnit in UnitManager.Default.PlayerUnits)
        {
            playerUnit.gameObject.SetActive(false);
        }

        foreach (var enemyUnit in UnitManager.Default.EnemyUnits)
        {
            enemyUnit.gameObject.SetActive(false);
        }

        _spawnedChests = new TheftChest[4];
        for (int i = 0; i < 4; i++)
        {
            _spawnedChests[i] = Instantiate(ChestPrefab).GetComponent<TheftChest>();
            _spawnedChests[i].transform.position = _chestPositions[i];
            _spawnedChests[i].panel = this;
        }

        ResultsGroup.alpha = 0f;
        _showResults = false;

        PrepareHeader();
        Transition.Default.DoOpponentIntro(false, DoHeaderAnim);

        UnclaimedPrizes = new List<int>() { 0, 1, 2, 3 };

        if (PlayerPrefs.GetInt("DidTheft") == 0)
        {
            UnclaimedPrizes.Remove(3);
        }

        AcceptPresses = true;

        _pressedTickets = 0;
        TotalReward = 0;
        visualReward = 0;

        for (int i = 0; i < 4; i++)
        {
            barTimer[i] = 0;
            lastReward[i] = 0;
            lastReward2[i] = 0;
        }

        moneyLeft = OpponentService.Default.MoneyLeft;

        foreach (var flow in MoneyFlow)
        {
            flow.gameObject.SetActive(true);
        }

        Crack.SetActive(false);

        MoneySpeed = 1f;

        foreach (var text in MoneyTexts)
        {
            text.gameObject.SetActive(false);
        }
    }

    void PrepareHeader()
    {
        HeaderBg.anchoredPosition = new Vector3(-667f, -14.29999f);
        Name.color = new Color(1f, 1f, 1f, 0f);
        UserNumber.color = new Color(1f, 1f, 1f, 0f);
        AvatarParent.SetActive(false);
    }

    void DoHeaderAnim()
    {
        HeaderBg.DOAnchorPos(new Vector3(-13f, -14.29999f), 0.5f).SetEase(Ease.OutCubic);
        Name.DOFade(1f, 0.5f).SetEase(Ease.OutCubic);
        UserNumber.DOFade(1f, 0.5f).SetEase(Ease.OutCubic);
        AvatarParent.SetActive(true);
    }
    
    void OnDisable()
    {
        if (UnitManager.Default == null || UnitManager.Default.PlayerGrid == null)
        {
            return;
        }

        CameraSystem.Default.CurentState = CameraState.Default;

        UnitManager.Default.PlayerGrid.gameObject.SetActive(true);
        UnitManager.Default.EnemyGrid.gameObject.SetActive(true);

        foreach (var playerUnit in UnitManager.Default.PlayerUnits)
        {
            playerUnit.gameObject.SetActive(true);
        }

        foreach (var enemyUnit in UnitManager.Default.EnemyUnits)
        {
            enemyUnit.gameObject.SetActive(true);
        }

        foreach (var chest in _spawnedChests)
        {
            if (chest)
            {
                Destroy(chest.gameObject);
            }
        }

        foreach (var flow in MoneyFlow)
        {
            if (flow)
            {
                flow.gameObject.SetActive(false);
            }
        }

        OpponentService.Default.ChangeOpponent();
    }

    void Update()
    {
        if (CameraSystem.Default.CurentState != CameraState.Theft)
        {
            CameraSystem.Default.CurentState = CameraState.Theft;
        }

        ResultsGroup.alpha = Mathf.MoveTowards(ResultsGroup.alpha, _showResults ? 1f : 0f, Time.deltaTime * 5f);
        ResultsGroup.interactable = ResultsGroup.alpha == 1f;
        ResultsGroup.blocksRaycasts = ResultsGroup.interactable;

        Avatar.sprite = OpponentService.Default.Description.Avatar;
        Name.text = OpponentService.Default.Description.Username;
        UserNumber.text = string.Format(Language.Text("MoneyAmount"), moneyLeft.ToString("N0"));

        for (int i = 0; i < 4; i++)
        {
            if (barTimer[i] > 1f)
            {
                if (lastReward2[i] > 0)
                {
                    float delta = Mathf.Min((float)lastReward2[i], Time.deltaTime * ((float)(moneyLeft * SlotMachine.Multiplier + lastReward[i]) * 0.1f) * 0.7f * MoneySpeed);
                    lastReward2[i] -= delta;
                    visualReward += delta;
                }
                else
                {
                    barTimer[i] = 0f;
                }
            }
            else
            {
                if (lastReward2[i] > 0)
                {
                    barTimer[i] += Time.deltaTime * MoneySpeed;
                }
                else
                {
                    barTimer[i] = 0f;
                }
            }
        }

        FillBarFill.fillAmount = (float)(visualReward / moneyLeft * SlotMachine.Multiplier);
        string newText = string.Format("<sprite index=0> {0}/{1}", Mathf.RoundToInt(visualReward).ToString("N0"), (moneyLeft * SlotMachine.Multiplier).ToString("N0"));

        if (newText != FillBarText.text)
        {
            FillBarText.text = newText;

            if (FillScaled.localScale == Vector3.one)
            {
                FillScaled.DOScale(Vector3.one * 1.01f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
                {
                    FillScaled.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
                });
            }
        }

        FillBarMultiplier.text = $"X{SlotMachine.Multiplier}";

        foreach (var flow in MoneyFlow)
        {
            var main = flow.main;
            main.simulationSpeed = MoneySpeed;
        }

        foreach (var text in MoneyTexts)
        {
            if (text.gameObject.activeSelf)
            {
                text.alpha -= Time.deltaTime;
                if (text.alpha == 0f)
                {
                    text.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowResults(ulong result, bool success)
    {
        if (success)
        {
            _showResults = true;
            ResultsText.text = string.Format(Language.Text("MoneyAmount"), result.ToString("N0"));
            var c = ColorUtility.ToHtmlStringRGB(GameData.Default.TextAccentColor);
            ResultsText.text = string.Format(Language.Text("YouStoleAmount"), c, result.ToString("N0"));
        }
        else
        {
            DoBombExplosion(() => ScreenManager.Default.GoToSlotScreen(() => OnDisable()));
        }
    }

    public void Hide()
    {
        if (!_showResults)
        {
            return;
        }

        ScreenManager.Default.GoToSlotScreen(() => OnDisable());

        OkButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            OkButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        PlayerPrefs.SetInt("DidTheft", 1);

        if (LevelSettings.TutorialStage == 13)
        {
            LevelSettings.TutorialStage++;
        }

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void DoBombExplosion(Action onComplete)
    {
        Bomb.gameObject.SetActive(true);
        Bomb.localScale = Vector3.zero;

        var seq = DOTween.Sequence();

        seq.Append(Bomb.DOScale(Vector3.one, 1f).SetEase(Ease.InOutCubic));
        seq.AppendInterval(1f);
        seq.AppendCallback(() => BombExplosion.Play());
        seq.AppendCallback(() => SoundHolder.Default.PlayFromSoundPack("Explosion", allowPitchShift: false));
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() => Crack.SetActive(true));
        seq.AppendCallback(() => Bomb.gameObject.SetActive(false));
        seq.AppendInterval(1f);
        seq.AppendCallback(() => onComplete());
    }

    public void RequestMouseProxy(IMouseEventProxyTarget target)
    {
        var proxy = new GameObject("proxy", typeof(RectTransform), typeof(MouseEventProxy), typeof(Image));
        proxy.transform.SetParent(transform);
        proxy.transform.SetAsFirstSibling();
        proxy.GetComponent<MouseEventProxy>().Target = target;
        proxy.GetComponent<Image>().color = Color.clear;
    }

    public void OnTicketPress(int index)
    {
        _pressedTickets++;

        if (_spawnedChests[index].Result)
        {
            lastReward[index] = OpponentService.Default.MoneyLeft * _spawnedChests[index].RewardPercent * SlotMachine.Multiplier;
            lastReward2[index] = lastReward[index];

            TotalReward += lastReward[index];

            MoneyFlow[index].transform.position = _spawnedChests[index].transform.position;
            var main = MoneyFlow[index].main;
            main.duration = (float)(lastReward[index]) / ((float)(moneyLeft * SlotMachine.Multiplier + lastReward[index]) * 0.1f);
            MoneyFlow[index].Play();

            var moneyText = MoneyTexts[index].GetComponent<TextMeshProUGUI>();
            var moneyTextRT = moneyText.GetComponent<RectTransform>();

            moneyText.gameObject.SetActive(true);
            moneyText.text = $"<sprite=0> {lastReward[index].ToString("N0")}";

            moneyTextRT.position = Camera.main.WorldToScreenPoint(_spawnedChests[index].transform.position);
            moneyTextRT.anchoredPosition += Vector2.up * 100f;
            moneyTextRT.DOAnchorPos(moneyTextRT.anchoredPosition + Vector2.up * 50f, 1f);

            SoundHolder.Default.PlayFromSoundPack("MoneySound");

            if (_pressedTickets >= 3)
            {
                ShowResults(true);
            }
        }
        else
        {
            ShowResults(false);
        }
    }

    void ShowResults(bool result)
    {
        AcceptPresses = false;
        foreach (var ticket in _spawnedChests)
        {
            ticket._isOpen = true;
        }

        MoneyService.Default.AddMoney((ulong)TotalReward);

        OpponentService.Default.MoneyLeft -= (ulong)Mathf.Min((int)OpponentService.Default.MoneyLeft, (int)TotalReward);

        ShowResults((ulong)TotalReward, result);

        MoneySpeed = 1.5f;
    }
}
