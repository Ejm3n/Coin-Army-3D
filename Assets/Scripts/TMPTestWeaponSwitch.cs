using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMPTestWeaponSwitch : MonoBehaviour
{
    public GameObject weapon, vaccum;
    private bool seted;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (seted)
            {
                weapon.SetActive(true);
                vaccum.SetActive(false);
            }
            else
            {
                weapon.SetActive(false);
                vaccum.SetActive(true);
            }
            seted = !seted;
        }
    }
}
