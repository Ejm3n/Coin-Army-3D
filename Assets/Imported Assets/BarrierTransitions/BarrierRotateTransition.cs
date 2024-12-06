using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace ButchersGames.BarrierTransitions
{
    public class BarrierRotateTransition : MonoBehaviour
    {
        public enum RotationDirection { Normal, Inverse }
        public enum RotationAxis { y, x, z }

        [SerializeField] private Transform _transitionObject;
        [Header("Options")]
        [SerializeField, Min(0f)] private float _speed;
        [SerializeField] private RotationDirection _direction;
        [SerializeField] private RotationAxis _axis;
        [Header("Activator")]
        [SerializeField] private bool _useActivator;
        [SerializeField] private BarrierActivator _activator;

        private bool _stoped;
        private float _curentSpeed;

        private Transform Target => 
            _transitionObject != null ? _transitionObject : transform;


        private void Start()
        {
            if (_activator && _useActivator) _activator.onTriggered += Launch;
            else Launch();
        }

        private void OnDestroy()
        {
            if (_activator && _useActivator) _activator.onTriggered -= Launch;
        }

        private void FixedUpdate()
        {
            Vector3 rotation = Vector3.zero;
            if (_axis == RotationAxis.x) rotation = Vector3.right;
            if (_axis == RotationAxis.y) rotation = Vector3.up;
            if (_axis == RotationAxis.z) rotation = Vector3.forward;
            rotation *= _direction == RotationDirection.Normal ? 1f : -1f;
            rotation *= _curentSpeed;

            Target.localRotation *= Quaternion.Euler(rotation);
        }


        public void Stop()
        {
            if (!_stoped)
            {
                _stoped = true;
                DOTween.To(() => _curentSpeed, (v) => _curentSpeed = v, 0f, 1f);
            }
        }


        private void Launch()
        {
            _curentSpeed = _speed;
        }
    }
}