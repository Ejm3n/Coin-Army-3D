using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Opponent")]
public class OpponentDescription : ScriptableObject
{
    public string Username;
    public Sprite Avatar;
    public List<UnitSetting> UnitsSetup;
    public int ShieldsAmount;
    public ulong MoneyAmount;
    public GameObject HomeLocation;
}
