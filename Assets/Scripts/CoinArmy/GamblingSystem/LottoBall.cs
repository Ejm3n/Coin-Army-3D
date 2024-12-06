using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LottoBall : MonoBehaviour
{
    public SlotMachine ParentLotto;
    public GameData.BallType BallType;
    public float BallRadius;

    public bool IsReady => _ballState == 3;

    private float _angleAround;
    private float _horizontalPosition;

    private Vector3 _ballRotation;
    private Vector3 _ballRotationDirection;

    private Vector3 _ballAngularVelocity;

    private int _ballState;
    private bool _allowToStop;
    private Transform _exit;

    private Vector3 _oldPos;
    private float _pushTime;

    private bool _resting = false;
    private bool _slowing = false;

    private float _changeSpeed = 5f;

    private Vector3 baseScale;

    private bool _releasePosition;

    // states:
    // 0 - ball is spinning inside
    // 1 - ball is chosen to be pulled out, waiting for good angle
    // 2 - ball is going out
    // 3 - ball is out, still spinning
    // 4 - ball stops spinning facing camera
    // 5 - ball flies back

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Start()
    {
        _ballRotationDirection = new Vector3
        (
            Random.Range(-1f, 1f),
            Random.Range(-0.1f, 0.1f),
            0f
        ).normalized;

        _horizontalPosition = Random.Range(ParentLotto.InternalPivots[0].localPosition.x, ParentLotto.InternalPivots[1].localPosition.x);
        _angleAround = Random.Range(55, 125f);

        var center = new Vector3(0f, ParentLotto.InternalPivots[0].localPosition.y, ParentLotto.InternalPivots[0].localPosition.z);
        var targetPosition = center;
        targetPosition += Vector3.right * _horizontalPosition;
        targetPosition += (Quaternion.Euler(_angleAround, 0f, 0f) * Vector3.forward) * (ParentLotto.InnerRadius - BallRadius);

        transform.localPosition = targetPosition;

        _resting = true;
    }

    void Update()
    {
        switch (_ballState)
        {
            case 0:
            case 1:
                _changeSpeed = Mathf.MoveTowards(_changeSpeed, _slowing ? 5f : 7f, Time.deltaTime * 10f);

                _ballAngularVelocity = Vector3.MoveTowards(_ballAngularVelocity, !_resting ? _ballRotationDirection : Vector3.zero, Time.deltaTime * _changeSpeed);

                _angleAround = Mathf.Repeat(_angleAround, 360f);
                _horizontalPosition = Mathf.Clamp(_horizontalPosition, ParentLotto.InternalPivots[0].localPosition.x + BallRadius, ParentLotto.InternalPivots[1].localPosition.x - BallRadius);

                var center = new Vector3(0f, ParentLotto.InternalPivots[0].localPosition.y, ParentLotto.InternalPivots[0].localPosition.z);
                var targetPosition = center;
                targetPosition += Vector3.right * _horizontalPosition;
                targetPosition += (Quaternion.Euler(_angleAround, 0f, 0f) * Vector3.forward) * (ParentLotto.InnerRadius - BallRadius);

                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, Time.deltaTime * _changeSpeed);

                if (transform.localPosition == targetPosition && (ParentLotto.Rotating || !_resting))
                {
                    if (!ParentLotto.Rotating)
                    {
                        _slowing = true;
                    }
                    else
                    {
                        _slowing = false;
                    }
                    if (!ParentLotto.Rotating && _changeSpeed == 5f)
                    {
                        _horizontalPosition = Random.Range(_horizontalPosition - 1f, _horizontalPosition + 1f);
                        _angleAround = Random.Range(55, 125f);
                        _resting = true;
                    }
                    else
                    {
                        _horizontalPosition = Random.Range(ParentLotto.InternalPivots[0].localPosition.x, ParentLotto.InternalPivots[1].localPosition.x);
                        _angleAround = Random.Range(0f, 360f);
                        _resting = false;

                        SoundHolder.Default.PlayFromSoundPack("BallShuffle");
                    }
                }

                _ballRotation += _ballAngularVelocity * 1000f * Time.deltaTime;

                _ballRotation = new Vector3
                (
                    Mathf.Repeat(_ballRotation.x, 360f),
                    Mathf.Repeat(_ballRotation.y, 360f),
                    Mathf.Repeat(_ballRotation.z, 360f)
                );

                transform.localEulerAngles = _ballRotation;

                if (_ballState == 1)
                {
                    _ballState = 2;
                    transform.localPosition = _exit.localPosition;
                    _ballRotation = Vector3.zero;
                    _ballRotationDirection = Vector3.right;
                    _ballAngularVelocity = Vector3.right;
                }
                break;
            case 2:

                _resting = false;

                _ballRotation += _ballAngularVelocity * 1000f * Time.deltaTime;

                var lookAtCameraRotation = Quaternion.LookRotation((Camera.main.transform.position - transform.position).normalized).eulerAngles;

                _ballRotation = new Vector3
                (
                    Mathf.Repeat(_ballRotation.x, 360f),
                    Mathf.LerpAngle(_ballRotation.y, lookAtCameraRotation.y, Time.deltaTime * 10f),
                    Mathf.LerpAngle(_ballRotation.z, 0f, Time.deltaTime * 10f)
                );

                transform.localEulerAngles = _ballRotation;

                var target = _exit.localPosition + Vector3.up * 0.5f;

                transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, Time.deltaTime * 10f);

                if (Vector3.Distance(transform.localPosition, target) < 0.1f)
                {
                    _ballState = 3;
                }
                break;
            case 3:
                _ballRotation += _ballAngularVelocity * 1000f * Time.deltaTime;

                lookAtCameraRotation = Quaternion.LookRotation((Camera.main.transform.position - transform.position).normalized).eulerAngles;

                _ballRotation = new Vector3
                (
                    Mathf.Repeat(_ballRotation.x, 360f),
                    Mathf.LerpAngle(_ballRotation.y, lookAtCameraRotation.y, Time.deltaTime * 10f),
                    Mathf.LerpAngle(_ballRotation.z, 0f, Time.deltaTime * 10f)
                );

                target = Vector3.Scale(_exit.localPosition, new Vector3(0.45f, 1f, 1f)) + Vector3.up * 0.4f + Vector3.back * 4f;

                transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * 10f);

                transform.localEulerAngles = _ballRotation;

                if (_allowToStop)
                {
                    _ballState = 4;
                    _releasePosition = false;
                    _ballAngularVelocity = Vector3.zero;
                    SoundHolder.Default.PlayFromSoundPack("BallShow");
                }
                break;
            case 4:

                if (SlotMachine.ForceSpinBalls)
                {
                    _ballAngularVelocity = Vector3.MoveTowards(_ballAngularVelocity, Vector3.right, Time.deltaTime * 10f);

                    _ballRotation += _ballAngularVelocity * 2000f * Time.deltaTime;

                    lookAtCameraRotation = Quaternion.LookRotation((Camera.main.transform.position - transform.position).normalized).eulerAngles;

                    _ballRotation = new Vector3
                    (
                        Mathf.Repeat(_ballRotation.x, 360f),
                        Mathf.LerpAngle(_ballRotation.y, lookAtCameraRotation.y, Time.deltaTime * 10f),
                        Mathf.LerpAngle(_ballRotation.z, 0f, Time.deltaTime * 10f)
                    );
                }
                else
                {
                    lookAtCameraRotation = Quaternion.LookRotation((Camera.main.transform.position - transform.position).normalized).eulerAngles;

                    _ballRotation = new Vector3
                    (
                        _ballRotation.x,
                        Mathf.LerpAngle(_ballRotation.y, lookAtCameraRotation.y, Time.deltaTime * 10f),
                        Mathf.LerpAngle(_ballRotation.z, 0f, Time.deltaTime * 10f)
                    );

                    if (_ballRotation.x < Mathf.Repeat(lookAtCameraRotation.x - 90f, 360f))
                    {
                        float ang = Mathf.DeltaAngle(_ballRotation.x, Mathf.Repeat(lookAtCameraRotation.x - 90f, 360f));
                        _ballRotation.x += Time.deltaTime * 1000f * Mathf.Lerp(1f, 0f, Mathf.InverseLerp(180f, 0f, ang < 0f ? 180f : ang));
                    }
                    else
                    {
                        _ballRotation.x += Time.deltaTime * 1000f;
                    }

                    _ballRotation = new Vector3
                    (
                        Mathf.Repeat(_ballRotation.x, 360f),
                        Mathf.Repeat(_ballRotation.y, 360f),
                        Mathf.Repeat(_ballRotation.z, 360f)
                    );
                }

                target = Vector3.Scale(_exit.localPosition, new Vector3(0.45f, 1f, 1f)) + Vector3.up * 0.4f + Vector3.back * 4f;

                if (!_releasePosition && Vector3.Distance(transform.localPosition, target) > 0.01f)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * 10f);
                }
                else
                {
                    _releasePosition = true;
                }

                transform.localEulerAngles = _ballRotation;
                break;
            case 5:
                _ballRotation += _ballAngularVelocity * 1000f * Time.deltaTime;

                _ballRotation = new Vector3
                (
                    Mathf.Repeat(_ballRotation.x, 360f),
                    Mathf.Repeat(_ballRotation.y, 360f),
                    Mathf.Repeat(_ballRotation.z, 360f)
                );

                transform.localEulerAngles = _ballRotation;

                target = _exit.localPosition + Vector3.up * 10f + Vector3.forward * 10f;

                _pushTime += Time.deltaTime * 2f;

                transform.localPosition = Vector3.Lerp(_oldPos, target, Mathf.SmoothStep(0f, 1f, _pushTime));

                if (Vector3.Distance(transform.localPosition, target) < 0.001f)
                {
                    _ballState = 0;
                    Start();
                }
                break;
        }
    }

    public void PushBall(bool instant)
    {
        if (instant)
        {
            _ballState = 0;
            _allowToStop = false;
            Start();
        }
        else
        {
            _ballState = 5;
            _allowToStop = false;
            _oldPos = transform.localPosition;
            _pushTime = 0f;
        }
    }

    public void PullBall(Transform exit)
    {
        _ballState = 1;
        _allowToStop = false;
        _exit = exit;
    }

    public void StopBall()
    {
        _allowToStop = true;
    }

    public void Pulse()
    {
        transform.DOScale(baseScale * 1.1f, 0.25f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            transform.DOScale(baseScale, 0.25f).SetEase(Ease.InOutCubic);
        });
    }

    public void ResetScale()
    {
        transform.localScale = baseScale;
    }
}
