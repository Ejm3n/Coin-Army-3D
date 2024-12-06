using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Transition : MonoBehaviour
{
    #region Singleton
    private static Transition _default;
    public static Transition Default => _default;
    #endregion

    public Animator Animator;
    public GameObject InputBlocker;

    public Image Avatar;
    public TextMeshProUGUI Name;

    public RectTransform AvatarRT;
    public Image AttackIcon;
    public Image TheftIcon;

    public CanvasGroup OverlayGroup;

    private bool _isTransitioning;

    void Awake()
    {
        _default = this;
    }

    void Update()
    {
        Avatar.sprite = OpponentService.Default.Description.Avatar;
        Name.text = OpponentService.Default.Description.Username;
    }

    public bool IsInTransition()
    {
        return _isTransitioning;
    }

    public void DoTransition(Action action)
    {
        StartCoroutine(DoTransitionH(action));
    }

    IEnumerator DoTransitionH(Action action)
    {
        _isTransitioning = true;
        SoundHolder.Default.PlayFromSoundPack("Transition", delay: 0.25f);
        InputBlocker.SetActive(true);
        Animator.enabled = true;
        Animator.SetBool("IsClosed", true);
        yield return new WaitForSeconds(1f);
        action?.Invoke();
        Animator.SetBool("IsClosed", false);
        yield return new WaitForSeconds(0.5f);
        InputBlocker.SetActive(false);
        _isTransitioning = false;
    }

    public void DoOpponentIntro(bool isAttack, Action onEnd)
    {
        AttackIcon.gameObject.SetActive(isAttack);
        TheftIcon.gameObject.SetActive(!isAttack);
        OverlayGroup.alpha = 0f;
        AvatarRT.anchorMin = new Vector2(0.5f, 0.5f);
        AvatarRT.anchorMax = new Vector2(0.5f, 0.5f);
        AvatarRT.anchoredPosition = new Vector2(0f, 62f);
        AvatarRT.localScale = new Vector3(1.5f, 1.5f, 1f);
        Name.color = Color.white;
        AttackIcon.color = Color.white;
        TheftIcon.color = Color.white;

        var seq = DOTween.Sequence();

        seq.Append(OverlayGroup.DOFade(1f, 0.5f));
        seq.AppendInterval(0.5f);
        seq.Append(Name.DOFade(0f, 0.5f));
        seq.Join(AttackIcon.DOFade(0f, 0.5f));
        seq.Join(TheftIcon.DOFade(0f, 0.5f));
        seq.Join(AvatarRT.DOAnchorMin(new Vector2(0f, 1f), 0.5f));
        seq.Join(AvatarRT.DOAnchorMax(new Vector2(0f, 1f), 0.5f));
        seq.Join(AvatarRT.DOAnchorPos(new Vector2(88.69995f, -101.4247f), 0.5f));
        seq.Join(AvatarRT.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        seq.AppendCallback(() => OverlayGroup.alpha = 0f);
        seq.AppendCallback(() => onEnd());
    }
}
