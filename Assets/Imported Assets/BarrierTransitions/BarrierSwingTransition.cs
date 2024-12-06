using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace ButchersGames.BarrierTransitions
{
    public class BarrierSwingTransition : MonoBehaviour
    {
        public enum RotationAxis { y, x, z }

        [SerializeField] private Transform _transitionObject;
        [Header("Options")]
        [SerializeField] private RotationAxis _axis;
        [SerializeField, Range(-360, 360)] private float _angle;
        [SerializeField] private float _durationTo;
        [SerializeField] private float _durationFrom;
        [SerializeField] private float _pauseStart;
        [SerializeField] private float _pauseEnd;
        [Header("Activator")]
        [SerializeField] private bool _useActivator;
        [SerializeField] private BarrierActivator _activator;
        [Header("Animation Settings")]
        [SerializeField] private Ease _easeTo = Ease.Linear;
        [SerializeField] private Ease _easeFrom = Ease.Linear;

        private bool _moveTo = true;
        private Vector3 _startRotation;

        private Transform Target => 
            _transitionObject != null ? _transitionObject : transform;


        private void Start()
        {
            _startRotation = Target.localRotation.eulerAngles;
            if (_activator && _useActivator) _activator.onTriggered += Launch;
            else Launch();
        }

        private void OnDestroy()
        {
            if (_activator && _useActivator) _activator.onTriggered -= Launch;
        }


        private void Launch()
        {
            StartCoroutine(Swing());
        }

        private IEnumerator Swing()
        {
            Vector3 axis = Vector3.zero;
            if (_axis == RotationAxis.x) axis = Vector3.right;
            if (_axis == RotationAxis.y) axis = Vector3.up;
            if (_axis == RotationAxis.z) axis = Vector3.forward;

            float duration = _moveTo ? _durationTo : _durationFrom;
            float pause = _moveTo ? _pauseEnd : _pauseStart;
            Ease ease = _moveTo ? _easeTo : _easeFrom;

            Quaternion rotatePart = Quaternion.Euler(axis * ((_angle / duration) * Time.fixedDeltaTime)) ;
            if (!_moveTo) rotatePart = Quaternion.Inverse(rotatePart);
            DOTween.To(() => 0f, (v) => 
            {
                Target.transform.localRotation *= rotatePart;
            }, 1f, duration)
                .SetUpdate(UpdateType.Fixed)
                .SetEase(ease);

            _moveTo = !_moveTo;
            yield return new WaitForSeconds(duration + pause);
            StartCoroutine(Swing());
        }
    }
}