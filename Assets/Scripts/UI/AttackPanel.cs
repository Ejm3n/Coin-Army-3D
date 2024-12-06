using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BG.UI.Camera;
using DG.Tweening;
using TMPro;

public class AttackPanel : MonoBehaviour
{
    public GameObject ShieldPrefab;

    public TextMeshProUGUI ResultsText;
    public GameObject ResultsTextYes;
    public TextMeshProUGUI ResultsTextNoText;
    public GameObject ResultsTextNo;
    public GameObject ResultsTextNo2;
    public CanvasGroup ResultsGroup;
    public CanvasGroup MainGroup;
    public CanvasGroup Crosshair;
    public CanvasGroup HelpText;
    public Transform ArrowMarker;
    public ParticleSystem ArrowParticles;

    public RectTransform Glow;

    public Image Avatar;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI UserNumber;

    public RectTransform OkButton;

    public RectTransform HeaderBg;
    public GameObject AvatarParent;

    public GameObject AttackSetup;
    public Animator Crossbow;
    public Transform Arrow;

    private bool _attacked = false;
    private bool _notShielded = false;

    private Vector3 _attackCameraPosition;
    private Quaternion _attackCameraRotation;
    private float _attackCameraFov;

    private GameObject _shield;

    private bool _showResults;

    private int _state;

    private Cinemachine.CinemachineVirtualCamera _cameraControl;

    private float _zoom;
    private float _horizontalAim;
    private float _verticalAim;
    private Vector3 _mousePos;

    private Ray _hitRay;
    private Vector3 _arrowDestination;

    private List<Unit> _affectedUnits;

    private float _pressTime;

