using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/PhysicMaterialHolder")]
public class PhysicMaterialHolder : DataHolder
{
    #region Singleton

    private static PhysicMaterialHolder _default;
    public static PhysicMaterialHolder Default => _default;

    #endregion

    [Header("CurrentPhysicMaterial")]
    public PhysicMaterial physicMaterial;


    public override void Init()
    {
        _default = this;
    }
}
