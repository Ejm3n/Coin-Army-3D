using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using BG.UI.Main;
using MoreMountains.NiceVibrations;
using UnityEngine.UI;

public class BigRedButton : MonoBehaviour, IMouseEventProxyTarget
{
    public SlotMachine Lotto;

    public Transform PressedPivot;
    public Transform DepressedPivot;
    public Transform Cap;

    public MeshRenderer CapRenderer;

    public float GlowBrightness;

    public Image Fill;

    private bool _isPressed;
    private bool _isPressed2;
    private float _pressAnim;

    private float _autospinTimeout;
    private bool _autospin;

    void Start()
    {
        UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().RequestMouseProxy(this);
    }

    void OnEnable()
    {
        _isPressed = false;
        _isPressed2 = false;
        _pressAnim = 0f;
    }

    void Update()
    {
        if (_isPressed || _autospin)
        {
            _isPressed2 = true;
        }
        else
        {
            if (_pressAnim == 1f)
            {
                _isPressed2 = false;
                
            }
        }

        _pressAnim = Mathf.MoveTowards(_pressAnim, _isPressed2 ? 1f : 0f, Time.deltaTime * 5f);

        Cap.position = Vector3.Lerp(DepressedPivot.position, PressedPivot.position, Mathf.SmoothStep(0f, 1f, _pressAnim));

        CapRenderer.material.EnableKeyword("_EMISSION");
        CapRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        CapRenderer.material.SetColor("_EmissionColor", (_autospin ? Color.yellow : Color.white) * (_pressAnim * GlowBrightness));

        if (_isPressed)
        {
            _autospinTimeout += Time.deltaTime;
        }
        else
        {
            _autospinTimeout = 0f;
        }

        bool wasautospin = _autospin;

        if (_autospinTimeout >= 1f)
        {
            _autospin = true;
        }

        Fill.fillAmount = _autospin ? 0f : (_autospinTimeout - 0.25f) * 1.3333f;

        if (_autospin)
        {
            var lottoPanel = UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>();

            if (
                SpinsService.Default.GetSpins() < SlotMachine.NextMultiplier ||
                lottoPanel.TutorialDarkenScreen.alpha > 0f ||
                lottoPanel.TutorialDarkenScreen2.alpha > 0f ||
                lottoPanel.TutorialDarkenScreen3.alpha > 0f ||
                lottoPanel.MissionPanelOpen ||
                lottoPanel.MissionResultsOpen
                )
            {
                _autospin = false;
            }
            else
            {
                if (
                    !SlotMachine.ShowsJackpotAnimationType2 &&
                    SlotMachine.IsIdling
                    )
                {
                    Lotto.Activate();

                    LottoPanel.BigRedButtonWasPressed = true;
                }
            }
        }

        if (_autospin && !wasautospin)
        {
            MMVibrationManager.Haptic(HapticTypes.MediumImpact, false, true, this);
        }
    }

    public bool Exists()
    {
        return this != null && gameObject != null;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector2 GetSize()
    {
        return new Vector2(200f, 200f);
    }

    public void OnPointerDown()
    {
        _isPressed = true;
        _autospin = false;

        SoundHolder.Default.PlayFromSoundPack("ButtonSound");
    }

    public void OnPointerDrag()
    {

    }
    
    public void OnPointerUp()
    {
        if (_isPressed)
        {
            _isPressed = false;

            if (!_autospin)
            {
                if (SpinsService.Default.GetSpins() == 0)
                {
                    UIManager.Default.GetPanel(UIState.Lotto).GetComponent<LottoPanel>().DoNoSpinsPromotion();
                }

                Lotto.Activate();

                LottoPanel.BigRedButtonWasPressed = true;
            }
        }
    }

    public void DisableAutospin()
    {
        _autospin = false;
    }
}
