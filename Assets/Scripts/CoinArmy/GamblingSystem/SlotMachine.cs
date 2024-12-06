using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using DG.Tweening;
using BG.UI.Main;
using MoreMountains.NiceVibrations;

public class SlotMachine : MonoBehaviour
{
    public static bool IsTransitioning = false;
    public static bool IsIdling = true;
    public static bool IsReady = true;
    public static int NextMultiplier = 1;
    public static bool SpecialBet;
    public static int Multiplier = 1;
    public static bool ShowsJackpotAnimationType2 = false;
    public static bool ForceSpinBalls = false;

    public LottoBall[] Balls;
    public Transform[] InternalPivots;
    public Transform[] ExitPivots;
    public float InnerRadius;
    [NonSerialized]
    public bool Rotating;
    public int MaxBallType;

    public Sprite MoneyIcon;
    public Sprite SpinsIcon;

    public ParticleSystem MoneyRain;
    public ParticleSystem GoldRain;

    public ParticleSystem MoneyRainBurst;
    public ParticleSystem GoldRainBurst;
    public ParticleSystem MoneyRainBurst2;
    public ParticleSystem GoldRainBurst2;

    public ParticleSystem BallSparkle;

    public Animator AttackOverlay;

    public SpriteRenderer DarkBackground;

    private List<LottoBall> _pulledBalls = new List<LottoBall>();

    private int _slotState = 6;

    // states:
    // 0 - spinning
    // 1 - pulled 1 ball
    // 2 - pulled 2 balls
    // 3 - pulled 3 balls
    // 4 - turn all balls around

    private GameData.BallType[] _desiredCombination = new GameData.BallType[] { GameData.BallType.Nothing, GameData.BallType.Nothing, GameData.BallType.Nothing };

    private float _timer;
    private float _maxTime;

    private Sequence _jackpotType2Sequence;

    private AudioSource _rainSoundGold;
    private AudioSource _rainSoundDollar;
    private float _rainSoundGoldVolume;
    private float _rainSoundDollarVolume;

    private bool _readyToBeDone;

    void OnEnable()
    {
        IsTransitioning = false;
        _slotState = 6;
        Rotating = false;
        GenerateNewDesiredCombination(false);
        EndPowerJackpotAnim();
        EndShieldJackpotAnim();
        foreach (var ball in _pulledBalls)
        {
            ball.PushBall(true);
        }
        _pulledBalls.Clear();
        MoneyRain.Stop();
        GoldRain.Stop();
        MoneyRainBurst.Stop();
        MoneyRainBurst2.Stop();
        GoldRainBurst.Stop();
        GoldRainBurst2.Stop();
        _jackpotType2Sequence?.Kill();
        ForceSpinBalls = false;
        _rainSoundGold = SoundHolder.Default.PlayFromSoundPack("GoldFlow", true, allowPitchShift: false);
        _rainSoundDollar = SoundHolder.Default.PlayFromSoundPack("DollarFlow", true, allowPitchShift: false);
        _rainSoundGoldVolume = _rainSoundGold.volume;
        _rainSoundDollarVolume = _rainSoundDollar.volume;
        _rainSoundGold.volume = 0f;
        _rainSoundDollar.volume = 0f;
        if (UIManager.Default == null)
        {
            Debug.LogError("UI Manager does not exist!");
            return;
        }
        var uiManager = UIManager.Default.GetPanel(UIState.Lotto);
        var lottoPanel = uiManager.GetComponent<LottoPanel>();
        if (MissionService.Default == null)
        {
            Debug.LogError("Mission Service does not exist!");
            return;
        }
        MissionService.Default.ProgressMission(0, lottoPanel.DoMissionResultScreen);
    }

    void OnDisable()
    {
        if (_slotState < 4)
        {
            GiveReward(true);
        }
        if (_rainSoundGold)
        {
            Destroy(_rainSoundGold.gameObject);
        }
        if (_rainSoundDollar)
        {
            Destroy(_rainSoundDollar.gameObject);
        }
    }

