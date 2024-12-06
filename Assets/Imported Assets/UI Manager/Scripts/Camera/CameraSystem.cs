using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

namespace BG.UI.Camera
{
    public enum CameraState { Start, Process, Win, Attack, Theft, Default }

    public class CameraSystem : MonoBehaviour
    {        
        #region Singleton
        private static CameraSystem _default;
        public static CameraSystem Default => _default;
        #endregion

        [SerializeField] private Animator _animator;

        private CameraState _curentState = CameraState.Default;

        public Action<CameraState, CameraState> OnStateChanged;

        public CameraState CurentState
        {
            get => _curentState;
            set
            {
                if (!GameData.Default.useCameraAngles)
                {
                    value = CameraState.Default;
                }

                if (_curentState != value)
                {
                    _curentState = value;

                    if (_animator)
                    {
                        switch (_curentState)
                        {
                            case CameraState.Default:
                                _animator.Play("Default");
                                break;
                            case CameraState.Start:
                                _animator.Play("Start");
                                break;
                            case CameraState.Process:
                                _animator.Play("Game");
                                break;
                            case CameraState.Win:
                                _animator.Play("Finish");
                                break;
                            case CameraState.Attack:
                                _animator.Play("Attack");
                                break;
                            case CameraState.Theft:
                                _animator.Play("Theft");
                                break;
                        }
                    }

                    OnStateChanged?.Invoke(_curentState, value);
                }
            }
        }

        public Transform AttackCamera;

        private void Awake()
        {
            _default = this;
        }

        private void Start()
        {
            LevelManager.Default.OnLevelLoad += () => CurentState = CameraState.Start;
            LevelManager.Default.OnLevelStarted += () => CurentState = CameraState.Process;
            LevelManager.Default.OnLevelComplete += () => CurentState = CameraState.Win;

            CurentState = CameraState.Start;
        }
    }
}