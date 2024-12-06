using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [SerializeField] private Material _from;
    [SerializeField] private Material _to;

    [ContextMenu("Change Materials")]
    private void ChangeMaterials() 
    {
        foreach (MeshRenderer rend in GetComponentsInChildren<MeshRenderer>()) 
        {
            List<Material> materials = new List<Material>(rend.sharedMaterials);
            if (materials.Contains(_from))
            {
                materials[materials.IndexOf(_from)] = _to;
                rend.sharedMaterials = materials.ToArray();
            }
        }
    }
}
