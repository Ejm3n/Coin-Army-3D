using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FloatingHandSwiping : MonoBehaviour
{
    public bool Active;
    public RectTransform Position1;
    public RectTransform Position2;
    public LineRenderer Lines;
    public bool DrawLines;

    private RectTransform _rt;
    private CanvasGroup _group;

    private bool _animatingIn;
    private bool _shouldAnimateOut = true;

    private Vector3 _position;
    private bool _pressedIn;
    private bool _released;

    private bool _ready;

    private Sequence _seq;

    private float _delay;

    private List<Vector3> pos = new List<Vector3>();

    void Start()
    {
        _rt = GetComponent<RectTransform>();
        _group = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        _ready = true;
    }

    void OnDisable()
    {
        if (Lines && Lines.gameObject)
        {
            Lines.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        _rt.position = _position + Vector3.up * 10f;

        if (_pressedIn)
        {
            _pressedIn = false;
            if (!_animatingIn)
            {
                _animatingIn = true;
                _group.DOFade(1f, 0.1f).SetEase(Ease.InOutBounce);
                _rt.DOPivotY(0.91f, 0.1f).SetEase(Ease.InOutBounce);
                _rt.DOScale(0.9f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
                {
                    _animatingIn = false;
                });
            }
        }

        if (_released)
        {
            _released = false;
            _shouldAnimateOut = true;
        }

        if (_shouldAnimateOut && !_animatingIn)
        {
            _shouldAnimateOut = false;
            _group.DOFade(0f, 0.1f).SetEase(Ease.InOutBounce);
            _rt.DOPivotY(0.72f, 0.15f).SetEase(Ease.InOutBounce);
            _rt.DOScale(1f, 0.15f).SetEase(Ease.InOutBounce);
        }

        if (_ready && Active)
        {
            _delay += Time.deltaTime;
            if (_delay >= 0.5f)
            {
                _delay = 0f;
                _ready = false;
                Anim();
            }
        }
        else
        {
            _delay = 0f;
        }

        if (!_ready && !Active)
        {
            _seq.Kill();
            _released = true;
            _ready = true;
        }

        Lines.gameObject.SetActive(DrawLines && (_group.alpha > 0f));

        if (DrawLines)
        {
            if (_group.alpha > 0f)
            {
                var p = GetPointerPosition();

                if (pos.Count == 0 || Vector3.Distance(pos[pos.Count - 1], p) > 0.1f)
                {
                    pos.Add(p);
                }

                Lines.positionCount = pos.Count;
                Lines.SetPositions(pos.ToArray());
            }
            else
            {
                pos.Clear();
            }
        }
        else
        {
            pos.Clear();
        }
    }

    public void Anim()
    {
        if (!Active)
        {
            _ready = true;
            return;
        }

        var start = Position2.position;
        var end = Position1.position;
        _position = end;
        
        _pressedIn = true;

        _seq = DOTween.Sequence();
        _seq.Append(DOTween.To(() => _position, (v) => _position = v, start, 1f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            _released = true;
            DOTween.To(() => _position, (v) => _position = v, end, 1f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                _ready = true;
            });
        }));
    }

    Vector3 GetPointerPosition()
    {
        var plane = new Plane(Vector3.up, Vector3.up * 0.5f);
        var ray = Camera.main.ScreenPointToRay(_rt.position);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.origin + ray.direction * distance;
        }
        return Vector3.zero;
    }
}
