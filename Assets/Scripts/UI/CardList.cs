using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CardList : MonoBehaviour
{
    public GameObject CardPrefab;
    public RectTransform Content;
    public Toggle IsAnimal;
    public Material LockedUnitMaterial;

    private List<CardListEntry> _entries = new List<CardListEntry>();
    private List<int> _allUnits;

    private int category = -1;

    void Start()
    {
        OnCategoryChange();
    }

    public void OnCategoryChange()
    {
        int newCat = IsAnimal.isOn ? 0 : 1;

        if (newCat == category)
        {
            return;
        }

        category = newCat;

        foreach (var entry in _entries)
        {
            Destroy(entry.gameObject);
        }

        _entries.Clear();

        _allUnits = GameData.Default.AllUnits.Where(x => x.PlayerUsable && x.IsAnimal == IsAnimal.isOn).OrderBy(x => x.CardListOrder).Select(x => GameData.Default.AllUnits.IndexOf(x)).ToList();

        foreach (var unit in _allUnits)
        {
            var entry = Instantiate(CardPrefab, Content).GetComponent<CardListEntry>();
            _entries.Add(entry);
        }

        UpdateList();

        GetComponentInChildren<Scrollbar>().value = 1f;
    }

    public void UpdateList()
    {
        var unlockedCards = CardsService.Default.GetPlayerCards();

        for (int i = 0; i < _allUnits.Count; i++)
        {
            var desc = GameData.Default.AllUnits[_allUnits[i]];
            var entry = _entries[i];
            var unlocked = unlockedCards.Contains(_allUnits[i]);
            
            if (unlocked)
            {
                entry.UnitName.text = Language.Text(desc.UnitName);
                entry.UnitHP.text = MoneyService.AmountToString(desc.HP);
                entry.UnitDamage.text = MoneyService.AmountToString(desc.Damage);
            }
            else
            {
                entry.UnitName.text = Language.Text("Locked");
                entry.UnitHP.text = "???";
                entry.UnitDamage.text = "???";
            }

            entry.LoadPreview(desc, unlocked, LockedUnitMaterial);
        }
    }

    public static Transform SpawnUnitPreview(UnitSetting desc, bool unlocked, Material lockedMaterial, bool useMask)
    {
        var prefab = desc.Prefab.GetComponent<Unit>().Animator.gameObject;
        var instance = Instantiate(prefab);

        foreach (var p in instance.GetComponentsInChildren<ParticleSystem>())
        {
            Destroy(p.gameObject);
        }

        if (!unlocked)
        {
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                var mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = lockedMaterial;
                }
                renderer.materials = mats;
            }
        }
        else
        {
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_OutlineWidth"))
                    {
                        mat.SetFloat("_OutlineWidth", mat.GetFloat("_OutlineWidth") * 0.1f);
                    }
                }
            }
        }

        if (useMask)
        {
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                renderer.gameObject.layer = 17;
            }
        }

        return instance.transform;
    }
}
