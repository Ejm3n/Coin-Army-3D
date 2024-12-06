using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Archer : Unit
{
    public ArcherArrow ArcherArrow;
    public Transform ShootingLocation;
    public ParticleSystem ShootingParticle;

    private float _damageTimer;

    private bool _currentlyShooting;

    private Unit _closestOpponent;

    void Awake()
    {
        _damageTimer = Description.DamageDelay / GameData.Default.GameSpeed;
        ArcherArrow.transform.SetParent(transform.parent);
    }

    void OnDestroy()
    {
        Destroy(ArcherArrow.gameObject);
    }

    void Update()
    {
        UpdateAnimations();

        IsWalking = false;

        if (!LevelSettings.Default.IsFightActive || IsAnimated || IsGrabbed)
        {
            return;
        }

        if (IsDead || IsTempUnit || _currentlyShooting)
        {
            return;
        }

        var _closestOpponent2 = GetClosestOpponent();

        if (_closestOpponent == null || _closestOpponent.IsDead || _closestOpponent2 != null && Vector3.Distance(transform.position, _closestOpponent2.transform.position) < Vector3.Distance(transform.position, _closestOpponent.transform.position))
        {
            _closestOpponent = _closestOpponent2;
        }

        if (_closestOpponent == null)
        {
            return;
        }

        var direction = (_closestOpponent.transform.position - transform.position).normalized;
        var targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * Description.RotationSpeed * GameData.Default.GameSpeed);

        _damageTimer += Time.deltaTime;

        if (Quaternion.Angle(transform.rotation, targetRotation) < 90f)
        {
            if (_damageTimer >= Description.DamageDelay / GameData.Default.GameSpeed)
            {
                _damageTimer = 0f;

                Attack(_closestOpponent);
            }
        }
    }

    public override void Attack(Unit opponent, bool noDamage = false)
    {
        _currentlyShooting = true;

        if (Animator)
        {
            Animator.SetTrigger("Attack");
        }

        StartCoroutine(AttackDelay(opponent, noDamage));

        DoAttackSound();
    }

    IEnumerator AttackDelay(Unit opponent, bool noDamage)
    {
        yield return new WaitForSeconds(Description.DamageApplyDelay / GameData.Default.GameSpeed);

        if (IsDead)
        {
            yield break;
        }

        ArcherArrow.Shoot(opponent, noDamage);

        if (ShootingParticle)
        {
            ShootingParticle.Play();
        }

        _currentlyShooting = false;

        OnAttack?.Invoke();

        DoAttackDamageSound();
    }
}
