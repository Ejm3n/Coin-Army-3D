using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BG.UI.Main
{
    public enum UIState { Start, Win, Fail, Lotto, Attack, Theft, Store, Undefined }

    public class UIManager : MonoBehaviour
    {
        #region Singleton
        private static UIManager _default;
        public static UIManager Default => _default;
        #endregion

        [SerializeField] private Panel _startPanel;
        [SerializeField] private Panel _winPanel;
        [SerializeField] private Panel _failPanel;
        [SerializeField] private Panel _lottoPanel;
        [SerializeField] private Panel _attackPanel;
        [SerializeField] private Panel _theftPanel;
        [SerializeField] private Panel _storePanel;

        private Dictionary<UIState, Panel> _stateToPanel;
        private UIState _curentState = UIState.Undefined;

        public Action<UIState, UIState> OnStateChanged;
        public UIState CurrentState
        {
            get => _curentState;
            set
            {
                if (_curentState != value)
                {
                    if (_stateToPanel.ContainsKey(value))
                    {
                        _stateToPanel[value].ShowPanel();
                    }
                    if (_stateToPanel.ContainsKey(_curentState))
                    {
                        _stateToPanel[_curentState].HidePanel();
                    }
                    OnStateChanged?.Invoke(_curentState, value);
                    _curentState = value;
                }
            }
        }

        public FloatingHand Hand;

        private void Awake()
        {
            _default = this;

            _stateToPanel = new Dictionary<UIState, Panel>();
            _stateToPanel.Add(UIState.Start, _startPanel);
            _stateToPanel.Add(UIState.Win, _winPanel);
            _stateToPanel.Add(UIState.Fail, _failPanel);
            _stateToPanel.Add(UIState.Lotto, _lottoPanel);
            _stateToPanel.Add(UIState.Attack, _attackPanel);
            _stateToPanel.Add(UIState.Theft, _theftPanel);
            _stateToPanel.Add(UIState.Store, _storePanel);

            Hand.gameObject.SetActive(GameData.Default.showPromoCursor);
        }
        
        public Panel GetPanel(UIState state)
        {
            return _stateToPanel[state];
        }
    }
}