    void OnEnable()
    {
        if (UnitManager.Default == null || UnitManager.Default.PlayerGrid == null)
        {
            return;
        }

        _attacked = false;

        UnitManager.Default.PlayerGrid.gameObject.SetActive(false);

        _shield = Instantiate(ShieldPrefab);
        _shield.SetActive(false);

        _attackCameraPosition = CameraSystem.Default.AttackCamera.position;
        _attackCameraRotation = CameraSystem.Default.AttackCamera.rotation;
        _cameraControl = CameraSystem.Default.AttackCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        _attackCameraFov = _cameraControl.m_Lens.FieldOfView;

        _notShielded = OpponentService.Default.ShieldsLeft <= 0 || LevelSettings.TutorialStage <= 7;

        UnitManager.DamageMultiplier = SlotMachine.Multiplier;

        ResultsGroup.alpha = 0f;
        _showResults = false;

        PrepareHeader();
        Transition.Default.DoOpponentIntro(true, DoHeaderAnim);

        AttackSetup.SetActive(true);

        _state = 0;
        _zoom = 0f;
        _verticalAim = 0f;
        _horizontalAim = 0f;

        Crossbow.SetTrigger("Fire");
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

    void Update()
    {
        if (CameraSystem.Default.CurentState != CameraState.Attack)
        {
            CameraSystem.Default.CurentState = CameraState.Attack;
        }

        ResultsGroup.alpha = Mathf.MoveTowards(ResultsGroup.alpha, _showResults ? 1f : 0f, Time.deltaTime * 2f);
        ResultsGroup.interactable = ResultsGroup.alpha == 1f;
        ResultsGroup.blocksRaycasts = ResultsGroup.interactable;

        Glow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(Time.time * 90f, 360f));

        MainGroup.alpha = 1f - ResultsGroup.alpha;

        Avatar.sprite = OpponentService.Default.Description.Avatar;
        Name.text = OpponentService.Default.Description.Username;
        UserNumber.text = string.Format(Language.Text("MoneyAmount"), OpponentService.Default.MoneyLeft.ToString("N0"));

        switch (_state)
        {
            case 0:

                _zoom = Mathf.MoveTowards(_zoom, 0f, Time.deltaTime * 3f);

                if (Input.GetMouseButtonDown(0))
                {
                    _state = 1;
                    _mousePos = Input.mousePosition;

                    _pressTime = 0f;

                    Crossbow.SetTrigger("Load");
                }

                break;
            
            case 1:

                _zoom = Mathf.MoveTowards(_zoom, 1f, Time.deltaTime * 2f);

                if (Input.GetMouseButtonDown(0))
                {
                    _mousePos = Input.mousePosition;
                    _pressTime = 0f;
                }

                if (Input.GetMouseButton(0))
                {
                    var delta = Input.mousePosition - _mousePos;
                    _horizontalAim += delta.x / Screen.height * 50f;
                    _verticalAim -= delta.y / Screen.height * 50f;
                    _horizontalAim = Mathf.Clamp(_horizontalAim, -10f, 10f);
                    _verticalAim = Mathf.Clamp(_verticalAim, -10f, 2f);
                    _mousePos = Input.mousePosition;

                    _pressTime += Time.deltaTime;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (_pressTime < 0.2f)
                    {
                        _state = 0;

                        Crossbow.SetTrigger("Fire");
                    }
                    else
                    {
                        _state = 2;

                        _hitRay = new Ray(CameraSystem.Default.AttackCamera.position, CameraSystem.Default.AttackCamera.forward);

                        var hits = Physics.RaycastAll(_hitRay, 100f);

                        _affectedUnits = new List<Unit>();

                        if (hits.Length > 0)
                        {
                            _arrowDestination = hits[0].point;

                            foreach (var hit in hits)
                            {
                                var unit = hit.collider.GetComponent<Unit>();

                                if (unit && !unit.IsDead && unit.IsEnemy)
                                {
                                    _affectedUnits.Add(unit);
                                }
                            }

                            if (_affectedUnits.Count == 0)
                            {
                                _arrowDestination = Arrow.position - Arrow.forward * 100f;
                            }
                        }
                        else
                        {
                            _arrowDestination = Arrow.position - Arrow.forward * 100f;
                        }

                        if (!_notShielded)
                        {
                            _arrowDestination = Vector3.Lerp(Arrow.position, _arrowDestination, 0.5f);
                        }
                    }
                }

                break;
            
            case 2:

                _zoom = Mathf.MoveTowards(_zoom, 0f, Time.deltaTime * 3f);

                if (_zoom == 0f)
                {
                    _state = 3;

                    Attack();
                }

                break;
            
            case 3:

                Arrow.position = Vector3.MoveTowards(Arrow.position, _arrowDestination, Time.deltaTime * 30f);

                if ((Arrow.position - _arrowDestination).magnitude > 1f)
                {
                    Arrow.forward = (Arrow.position - _arrowDestination).normalized;
                }

                if (Arrow.position == _arrowDestination)
                {
                    _state = 4;

                    ArrowParticles.Stop();

                    Arrow.transform.position = Vector3.up * 100f;

                    if (_notShielded)
                    {
                        foreach (var unit in _affectedUnits)
                        {
                            if (LevelSettings.Default.Description.IsBoss)
                            {
                                unit.TakeDamage(0.2f, true);
                            }
                            else
                            {
                                unit.TakeDamage(1f, true);
                            }
                        }
                    }
                }

                break;
        }

        _cameraControl.m_Lens.FieldOfView = Mathf.SmoothStep(_attackCameraFov, _attackCameraFov * 0.5f, _zoom);

        Crosshair.alpha = Mathf.SmoothStep(0f, 1f, _zoom);
        HelpText.alpha = 1f - Crosshair.alpha;

        Crossbow.transform.rotation = Quaternion.Euler(0f, 180f + _horizontalAim * 1.2f, 0f);

        if (_state < 3)
        {
            Arrow.position = ArrowMarker.position;
            Arrow.rotation = ArrowMarker.rotation;

            CameraSystem.Default.AttackCamera.rotation = _attackCameraRotation * Quaternion.Euler(_verticalAim * Mathf.SmoothStep(0f, 1f, _zoom), _horizontalAim * Mathf.SmoothStep(0f, 1f, _zoom), 0f);
        }
    }

