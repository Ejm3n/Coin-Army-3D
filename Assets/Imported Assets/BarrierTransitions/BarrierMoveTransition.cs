using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace ButchersGames.BarrierTransitions
{
    public class BarrierMoveTransition : MonoBehaviour
    {
        [SerializeField] private Transform _transitionObject;
        [Header("Options")]
        [SerializeField] private bool _startOnLeftSide;
        [SerializeField] private float _transitDuration;
        [SerializeField] private float _pauseDuration;
        [SerializeField] private Transform _leftPoint;
        [SerializeField] private Transform _rightPoint;
        [Header("Activator")]
        [SerializeField] private bool _useActivator;
        [SerializeField] private BarrierActivator _activator;
        [Header("Animation Settings")]
        [SerializeField] private Ease _easeToLeft = Ease.OutBack;
        [SerializeField] private Ease _easeToRight = Ease.OutBack;

        private Vector3 _leftPosition;
        private Vector3 _rightPosition;

        private Transform Target =>
            _transitionObject != null ? _transitionObject : transform;



        private void Start()
        {
            _leftPosition = _leftPoint.position;
            _rightPosition = _rightPoint.position;
            if (_activator && _useActivator) _activator.onTriggered += Launch;
            else Launch();
        }

        private void OnDestroy()
        {
            if (_activator && _useActivator) _activator.onTriggered -= Launch;
        }


        public void Stop() 
        {
            StopAllCoroutines();
        }


        private void Launch()
        {
            Transition(_startOnLeftSide);
        }

        private void Transition(bool isLeftSide)
        {
            Vector3 target = isLeftSide ? _leftPosition : _rightPosition;
            Target.DOMove(target, _transitDuration)
                .SetEase(isLeftSide ? _easeToLeft : _easeToRight)
                .OnComplete(() => StartCoroutine(TransitionPause(!isLeftSide)));
        }

        private IEnumerator TransitionPause(bool isLeftSide)
        {
            yield return new WaitForSeconds(_pauseDuration);
            Transition(isLeftSide);
        }
    }
}