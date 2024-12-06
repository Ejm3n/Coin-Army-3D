using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TotalHPBar : MonoBehaviour
{
    public RectTransform SliderRT;
    public float EmptyX;
    public float FullX;
    public bool IsEnemy;

    private float _hp;

    void Update()
    {
        if (UnitManager.Default == null)
        {
            return;
        }

        var unitList = IsEnemy ? UnitManager.Default.EnemyUnits : UnitManager.Default.PlayerUnits;
        
        float currentHP = 0f;
        float maxHP = 0f;

        foreach (var unit in unitList)
        {
            currentHP += unit.CurrentHP;
            maxHP += unit.MaxHP;
        }

        float newhp = currentHP / Mathf.Max(1f, maxHP);

        if (newhp > _hp)
        {
            _hp = newhp;
        }
        else
        {
            _hp = Mathf.MoveTowards(_hp, newhp, Time.deltaTime * 0.5f);
        }

        SliderRT.anchoredPosition = new Vector2(Mathf.Lerp(EmptyX, FullX, _hp), SliderRT.anchoredPosition.y);
    }
}