    void OnDisable()
    {
        if (UnitManager.Default == null || UnitManager.Default.PlayerGrid == null)
        {
            return;
        }

        UnitManager.Default.PlayerGrid.gameObject.SetActive(true);

        CameraSystem.Default.CurentState = CameraState.Default;

        Destroy(_shield);

        CameraSystem.Default.AttackCamera.position = _attackCameraPosition;
        CameraSystem.Default.AttackCamera.rotation = _attackCameraRotation;
        _cameraControl.m_Lens.FieldOfView = _attackCameraFov;

        UnitManager.DamageMultiplier = 1;

        if (AttackSetup)
        {
            AttackSetup.SetActive(false);
        }
    }

    public void Attack()
    {
        if (_attacked)
        {
            return;
        }
        _attacked = true;

        Crossbow.SetTrigger("Fire");

        ArrowParticles.Play();

        if (_notShielded && _affectedUnits.Count > 0)
        {
            CameraSystem.Default.AttackCamera.DOMove(_arrowDestination - CameraSystem.Default.AttackCamera.forward * 5f, 0.5f);
        }

        StartCoroutine(AttackH());
    }

    IEnumerator AttackH()
    {
        if (!_notShielded)
        {
            _shield.SetActive(true);

            _shield.transform.position = _arrowDestination;
            
            OpponentService.Default.ShieldsLeft = Mathf.Max(OpponentService.Default.ShieldsLeft - SlotMachine.Multiplier, 0);

            SoundHolder.Default.PlayFromSoundPack("ShieldBreak", allowPitchShift: false);
        }
        else
        {
            SoundHolder.Default.PlayFromSoundPack("ShootSound");
        }

        yield return new WaitForSeconds(1f);

        OnAttackFinished();
    }

    public void OnAttackFinished()
    {
        StartCoroutine(Finish());
    }

    IEnumerator Finish()
    {
        yield return new WaitForSeconds(1f);

        ulong reward;

        if (_notShielded)
        {
            reward = (GameData.Default.AttackReward + GameData.Default.AttackRewardRaise * (ulong)LevelManager.Default.DifficultyCounter) * (ulong)SlotMachine.Multiplier;
        }
        else
        {
            reward = (ulong)(MoneyService.PrettyRounding((GameData.Default.AttackReward + GameData.Default.AttackRewardRaise * (ulong)LevelManager.Default.DifficultyCounter) * GameData.Default.FailedAttackPercent) * (ulong)SlotMachine.Multiplier);
        }

        MoneyService.Default.AddMoney(reward);
        ShowResults(reward);

        float distance = LevelSettings.Default.Description.IsBoss ? 5f : 2f;

        UnitManager.Default.SaveHP();
    }

    public void ShowResults(ulong result)
    {
        _showResults = true;
        ResultsTextYes.SetActive(_notShielded && _affectedUnits.Count > 0);
        ResultsTextNo.SetActive(!_notShielded);
        ResultsTextNo2.SetActive(_notShielded && _affectedUnits.Count == 0);
        ResultsTextNoText.text = string.Format(Language.Text("BlockedAttack"), OpponentService.Default.Description.Username.ToUpper());
        var c = ColorUtility.ToHtmlStringRGB(GameData.Default.TextAccentColor);
        ResultsText.text = string.Format(Language.Text("YouWonAmount"), c, result.ToString("N0"));

        SoundHolder.Default.PlayFromSoundPack("TheftJackpot", allowPitchShift: false);
    }

    public void Hide()
    {
        if (!_showResults)
        {
            return;
        }

        ScreenManager.Default.GoToSlotScreen(() =>
        {
            OnDisable();
        });

        OkButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            OkButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        if (LevelSettings.TutorialStage <= 7)
        {
            LevelSettings.TutorialStage = 8;
        }

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
