using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.EventSystems;
using MoreMountains.NiceVibrations;
using BG.UI.Main;

public class Unit : MonoBehaviour, IMouseEventProxyTarget
{
    public static int GrabbedUnitsCount;

    private static float LastVibration;
    private static float LastTap;

    public UnitSetting Description;
    public Animator Animator;
    public Transform AnimatorOffset;

    public ParticleSystem UpgradeParticles;

    public Renderer[] Renderers;

    [NonSerialized]
    public Grid ParentGrid;
    [NonSerialized]
    public int GridCellIndex;
    [NonSerialized]
    public bool IsEnemy;

    public bool IsGrabbed
    {
        get
        {
            return _grabbed;
        }
        set
        {
            if (_grabbed != value)
            {
                _grabbed = value;
                if (_grabbed)
                {
                    GrabbedUnitsCount++;
                }
                else
                {
                    GrabbedUnitsCount = Mathf.Max(GrabbedUnitsCount - 1, 0);
                }
            }
        }
    }
    [NonSerialized]
    public bool IsAnimated;
    [NonSerialized]
    public bool IsDead;
    [NonSerialized]
    public bool IsWalking;
    [NonSerialized]
    public bool IsTempUnit;
    [NonSerialized]
    public bool BeingAttacked;

    public float MaxHP => LevelManager.Default.DifficultyCounter == 1 && !IsEnemy ? Description.HP * 2f : IsEnemy ? Description.HP * UnitManager.EnemyToughnessMultiplier : Description.HP;
    [NonSerialized]
    public float CurrentHP;

    public float Damage => IsEnemy ? Description.Damage * UnitManager.EnemyToughnessMultiplier : Description.Damage;

    public Action OnAttack;

    [NonSerialized]
    public Vector3 Offset;

    public string AttackSound;
    public string AttackDamageSound;

    public ParticleSystem LightningStrike;

    private float _glow;
    private float _grabAnim;

    private float _deathTimer;

    private Cell highlightedCell;

    private bool _reallyGrabbed = false;

    private bool _grabbed = false;

    private List<Unit> allAffectedUnits = new List<Unit>();

    void Start()
    {
        if (CurrentHP == 0f && !IsDead)
        {
            CurrentHP = MaxHP;
        }

        UIManager.Default.GetPanel(UIState.Start).GetComponent<StartPanel>().RequestMouseProxy(this);
    }

    void OnEnable()
    {
        if (IsDead)
        {
            _deathTimer = 100f;
        }
    }

    void LateUpdate()
    {
        if (!_reallyGrabbed)
        {
            if (IsGrabbed)
            {
                if (Input.GetMouseButton(0))
                {
                    OnPointerDrag();
                }
                if (Input.GetMouseButtonUp(0))
                {
                    OnPointerUp();
                }
            }
        }
        
        foreach (var renderer in Renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                material.SetColor("_EmissionColor", Color.white * (_glow * 0.5f));
            }
        }

        _glow = Mathf.MoveTowards(_glow, 0f, Time.deltaTime * 5f);

        if (IsDead)
        {
            _deathTimer += Time.deltaTime;
        }
        else
        {
            _deathTimer = 0f;
        }

        _grabAnim = Mathf.MoveTowards(_grabAnim, IsGrabbed && !GameData.Default.doPromoMerging ? 0.2f : 0f, Time.deltaTime * 1.5f);

        AnimatorOffset.localPosition = new Vector3(0f, Mathf.Lerp(Mathf.Lerp(0f, -1f * 2f, (_deathTimer - 6f) * 0.5f), 1f, _grabAnim), 0f);

        Animator.speed = GameData.Default.GameSpeed;

        Animator.SetBool("HasWon", LevelSettings.Default.IsPostFight && (!IsEnemy == LevelSettings.Default.IsWon));
        Animator.SetBool("BeingGrabbed", IsGrabbed && !GameData.Default.doPromoMerging);

