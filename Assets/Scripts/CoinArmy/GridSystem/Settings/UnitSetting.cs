using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Unit Description")]
public class UnitSetting : ScriptableObject
{
    [Header("References")]
    public GameObject Prefab;
    public UnitSetting TurnInto;
    public int UnitLevel = -1;
    [Header("Damage System")]
    public float HP;
    public float Damage;
    public bool DamageInPercent;
    public bool DamageEveryone;
    public float DamageDelay;
    public float DamageApplyDelay;
    public float DamageFromLightningPercent;
    [Header("Economy")]
    public ulong Price;
    public ulong BaseRewardForKill;
    public ulong RewardForKillRaise;
    [Header("Common")]
    public float RotationSpeed;
    [Header("Short Range")]
    public float MoveSpeed;
    public float StopDistance;
    [Header("Long Range")]
    public float ArrowSpeed;
    public float ArrowDamageRadius;
    [Header("Card")]
    public string UnitName;
    public bool IsAnimal;
    public bool PlayerUsable;
    public int CardListOrder;
    public Vector3 CardPreviewOffset;
    public float CardPreviewScale;
}
