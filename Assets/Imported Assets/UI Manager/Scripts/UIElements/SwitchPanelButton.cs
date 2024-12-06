using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BG.UI.Main;
using BG.UI.Camera;

namespace BG.UI.Elements
{
    public class SwitchPanelButton : MonoBehaviour
    {
        enum LevelManagerAction { None, Start, Next }


        [SerializeField] private UIState _onClickState;
        [SerializeField] private LevelManagerAction _levelManagerAction;
        private Button _button;


        private void Awake()
        {
            _button = GetComponentInChildren<Button>();
            _button.onClick.AddListener(HandleOnButtonClicked);
        }

        private void HandleOnButtonClicked()
        {
            Action action = () =>
            {
                switch (_levelManagerAction)
                {
                    case LevelManagerAction.None:
                        break;
                    case LevelManagerAction.Start:
                        LevelManager.Default.StartLevel();
                        break;
                    case LevelManagerAction.Next:
                        LevelManager.Default.NextLevel();
                        break;
                }
                UIManager.Default.CurrentState = _onClickState;
            };

            if (_levelManagerAction == LevelManagerAction.Next)
            {
                Transition.Default.DoTransition(action);
            }
            else
            {
                action.Invoke();
            }
        }
    }
}