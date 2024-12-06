using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ButchersGames.BarrierTransitions
{
    public class BarrierActivator : MonoBehaviour
    {
        public Action onTriggered;

        private bool _isActive = true;

        private void OnTriggerEnter(Collider other)
        {
            if (_isActive && other.attachedRigidbody && other.attachedRigidbody.tag == "Player")
            {
                onTriggered?.Invoke();
                _isActive = false;
            }
        }
    }
}