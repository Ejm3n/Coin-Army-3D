using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HPBar : MonoBehaviour
{
    public static int ShownHPNumbers;

    public Unit Target;

    public Image FillerBlue;
    public Image Filler2Blue;
    public Image FillerRed;
    public Image Filler2Red;

    public GameObject BlueMain;
    public GameObject RedMain;

    public CanvasGroup GroupBlue;
    public CanvasGroup GroupRed;

    public TextMeshProUGUI DamageTextBlue;
    public RectTransform DamageTextRTBlue;
    public TextMeshProUGUI DamageTextRed;
    public RectTransform DamageTextRTRed;
    public float DamageTextShownY;
    public float DamageTextHiddenY;
    public float DamageTextHiddenY2;

    private float _filler2time;
    private float _oldhp;
    private float _oldhp2;
    private float _newhp;

    private bool _texttime;
    private bool _texttime3;
    private float _texttime2;

    private float accumulatedDamage;

    private bool _shownHPNumber;

    void OnEnable()
    {
        GroupBlue.alpha = 0f;
        GroupRed.alpha = GroupBlue.alpha;
    }

    void OnDestroy()
    {
        if (_shownHPNumber)
        {
            _shownHPNumber = false;
            ShownHPNumbers--;
        }
    }

    void LateUpdate()
    {
        BlueMain.SetActive(!Target.IsEnemy);
        RedMain.SetActive(Target.IsEnemy);

        transform.rotation = Camera.main.transform.rotation;

        float newhp2 = Target.CurrentHP / Target.MaxHP;

        if (newhp2 > _newhp)
        {
            _newhp = newhp2;
        }
        else
        {
            _newhp = Mathf.MoveTowards(_newhp, newhp2, Time.deltaTime * 5f);
        }

        if (newhp2 > _oldhp2)
        {
            _oldhp = newhp2;
            _oldhp2 = newhp2;
            _filler2time = 0f;
        }
        else if (newhp2 < _oldhp2)
        {
            accumulatedDamage += newhp2 - _oldhp2;
            _oldhp2 = newhp2;
            _filler2time = 0f;
            if (!_shownHPNumber && ShownHPNumbers < 4)
            {
                _shownHPNumber = true;
                ShownHPNumbers++;
                _texttime = true;
            }
        }
        else
        {
            _filler2time += Time.deltaTime;

            if (_filler2time >= 0.2f)
            {
                _oldhp = Mathf.MoveTowards(_oldhp, newhp2, Time.deltaTime);
            }
        }

        FillerBlue.fillAmount = _newhp;
        Filler2Blue.fillAmount = _oldhp;

        FillerRed.fillAmount = _newhp;
        Filler2Red.fillAmount = _oldhp;

        if (accumulatedDamage != 0f)
        {
            DamageTextBlue.text = MoneyService.AmountToStringTrunicate(accumulatedDamage * Target.MaxHP);
            DamageTextRed.text = DamageTextBlue.text;
        }

        float texttimeTarget = _texttime && _texttime2 == 0f || _texttime3 ? 1f : 0f;

        if (_texttime && _texttime2 == 0f)
        {
            _texttime3 = true;
            _texttime = false;
            accumulatedDamage = 0f;
        }

        bool goingUp = texttimeTarget >= _texttime2;

        _texttime2 = Mathf.MoveTowards(_texttime2, texttimeTarget, Time.deltaTime * 1.5f);

        if (_texttime2 == texttimeTarget)
        {
            _texttime3 = false;

            if (_shownHPNumber)
            {
                _shownHPNumber = false;
                ShownHPNumbers--;
            }
        }

        var damageTextAnim = Mathf.SmoothStep(0f, 1f, _texttime2);

        DamageTextBlue.color = new Color(1f, 1f, 1f, damageTextAnim);
        DamageTextRed.color = DamageTextBlue.color;

        DamageTextRTBlue.anchoredPosition = new Vector2(DamageTextRTBlue.anchoredPosition.x, Mathf.Lerp(goingUp ? DamageTextHiddenY : DamageTextHiddenY2, DamageTextShownY, damageTextAnim));
        DamageTextRTBlue.localScale = Vector3.one * Mathf.Lerp(goingUp ? 1f : 0f, 1f, damageTextAnim);

        DamageTextRTRed.anchoredPosition = DamageTextRTBlue.anchoredPosition;
        DamageTextRTRed.localScale = DamageTextRTBlue.localScale;

        GroupBlue.alpha = Mathf.MoveTowards(GroupBlue.alpha, !LevelSettings.Default.IsFightActive && !Target.BeingAttacked || Target.IsDead && _newhp == 0f && _oldhp == 0f ? 0f : 1f, Time.deltaTime * 5f);
        GroupRed.alpha = GroupBlue.alpha;
    }
}
