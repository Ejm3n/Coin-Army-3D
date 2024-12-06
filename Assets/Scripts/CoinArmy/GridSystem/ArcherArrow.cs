using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArcherArrow : MonoBehaviour
{
    public Archer Parent;
    public GameObject Model;
    public ParticleSystem Particles;
    public ParticleSystem Particles2;

    private Unit _target;

    private bool _noDamage;

    private bool _shot;

    void Update()
    {
        if (!_shot)
        {
            return;
        }

        if (_target == null || _target.IsDead)
        {
            _target = GetClosestOpponent();
        }

        if (_target == null)
        {
            if (Model)
            {
                Model.SetActive(false);
            }
            if (Particles)
            {
                Particles.Stop();
            }
            _shot = false;
            return;
        }

        var direction = (_target.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
        transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, Time.deltaTime * Parent.Description.ArrowSpeed * GameData.Default.GameSpeed);

        if (Vector3.Distance(transform.position, _target.transform.position) < Parent.Description.ArrowDamageRadius)
        {
            _target.TakeDamage(Parent.Damage, Parent.Description.DamageInPercent);
            if (Model)
            {
                Model.SetActive(false);
            }
            if (Particles)
            {
                Particles.Stop();
            }
            if (Particles2)
            {
                Particles2.Play();
            }
            _shot = false;
        }
    }

    public void Shoot(Unit target, bool noDamage)
    {
        if (Model)
        {
            Model.SetActive(true);
        }
        if (Particles)
        {
            Particles.Play();
        }
        transform.position = Parent.ShootingLocation.position;
        var direction = (target.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        _target = target;
        _noDamage = noDamage;
        _shot = true;
    }

    internal Unit GetClosestOpponent()
    {
        Unit closestOpponent = null;

        var opponents = Parent.IsEnemy ? UnitManager.Default.PlayerUnits : UnitManager.Default.EnemyUnits;

        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < opponents.Count; i++)
        {
            if (opponents[i] == this || opponents[i].IsDead)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, opponents[i].transform.position);

            if (i == 0 || dist < closestDistance)
            {
                closestOpponent = opponents[i];
                closestDistance = dist;
            }
        }

        return closestOpponent;
    }
}