        if (CurrentHP > MaxHP)
        {
            CurrentHP = MaxHP;
        }
    }

    void OnDestroy()
    {
        IsGrabbed = false;
    }

    internal void UpdateAnimations()
    {
        if (!Animator) return;
        Animator.SetBool("Walking", IsWalking);
        Animator.SetBool("IsDead", IsDead);
    }

    Vector3 GetPointerPosition()
    {
        var plane = new Plane(Vector3.up, transform.position);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.origin + ray.direction * distance;
        }
        return transform.position;
    }

    public bool Exists()
    {
        return this != null && gameObject != null;
    }

    public Vector3 GetPosition()
    {
        return transform.position + Vector3.up * 0.5f;
    }

    public Vector2 GetSize()
    {
        return new Vector2(150f, 150f);
    }

    public void OnPointerDown()
    {
        try
        {
            if (!IsEnemy && !LevelSettings.Default.IsFightActive && !LevelSettings.Default.IsPostFight && !IsTempUnit)
            {
                IsGrabbed = true;
                _reallyGrabbed = true;
                Offset = transform.position - GetPointerPosition();

                if (!GameData.Default.doPromoMerging)
                {
                    SoundHolder.Default.PlayFromSoundPack("UnitGrab");
                }
                else
                {
                    allAffectedUnits.Clear();
                }
            }
        }
        catch (NullReferenceException e)
        {
            Debug.Log(e);
            IsGrabbed = false;
            _reallyGrabbed = false;
        }

        if (LevelSettings.Default.IsFightActive && IsEnemy && !IsDead)
        {
            if (Time.time - LastTap > 0.5f)
            {
                LastTap = Time.time;
                
                TakeDamage(Description.DamageFromLightningPercent, true);

                LightningStrike?.Play();
                SoundHolder.Default.PlayFromSoundPack("Lightning");
            }
        }
    }

    public void OnPointerDrag()
    {
        if (IsGrabbed)
        {
            if (!GameData.Default.doPromoMerging)
            {
                transform.position = GetPointerPosition() + Offset;

                if (highlightedCell != null)
                {
                    highlightedCell.IsHighlighted = false;
                }

                Unit overlap;
                int closestCell = ParentGrid.GetClosestCell(transform.position, this, out overlap);

                if (closestCell >= 0)
                {
                    highlightedCell = ParentGrid.Cells[closestCell];
                    highlightedCell.IsHighlighted = true;
                }
            }
            else
            {
                Unit overlap;
                int closestCell = ParentGrid.GetClosestCell(GetPointerPosition(), this, out overlap);

                if (overlap != this && overlap != null && !allAffectedUnits.Contains(overlap))
                {
                    allAffectedUnits.Add(overlap);
                }
            }
        }
    }

    public void OnPointerUp()
    {
        if (highlightedCell != null)
        {
            highlightedCell.IsHighlighted = false;
        }

        if (IsGrabbed)
        {
            IsGrabbed = false;
            _reallyGrabbed = false;
            IsAnimated = true;

            if (!GameData.Default.doPromoMerging)
            {
                Unit overlap;
                var tempIndex = GridCellIndex;
                GridCellIndex = ParentGrid.GetClosestCell(transform.position, this, out overlap);
                bool newCell = GridCellIndex != tempIndex;
                transform.DOMove(ParentGrid.GetCellPosition(GridCellIndex), 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    IsAnimated = false;

                    if (overlap != null && overlap != this && overlap.Description == Description && Description.TurnInto != null)
                    {
                        CombineWith(overlap);
                    }
                    else
                    {
                        //if (newCell)
                        //{
                        //    LevelManager.Default.JustFailed = false;
                        //}

                        UnitManager.Default.SaveState();

                        if (LevelSettings.TutorialStage == 2 && UnitManager.Default.PlayerUnits.Count >= 2 && Description.IsAnimal && GridCellIndex < 5)
                        {
                            LevelSettings.TutorialStage++;
                        }
                    }
                });
                if (overlap != null && overlap != this && !(overlap.Description == Description && Description.TurnInto != null))
                {
                    overlap.GridCellIndex = tempIndex;
                    overlap.IsAnimated = true;
                    overlap.transform.DOMove(ParentGrid.GetCellPosition(overlap.GridCellIndex), 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
                    {
                        overlap.IsAnimated = false;
                        //LevelManager.Default.JustFailed = false;
                    });
                }

                SoundHolder.Default.PlayFromSoundPack("UnitGrab");
            }
            else
            {
                if (allAffectedUnits.Count > 0)
                {
                    if (!CombineAll())
                    {
                        Unit overlap;
                        var tempIndex = GridCellIndex;
                        GridCellIndex = ParentGrid.GetClosestCell(GetPointerPosition(), this, out overlap);
                        bool newCell = GridCellIndex != tempIndex;
                        if (overlap == null)
                        {
                            transform.DOMove(ParentGrid.GetCellPosition(GridCellIndex), 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
                            {
                                IsAnimated = false;

                                //if (newCell)
                                //{
                                //    LevelManager.Default.JustFailed = false;
                                //}

                                UnitManager.Default.SaveState();
                            });
                        }
                        else
                        {
                            IsAnimated = false;
                            GridCellIndex = tempIndex;
                        }
                    }
                }
                else
                {
                    Unit overlap;
                    var tempIndex = GridCellIndex;
                    GridCellIndex = ParentGrid.GetClosestCell(GetPointerPosition(), this, out overlap);
                    bool newCell = GridCellIndex != tempIndex;
                    if (overlap == null)
                    {
                        transform.DOMove(ParentGrid.GetCellPosition(GridCellIndex), 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
                        {
                            IsAnimated = false;

                            //if (newCell)
                            //{
                            //    LevelManager.Default.JustFailed = false;
                            //}

                            UnitManager.Default.SaveState();
                        });
                    }
                    else
                    {
                        IsAnimated = false;
                        GridCellIndex = tempIndex;
                    }
                }
            }
        }
    }

    bool CombineAll()
    {
        foreach (var unit in allAffectedUnits)
        {
            if (unit == null || unit.Description != Description)
            {
                return false;
            }
        }

        Vector3 center = transform.position;

        foreach (var unit in allAffectedUnits)
        {
            center += unit.transform.position;
        }

        center /= allAffectedUnits.Count + 1;

        Unit overlap;
        int cellIndex = ParentGrid.GetClosestCell(center, this, out overlap);

        if (overlap != null && overlap != this && !allAffectedUnits.Contains(overlap))
        {
            return false;
        }

        var baseUnit = Description.TurnInto;

        for (int i = 0; i < allAffectedUnits.Count - 1; i++)
        {
            if (baseUnit.TurnInto == null)
            {
                break;
            }

            baseUnit = baseUnit.TurnInto;
        }

        GridCellIndex = cellIndex;

        var copy = new List<Unit>(allAffectedUnits);

        transform.DOMove(ParentGrid.GetCellPosition(GridCellIndex), 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            var list = !IsEnemy ? UnitManager.Default.PlayerUnits : UnitManager.Default.EnemyUnits;

            list.Remove(this);

            foreach (var unit in copy)
            {
                list.Remove(unit);
            }

            IsAnimated = false;

            var newUnit = UnitManager.Default.SpawnUnit(baseUnit.Prefab, GridCellIndex, IsEnemy, true);

            LevelManager.BlockedFromProceeding = false;

            Destroy(gameObject);

            foreach (var unit in copy)
            {
                Destroy(unit.gameObject);
            }

            UnitManager.Default.SaveState();
            SoundHolder.Default.PlayFromSoundPack("MergeSound");
        });

        foreach (var unit in allAffectedUnits)
        {
            unit.GridCellIndex = cellIndex;
            unit.IsAnimated = true;
            unit.transform.DOMove(ParentGrid.GetCellPosition(unit.GridCellIndex), 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                unit.IsAnimated = false;
            });
        }

        allAffectedUnits.Clear();

        return true;
    }

    void CombineWith(Unit overlap)
    {
        AnimatorOffset.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InOutCubic);
        overlap.AnimatorOffset.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            var list = !IsEnemy ? UnitManager.Default.PlayerUnits : UnitManager.Default.EnemyUnits;

            list.Remove(this);
            list.Remove(overlap);

            var newUnit = UnitManager.Default.SpawnUnit(Description.TurnInto.Prefab, GridCellIndex, IsEnemy, true);

            newUnit.IsGrabbed = IsGrabbed || overlap.IsGrabbed;
            newUnit.Offset = IsGrabbed ? Offset : overlap.Offset;

            Destroy(gameObject);
            Destroy(overlap.gameObject);

            UnitManager.Default.SaveState();

            SoundHolder.Default.PlayFromSoundPack("MergeSound");
        });

        if (LevelSettings.TutorialStage == 4.5f)
        {
            LevelSettings.TutorialStage = 4;
        }

        if (LevelManager.UsesModifiedDifficulty && LevelManager.UsesModifiedDifficulty2)
        {
            LevelManager.UsesModifiedDifficulty2 = false;
            UnitManager.DecreaseEnemyToughness();
        }

        LevelManager.BlockedFromProceeding = false;
    }

    public void DoSpawnAnimation(bool newUnit)
    {
        var finalScale = AnimatorOffset.localScale;
        AnimatorOffset.localScale = Vector3.zero;
        AnimatorOffset.DOScale(finalScale, 0.2f).SetEase(Ease.InCubic);
        UpgradeParticles?.Play();
        if (newUnit)
        {
            transform.DORotate(new Vector3(0f, -160f, 0f), 1f).SetEase(Ease.InOutCubic);
        }
    }

    public void FinishSpawnNewUnitAnimation()
    {
        transform.DORotate(new Vector3(0f, 0f, 0f), 0.5f).SetEase(Ease.InOutCubic);
    }

    public void TakeDamage(float damageAmount, bool percent)
    {
        if (IsDead)
        {
            return;
        }

        if (percent)
        {
            if (LevelSettings.Default.Description.IsBoss && IsEnemy)
            {
                damageAmount = Mathf.Min(damageAmount, 0.1f);
            }

            CurrentHP -= Mathf.Max(1f, damageAmount * MaxHP);
        }
        else
        {
            damageAmount *= UnitManager.DamageMultiplier;
            CurrentHP -= damageAmount;
        }

        if (IsEnemy && GameData.Default.doPromoDamageOnEnemy || !IsEnemy && GameData.Default.doPromoDamageOnPlayer)
        {
            CurrentHP = 0f;
        }

        if (CurrentHP <= 0f)
        {
            Die();
        }
        else
        {
            if (Time.time - LastVibration > 1f)
            {
                LastVibration = Time.time;
                MMVibrationManager.Haptic(HapticTypes.MediumImpact, false, true, this);
            }
        }

        _glow = 1f;
    }

    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        CurrentHP = 0f;
        IsDead = true;

        if (IsEnemy && LevelSettings.Default.IsFightActive)
        {
            UnitManager.Default.DropLoot(this);
        }

        UnitManager.Default.OnUnitDied();

        MMVibrationManager.Haptic(HapticTypes.HeavyImpact, false, true, this);
    }

    public void FastForwardDeath()
    {
        CurrentHP = 0f;
        IsDead = true;
        _deathTimer = 100f;
    }

    public virtual void Attack(Unit opponent, bool noDamage = false)
    {
        if (!noDamage)
        {
            opponent.TakeDamage(Damage, Description.DamageInPercent);
        }

        OnAttack?.Invoke();

        DoAttackSound();
        DoAttackDamageSound();
    }

    public void DoAttackSound()
    {
        if (!string.IsNullOrEmpty(AttackSound))
        {
            SoundHolder.Default.PlayFromSoundPack(AttackSound);
        }
    }

    public void DoAttackDamageSound()
    {
        if (!string.IsNullOrEmpty(AttackDamageSound))
        {
            SoundHolder.Default.PlayFromSoundPack(AttackDamageSound);
        }
    }

    internal Unit GetClosestOpponent()
    {
        Unit closestOpponent = null;

        var opponents = IsEnemy ? UnitManager.Default.PlayerUnits : UnitManager.Default.EnemyUnits;

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
