using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FloatingHand : MonoBehaviour
{
    public LineRenderer Lines;

    private RectTransform _rt;

    private bool _animatingIn;
    private bool _shouldAnimateOut = true;

    private List<Vector3> pos = new List<Vector3>();

    void Start()
    {
        _rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }

    void Update()
    {
        _rt.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            if (!_animatingIn)
            {
                _animatingIn = true;
                _rt.DOPivotY(0.91f, 0.1f).SetEase(Ease.InOutBounce);
                _rt.DOScale(0.9f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
                {
                    _animatingIn = false;
                });
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _shouldAnimateOut = true;
        }

        if (_shouldAnimateOut && !_animatingIn)
        {
            _shouldAnimateOut = false;
            _rt.DOPivotY(0.72f, 0.15f).SetEase(Ease.InOutBounce);
            _rt.DOScale(1f, 0.15f).SetEase(Ease.InOutBounce);
        }

        Lines.gameObject.SetActive(Unit.GrabbedUnitsCount > 0);

        if (Unit.GrabbedUnitsCount > 0)
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

    Vector3 GetPointerPosition()
    {
        var plane = new Plane(Vector3.up, Vector3.up * 0.5f);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.origin + ray.direction * distance;
        }
        return Vector3.zero;
    }
}