    void Update()
    {
        switch (_slotState)
        {
            case 6:
                break;
            case 3:
                bool allReady = true;
                foreach (var ball in _pulledBalls)
                {
                    if (!ball.IsReady)
                    {
                        allReady = false;
                        break;
                    }
                }
                if (allReady)
                {
                    Next();
                }
                break;
            default:
                if (_slotState != 5 || !ShowsJackpotAnimationType2)
                {
                    _timer += Time.deltaTime;
                }
                if (_slotState == 5)
                {
                    _maxTime = 1.25f;
                }
                if (_timer >= _maxTime)
                {
                    Next();
                }
                break;
        }
        _rainSoundGold.volume = Mathf.MoveTowards(_rainSoundGold.volume, GoldRain.isPlaying ? _rainSoundGoldVolume : 0f, Time.deltaTime);
        _rainSoundDollar.volume = Mathf.MoveTowards(_rainSoundDollar.volume, MoneyRain.isPlaying ? _rainSoundDollarVolume : 0f, Time.deltaTime);

        IsReady = _slotState >= 5;
        IsIdling = _slotState == 6;

        if (!IsReady)
        {
            _readyToBeDone = true;
        }
        else if (_readyToBeDone)
        {
            _readyToBeDone = false;

            if (SpinsService.Default.GetSpins() == 0 && UIManager.Default.CurrentState == UIState.Lotto)
            {
                UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().DoNoSpinsPromotion();
            }
        }
    }

