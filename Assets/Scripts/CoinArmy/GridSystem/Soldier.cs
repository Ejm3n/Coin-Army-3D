using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : Unit
{
    private Vector3 _walkTarget;

    private float _damageTimer;

    private bool _currentlyAttacking = false;

    private Unit _closestOpponent;

    private bool _startedWalking;
    private bool _blockWalking;

    void Awake()
    {
        _damageTimer = Description.DamageDelay / GameData.Default.GameSpeed;
    }

    void Update()
    {
        UpdateAnimations();

        IsWalking = false;

        if (!LevelSettings.Default.IsFightActive || IsAnimated || IsGrabbed)
        {
            return;
        }

        if (IsDead || IsTempUnit || _currentlyAttacking)
        {
            return;
        }

        var _closestOpponent2 = GetClosestOpponent();

        if (_closestOpponent == null || _closestOpponent.IsDead || _closestOpponent2 != null && Vector3.Distance(transform.position, _closestOpponent2.transform.position) < Vector3.Distance(transform.position, _closestOpponent.transform.position))
        {
            _closestOpponent = _closestOpponent2;
            _blockWalking = false;
            _startedWalking = false;
        }

        if (_closestOpponent == null)
        {
            return;
        }

        _walkTarget = _closestOpponent.transform.position;

        var direction = (_walkTarget - transform.position).normalized;
        var targetRotation = Quaternion.LookRotation(direction);

        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || Animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * Description.RotationSpeed * GameData.Default.GameSpeed);
        }

        _damageTimer += Time.deltaTime;

        var dirToTarget = Vector3.Dot(transform.forward, (_walkTarget - direction * Description.StopDistance - transform.position).normalized);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 5f || _startedWalking && dirToTarget > 0f)
        {
            if (!_blockWalking && dirToTarget > 0f && Animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                transform.position = Vector3.MoveTowards(transform.position, _walkTarget - direction * Description.StopDistance, Time.deltaTime * Description.MoveSpeed * GameData.Default.GameSpeed);
            }

            if (_blockWalking || dirToTarget <= 0f || Vector3.Distance(transform.position, _walkTarget - direction * Description.StopDistance) <= 0.1f)
            {
                IsWalking = false;
                _blockWalking = true;

                if (_damageTimer >= Description.DamageDelay / GameData.Default.GameSpeed && !_closestOpponent.IsWalking)
                {
                    _damageTimer = 0f;
                    _startedWalking = false;

                    if (Vector3.Distance(transform.position, _walkTarget - direction * Description.StopDistance) > 0.5f)
                    {
                        _blockWalking = false;
                    }

                    Attack(_closestOpponent);
                }
            }
            else
            {
                IsWalking = true;
                _startedWalking = true;
            }
        }
        else
        {
            _blockWalking = false;
        }
    }

    public override void Attack(Unit opponent, bool noDamage = false)
    {
        _currentlyAttacking = true;

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

        if (!noDamage)
        {
            if (Description.DamageEveryone)
            {
                var units = IsEnemy ? UnitManager.Default.PlayerUnits : UnitManager.Default.EnemyUnits;

                foreach (var unit in units)
                {
                    unit.TakeDamage(Damage, Description.DamageInPercent);
                }
            }
            else
            {
                opponent.TakeDamage(Damage, Description.DamageInPercent);
            }

            DoAttackDamageSound();
        }

        _currentlyAttacking = false;

        OnAttack?.Invoke();
    }
}
