using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MoreMountains.NiceVibrations;

public class SettingsPanel : MonoBehaviour
{
    public TextMeshProUGUI VersionText;

    public GameObject SoundOn;
    public GameObject SoundOff;
    public GameObject VibrationOn;
    public GameObject VibrationOff;

    public Transform SoundToggle;
    public Transform VibrationToggle;
    public Transform RestorePurchaseButton;
    public Transform PrivacyPolicyButton;
    public Transform CloseButton;

    public CanvasGroup OwnCanvasGroup;
    public Transform Root;

    private bool _init = false;
    private bool _isOpen = false;

    private bool _isTransitioning = false;

    void Awake()
    {
        VersionText.text = Application.version;
        _init = true;
        bool sound = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        bool vibration = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        AudioListener.volume = sound ? 1f : 0f;
        MMVibrationManager.SetHapticsActive(vibration);
        SoundToggle.GetComponent<Toggle>().isOn = sound;
        VibrationToggle.GetComponent<Toggle>().isOn = vibration;
        _init = false;
        Root.localScale = Vector3.zero;
        UpdateButtons();
    }

    void Update()
    {
        OwnCanvasGroup.alpha = Mathf.MoveTowards(OwnCanvasGroup.alpha, _isOpen ? 1f : 0f, Time.deltaTime * 5f);

        if (!_isOpen && OwnCanvasGroup.alpha == 0f)
        {
            gameObject.SetActive(false);
            _isTransitioning = false;
        }
        else if (_isOpen && OwnCanvasGroup.alpha == 1f)
        {
            _isTransitioning = false;
        }
    }

    void UpdateButtons()
    {
        bool sound = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        bool vibration = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;

        SoundOn.SetActive(sound);
        SoundOff.SetActive(!sound);
        VibrationOn.SetActive(vibration);
        VibrationOff.SetActive(!vibration);
    }

    public void Open()
    {
        if (_isTransitioning)
        {
            return;
        }
        _isOpen = true;
        gameObject.SetActive(true);
        Root.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);
        _isTransitioning = true;
    }

    public void Close()
    {
        if (_isTransitioning)
        {
            return;
        }
        _isOpen = false;
        Root.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InCubic);
        _isTransitioning = true;
    }

    public void AnimCloseButton()
    {
        CloseButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            CloseButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void ToggleSound(bool enable)
    {
        if (_init)
        {
            return;
        }

        PlayerPrefs.SetInt("SoundEnabled", enable ? 1 : 0);
        AudioListener.volume = enable ? 1f : 0f;

        UpdateButtons();

        SoundToggle.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            SoundToggle.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void ToggleVibration(bool enable)
    {
        if (_init)
        {
            return;
        }

        PlayerPrefs.SetInt("VibrationEnabled", enable ? 1 : 0);
        MMVibrationManager.SetHapticsActive(enable);

        UpdateButtons();

        VibrationToggle.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            VibrationToggle.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void RestorePurchase()
    {
        RestorePurchaseButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            RestorePurchaseButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }

    public void PrivacyPolicy()
    {
        PrivacyPolicyButton.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            PrivacyPolicyButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutBounce);
        });

        SoundHolder.Default.PlayFromSoundPack("ButtonSoundUI");
    }
}