    private static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
    {
        var cnt = new Dictionary<T, int>();
        foreach (T s in list1)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]++;
            }
            else
            {
                cnt.Add(s, 1);
            }
        }
        foreach (T s in list2)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]--;
            }
            else
            {
                return false;
            }
        }
        return cnt.Values.All(c => c == 0);
    }

    void GiveReward(bool silent)
    {
        int combinationIndex = DetermineCombination();

        if (GameData.Default.LottoCombinations[combinationIndex].Combination.Length == 0)
        {
            GameData.RewardType[] rewards = new GameData.RewardType[3];
            ulong totalMoney = 0;

            for (int i = 0; i < _desiredCombination.Length; i++)
            {
                for (int j = 0; j < GameData.Default.LottoRewards.Length; j++)
                {
                    if (GameData.Default.LottoRewards[j].BallType == _desiredCombination[i])
                    {
                        rewards[i] = GameData.Default.LottoRewards[j].RewardType;
                        switch (GameData.Default.LottoRewards[j].RewardType)
                        {
                            case GameData.RewardType.Money:
                                ulong m = (ulong)((GameData.Default.LottoRewards[j].RewardAmount + GameData.Default.LottoRewards[j].RewardRaise * (ulong)LevelManager.Default.DifficultyCounter) * (ulong)Multiplier);
                                MoneyService.Default.AddMoney(m);
                                totalMoney += m;
                                break;
                        }
                        break;
                    }
                }
            }

            if (totalMoney > 0)
            {
                UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().DisplayIncome(totalMoney, false);
                SoundHolder.Default.PlayFromSoundPack("MoneySound");

                var dollarCount = _desiredCombination.Where(x => x == GameData.BallType.Money).Count();
                var goldCount = _desiredCombination.Where(x => x == GameData.BallType.Gold).Count();

                if (dollarCount == 2)
                {
                    MoneyRainBurst2.Play();
                }
                else if (dollarCount == 1)
                {
                    MoneyRainBurst.Play();
                }
                
                if (goldCount == 2)
                {
                    GoldRainBurst2.Play();
                }
                else if (goldCount == 1)
                {
                    GoldRainBurst.Play();
                }

                MMVibrationManager.Haptic(HapticTypes.MediumImpact, false, true, this);
            }
        }
        else
        {
            switch (GameData.Default.LottoCombinations[combinationIndex].RewardType)
            {
                case GameData.RewardType.Money:
                    var totalMoney = (ulong)((GameData.Default.LottoCombinations[combinationIndex].RewardAmount + GameData.Default.LottoCombinations[combinationIndex].RewardRaise * (ulong)LevelManager.Default.DifficultyCounter) * (ulong)Multiplier);
                    MoneyService.Default.AddMoney(totalMoney);
                    if (totalMoney > 0)
                    {
                        UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().DisplayIncome(totalMoney, true);
                    }
                    MMVibrationManager.Haptic(HapticTypes.MediumImpact, false, true, this);
                    break;
                case GameData.RewardType.Spins:
                    ShowsJackpotAnimationType2 = true;
                    StartCoroutine(DelayedAddSpins(((int)GameData.Default.LottoCombinations[combinationIndex].RewardAmount + (int)GameData.Default.LottoCombinations[combinationIndex].RewardRaise * LevelManager.Default.DifficultyCounter) * Multiplier));
                    break;
                case GameData.RewardType.Defend:
                    ShowsJackpotAnimationType2 = true;
                    StartCoroutine(DelayedAddShields(((int)GameData.Default.LottoCombinations[combinationIndex].RewardAmount + (int)GameData.Default.LottoCombinations[combinationIndex].RewardRaise * LevelManager.Default.DifficultyCounter) * Multiplier));
                    break;
                case GameData.RewardType.Attack:
                case GameData.RewardType.Theft:
                    IsTransitioning = true;
                    break;
            }

            if (!silent && !string.IsNullOrEmpty(GameData.Default.LottoCombinations[combinationIndex].AnimationName))
            {
                Invoke(GameData.Default.LottoCombinations[combinationIndex].AnimationName, 0f);
            }
        }

        int missionProgressAmount = 0;
        var currentMission = MissionService.Default.GetCurrentMission();
        var missionBalls = new List<int>();

        for (int i = 0; i < _desiredCombination.Length; i++)
        {
            if (_desiredCombination[i] == currentMission.TargetBallType)
            {
                missionProgressAmount++;
                missionBalls.Add(i);
            }
        }

        if (missionProgressAmount == 3)
        {
            missionProgressAmount = GameData.Default.MissionProgressOnJackpot;
        }

        if (LevelSettings.TutorialStage < 10)
        {
            missionProgressAmount = 0;
        }

        if (missionProgressAmount > 0)
        {
            var panel = UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>();

            for (int i = 0; i < missionBalls.Count; i++)
            {
                Action onComplete = () => { };

                if (i == 0)
                {
                    onComplete = () => MissionService.Default.ProgressMission(missionProgressAmount * Multiplier, UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().DoMissionResultScreen);

                    SoundHolder.Default.PlayFromSoundPack("HeartSound");
                }

                panel.DoMissionAnim(_pulledBalls[missionBalls[i]], missionBalls[i], onComplete);
            }
        }
    }

    IEnumerator DelayedAddSpins(int amount)
    {
        yield return new WaitUntil(() => !ShowsJackpotAnimationType2);
        SpinsService.Default.AddSpins(amount, false);
    }

    IEnumerator DelayedAddShields(int amount)
    {
        yield return new WaitUntil(() => !ShowsJackpotAnimationType2);
        ShieldsService.Default.AddShields(amount);
        UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().PulseShields();
    }

    int DetermineCombination()
    {
        for (int i = 0; i < GameData.Default.LottoCombinations.Length; i++)
        {
            if (GameData.Default.LottoCombinations[i].Combination.Length == 0 || ScrambledEquals(GameData.Default.LottoCombinations[i].Combination, _desiredCombination))
            {
                return i;
            }
        }

        return GameData.Default.LottoCombinations.Length - 1;
    }

    void GenerateNewDesiredCombination(bool raiseProbability)
    {
        int spinNumber = PlayerPrefs.GetInt("SpinsCount");

        if (spinNumber == 12)
        {
            if (LevelSettings.TutorialStage == 8)
            {
                LevelSettings.TutorialStage++;
            }

            if (LevelSettings.TutorialStage == 9)
            {
                LevelSettings.TutorialStage++;
            }
        }

        float maxProbability = 0f;

        for (int i = 0; i < GameData.Default.LottoCombinations.Length; i++)
        {
            if (!GameData.Default.LottoCombinations[i].Enabled)
            {
                continue;
            }

            maxProbability += GameData.Default.LottoCombinations[i].GetCurrentProbability();
        }

        maxProbability = Mathf.Min(maxProbability, 100f);

        float magicValue = UnityEngine.Random.Range(0f, maxProbability);
        float magicValue2 = UnityEngine.Random.Range(0f, 100f);

        float counter = 0f;

        int result = GameData.Default.LottoCombinations.Length - 1;

        var sortedCombos = GameData.Default.LottoCombinations.Where(x => x.Enabled).OrderByDescending(x => x.GetCurrentProbability()).ToArray();

        for (int i = 0; i < sortedCombos.Length; i++)
        {
            float prob = sortedCombos[i].GetCurrentProbability();
            if (prob > 0f)
            {
                counter += prob;
                if (counter >= magicValue)
                {
                    result = Array.IndexOf(GameData.Default.LottoCombinations, sortedCombos[i]);
                    break;
                }
            }
        }

        float currentComboProb = PlayerPrefs.GetFloat("CombinationProbability", GameData.Default.CombinationProbability);

        if (magicValue2 > currentComboProb || sortedCombos.Length == 0)
        {
            result = GameData.Default.LottoCombinations.Length - 1;
        }

        if (GameData.Default.UseTestCombination)
        {
            _desiredCombination[0] = GameData.Default.TestCombination.FirstBall;
            _desiredCombination[1] = GameData.Default.TestCombination.SecondBall;
            _desiredCombination[2] = GameData.Default.TestCombination.ThirdBall;
            
            return;
        }

        if (GameData.Default.FakeFirstSpins && spinNumber < GameData.Default.FakeFirstSpinsValues.Length)
        {
            _desiredCombination[0] = GameData.Default.FakeFirstSpinsValues[spinNumber].FirstBall;
            _desiredCombination[1] = GameData.Default.FakeFirstSpinsValues[spinNumber].SecondBall;
            _desiredCombination[2] = GameData.Default.FakeFirstSpinsValues[spinNumber].ThirdBall;

            if (raiseProbability)
            {
                PlayerPrefs.SetInt("SpinsCount", spinNumber + 1);
            }
            
            return;
        }

        if (raiseProbability)
        {
            if (result == GameData.Default.LottoCombinations.Length - 1)
            {
                PlayerPrefs.SetFloat("CombinationProbability", Mathf.Min(currentComboProb + GameData.Default.CombinationProbabilityRaise, GameData.Default.MaxCombinationProbability));
            }
            else
            {
                PlayerPrefs.SetFloat("CombinationProbability", GameData.Default.CombinationProbability);
            }
        }

        if (GameData.Default.LottoCombinations[result].Combination.Length > 0)
        {
            for (int i = 0; i < _desiredCombination.Length; i++)
            {
                _desiredCombination[i] = GameData.Default.LottoCombinations[result].Combination[i];
            }
        }
        else
        {
            int loopCount = 0;

            while (true)
            {
                int loopCount2 = 0;

                for (int i = 0; i < _desiredCombination.Length; i++)
                {
                    _desiredCombination[i] = (GameData.BallType)UnityEngine.Random.Range(0, MaxBallType + 1);

                    if (loopCount2 < 32)
                    {
                        for (int j = 0; j < GameData.Default.LottoRewards.Length; j++)
                        {
                            if (GameData.Default.LottoRewards[j].BallType == _desiredCombination[i])
                            {
                                if (!GameData.Default.LottoRewards[j].Enabled)
                                {
                                    i--;
                                    loopCount2++;
                                    break;
                                }

                                break;
                            }
                        }
                    }
                }

                int combinationIndex = DetermineCombination();

                if (loopCount >= 32 || combinationIndex == GameData.Default.LottoCombinations.Length - 1)
                {
                    break;
                }

                loopCount++;
            }
        }

        if (raiseProbability)
        {
            string debugOutput = "Current combo probabilities:";

            for (int i = 0; i < GameData.Default.LottoCombinations.Length; i++)
            {
                if (!GameData.Default.LottoCombinations[i].Enabled || GameData.Default.LottoCombinations[i].Combination.Length == 0)
                {
                    continue;
                }

                if (result != GameData.Default.LottoCombinations.Length - 1)
                {
                    GameData.Default.LottoCombinations[i].UpdateProbability(i == result);
                }

                debugOutput += "\n" + GameData.Default.LottoCombinations[i].Name + ": " + GameData.Default.LottoCombinations[i].GetCurrentProbability() + " (~" + Mathf.RoundToInt(GameData.Default.LottoCombinations[i].GetCurrentProbability() / maxProbability * 100f) + "%)";
            }

            debugOutput += "\nOverall combo probability: " + PlayerPrefs.GetFloat("CombinationProbability") + "%";

            Debug.Log(debugOutput);

            PlayerPrefs.SetInt("SpinsCount", spinNumber + 1);
        }
    }

    void PullNextBall()
    {
        while (true)
        {
            var ball = Balls[UnityEngine.Random.Range(0, Balls.Length)];
            if ((_desiredCombination[_slotState - 1] == GameData.BallType.Nothing || ball.BallType == _desiredCombination[_slotState - 1]) && !_pulledBalls.Contains(ball))
            {
                _pulledBalls.Add(ball);
                ball.PullBall(ExitPivots[_slotState - 1]);
                break;
            }
        }
    }

    public void Activate()
    {
        if (IsReady)
        {
            _slotState = 6;
            Next();
        }
    }

    void Next()
    {
        if (IsTransitioning)
        {
            return;
        }

        _timer = 0f;
        _maxTime = UnityEngine.Random.Range(GameData.Default.SlotMachineMinDelay, GameData.Default.SlotMachineMaxDelay);
        
        if (_slotState == 6)
        {
            if (SpinsService.Default.GetSpins() < NextMultiplier)
            {
                return;
            }
            for (int i = 0; i < NextMultiplier; i++)
            {
                SpinsService.Default.SpendSpin();
            }
        }

        _slotState++;

        if (_slotState > 6)
        {
            _slotState = 0;
        }

        switch (_slotState)
        {
            case 0:
                Multiplier = NextMultiplier;
                EndPowerJackpotAnim();
                EndShieldJackpotAnim();
                foreach (var ball in _pulledBalls)
                {
                    ball.PushBall(false);
                }
                _pulledBalls.Clear();
                GenerateNewDesiredCombination(true);
                Rotating = true;
                UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().HideIncome();
                MoneyRain.Stop();
                GoldRain.Stop();
                MoneyRainBurst.Stop();
                MoneyRainBurst2.Stop();
                GoldRainBurst.Stop();
                GoldRainBurst2.Stop();
                _jackpotType2Sequence?.Kill();
                ForceSpinBalls = false;
                break;
            case 1:
            case 2:
            case 3:
                PullNextBall();
                if (_slotState == 3)
                {
                    Rotating = false;
                }
                break;
            case 4:
                _slotState = 5;
                foreach (var ball in _pulledBalls)
                {
                    ball.StopBall();
                }
                GiveReward(false);
                break;
        }
    }

    void ShieldJackpotAnim()
    {
        if (_slotState < 5)
        {
            return;
        }
        foreach (var ball in _pulledBalls)
        {
            ball.Pulse();
        }
        _jackpotType2Sequence = DOTween.Sequence();
        _jackpotType2Sequence.AppendInterval(0.5f);
        _jackpotType2Sequence.AppendCallback(() => ForceSpinBalls = true);
        _jackpotType2Sequence.Append(DarkBackground.DOColor(new Color(0f, 0f, 0f, 0.5f), 0.5f));
        _jackpotType2Sequence.Append(_pulledBalls[0].transform.DOMove(_pulledBalls[1].transform.position, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[2].transform.DOMove(_pulledBalls[1].transform.position, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.AppendInterval(0.5f);
        _jackpotType2Sequence.AppendCallback(() =>
        {
            UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().DoShieldAnim(_pulledBalls[1], EndShieldJackpotAnim);
        });
        _jackpotType2Sequence.Append(_pulledBalls[0].transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[1].transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[2].transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(DarkBackground.DOColor(Color.clear, 0.25f));
        SoundHolder.Default.PlayFromSoundPack("ShieldJackpot", allowPitchShift: false);
    }

    void EndShieldJackpotAnim()
    {
        if (_pulledBalls.Count == 0 || !ShowsJackpotAnimationType2)
        {
            return;
        }
        ShowsJackpotAnimationType2 = false;
        ForceSpinBalls = false;
        _pulledBalls[0].ResetScale();
        _pulledBalls[1].ResetScale();
        _pulledBalls[2].ResetScale();
        _pulledBalls[0].transform.position = _pulledBalls[1].transform.position + Vector3.up * 10f;
        _pulledBalls[1].transform.position = _pulledBalls[1].transform.position + Vector3.up * 10f;
        _pulledBalls[2].transform.position = _pulledBalls[1].transform.position + Vector3.up * 10f;
        DarkBackground.color = Color.clear;
    }

    void PowerJackpotAnim()
    {
        if (_slotState < 5)
        {
            return;
        }
        foreach (var ball in _pulledBalls)
        {
            ball.Pulse();
        }
        _jackpotType2Sequence = DOTween.Sequence();
        _jackpotType2Sequence.AppendInterval(0.5f);
        _jackpotType2Sequence.AppendCallback(() => ForceSpinBalls = true);
        _jackpotType2Sequence.Join(DarkBackground.DOColor(new Color(0f, 0f, 0f, 0.5f), 0.5f));
        _jackpotType2Sequence.AppendCallback(() => 
        {
            BallSparkle.Play();
            BallSparkle.transform.SetParent(_pulledBalls[1].transform);
            BallSparkle.transform.localPosition = Vector3.zero;
        });
        _jackpotType2Sequence.Append(_pulledBalls[0].transform.DOMove(_pulledBalls[1].transform.position, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[2].transform.DOMove(_pulledBalls[1].transform.position, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.AppendInterval(0.5f);
        _jackpotType2Sequence.Append(_pulledBalls[0].transform.DOMove(_pulledBalls[1].transform.position + Vector3.down * 0.75f, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[1].transform.DOMove(_pulledBalls[1].transform.position + Vector3.down * 0.75f, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[2].transform.DOMove(_pulledBalls[1].transform.position + Vector3.down * 0.75f, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[0].transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[1].transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(_pulledBalls[2].transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InCubic));
        _jackpotType2Sequence.Join(DarkBackground.DOColor(Color.clear, 0.25f));
        _jackpotType2Sequence.AppendCallback(() => BallSparkle.Stop());
        _jackpotType2Sequence.OnComplete(EndPowerJackpotAnim);
        SoundHolder.Default.PlayFromSoundPack("EnergyJackpot", allowPitchShift: false);
    }

    void EndPowerJackpotAnim()
    {
        if (_pulledBalls.Count == 0 || !ShowsJackpotAnimationType2)
        {
            return;
        }
        ShowsJackpotAnimationType2 = false;
        ForceSpinBalls = false;
        _pulledBalls[0].ResetScale();
        _pulledBalls[1].ResetScale();
        _pulledBalls[2].ResetScale();
        _pulledBalls[0].transform.position = _pulledBalls[1].transform.position + Vector3.up * 10f;
        _pulledBalls[1].transform.position = _pulledBalls[1].transform.position + Vector3.up * 10f;
        _pulledBalls[2].transform.position = _pulledBalls[1].transform.position + Vector3.up * 10f;
        DarkBackground.color = Color.clear;
        BallSparkle.Stop();
        BallSparkle.transform.SetParent(transform);
    }

    void MoneyJackpotAnim()
    {
        if (_slotState < 5)
        {
            return;
        }
        foreach (var ball in _pulledBalls)
        {
            ball.Pulse();
        }
        MoneyRain.Play();
        _rainSoundDollar.Play();
        SoundHolder.Default.PlayFromSoundPack("DollarJackpot", allowPitchShift: false);
    }

    void GoldJackpotAnim()
    {
        if (_slotState < 5)
        {
            return;
        }
        foreach (var ball in _pulledBalls)
        {
            ball.Pulse();
        }
        GoldRain.Play();
        _rainSoundGold.Play();
        SoundHolder.Default.PlayFromSoundPack("GoldJackpot", allowPitchShift: false);
    }

    void DamageJackpotAnim()
    {
        IsTransitioning = true;
        foreach (var ball in _pulledBalls)
        {
            ball.Pulse();
        }
        AttackOverlay.gameObject.SetActive(true);
        var mask = AttackOverlay.transform.GetChild(0);
        mask.localScale = Vector3.zero;
        mask.DOScale(Vector3.one, 1f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            mask.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InCubic).SetDelay(1f).OnComplete(() =>
            {
                ScreenManager.Default.GoToAttackScreen(() =>
                {
                    AttackOverlay.gameObject.SetActive(false);
                    IsTransitioning = false;
                });
            });
        });
        SoundHolder.Default.PlayFromSoundPack("AttackJackpot", allowPitchShift: false);
    }

    void TheftJackpotAnim()
    {
        foreach (var ball in _pulledBalls)
        {
            ball.Pulse();
        }

        var seq = DOTween.Sequence();

        seq.AppendInterval(1f);

        seq.AppendCallback(() =>
        {
            ScreenManager.Default.GoToTheftScreen(() =>
            {
                IsTransitioning = false;
            });
        });

        SoundHolder.Default.PlayFromSoundPack("TheftJackpot", allowPitchShift: false);
    }

    void HeartJackpotAnim()
    {
        foreach (var ball in _pulledBalls)
        {
            ball.Pulse();
        }
    }
}
