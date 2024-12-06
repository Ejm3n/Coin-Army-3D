using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TutorialHand : MonoBehaviour
{
    [SerializeField] private float _clickMoveTime;
    [SerializeField] private Image _handImage;
    [Header("Trail")]
    [SerializeField] private bool _useTrail;
    [SerializeField] private float _trailSpawnDistance;
    [SerializeField] private float _trailLifeTime;
    [SerializeField] private Image _trailImagePrefab;
    [SerializeField] private Transform _trailHolder;

    private float _distance;
    private Vector2 _targetPosition;
    private Vector2 _clickPosition;
    private Sequence _clickSeq;



    private void Start()
    {
        _targetPosition = _handImage.transform.position;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _targetPosition = Input.mousePosition;
        }

        if (_useTrail)
        {
            _distance += ((Vector2)_handImage.transform.position - _targetPosition).magnitude;
            if (_distance >= _trailSpawnDistance)
            {
                _distance = 0f;
                Image newTrailElement = Instantiate(_trailImagePrefab);
                newTrailElement.transform.position = _handImage.transform.position;
                newTrailElement.transform.SetParent(_trailHolder);
                newTrailElement.DOColor(GetTargetColor(newTrailElement.color, 0f), _trailLifeTime);
                Destroy(newTrailElement.gameObject, _trailLifeTime);
            }
        }

        if (Input.GetMouseButtonDown(0))
            _clickPosition = Input.mousePosition;
        if (Input.GetMouseButtonUp(0) && _clickPosition == (Vector2)Input.mousePosition)
        {
            Vector2 animPosition = Input.mousePosition;
            if (_clickSeq == null)
            {
                _clickSeq = DOTween.Sequence();
                _clickSeq.Append(_handImage.transform.DOMove(animPosition, _clickMoveTime).SetEase(Ease.OutCubic));
                _clickSeq.Append(_handImage.transform.DOScale(Vector3.one * 0.5f, 0.25f).SetEase(Ease.InOutCubic));
                _clickSeq.Append(_handImage.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
                _clickSeq.OnComplete(() =>
                {
                    _clickSeq = null;
                    _targetPosition = animPosition;
                });
                return;
            }
        }

        if (_clickSeq == null && _clickPosition != (Vector2)Input.mousePosition)
        {
            _handImage.transform.position = Vector2.Lerp(_handImage.transform.position, _targetPosition, Time.deltaTime * 15f);
            Vector3 targetScale;
            float distanceToTarget = ((Vector2)_handImage.transform.position - _targetPosition).magnitude;
            if (Input.GetMouseButton(0) && distanceToTarget <= 10f)
                targetScale = Vector3.one * 0.6f;
            else
                targetScale = Vector3.one;
            _handImage.transform.localScale = Vector3.Lerp(_handImage.transform.localScale, targetScale, Time.deltaTime * 5f);
        }
    }


    private Color GetTargetColor(Color color, float a) 
    {
        Color targetColor = color;
        targetColor.a = a;
        return targetColor;
    }
}
