using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardListEntry : MonoBehaviour
{
    public TextMeshProUGUI UnitName;
    public TextMeshProUGUI UnitHP;
    public TextMeshProUGUI UnitDamage;
    public Transform PreviewParent;

    private Transform _existingPreview;

    public void LoadPreview(UnitSetting desc, bool unlocked, Material LockedUnitMaterial)
    {
        if (_existingPreview != null)
        {
            Destroy(_existingPreview.gameObject);
        }
        var preview = CardList.SpawnUnitPreview(desc, unlocked, LockedUnitMaterial, true);
        preview.SetParent(PreviewParent, false);
        preview.localPosition = desc.CardPreviewOffset;
        preview.localScale = Vector3.one * desc.CardPreviewScale;
        _existingPreview = preview;
    }
}
