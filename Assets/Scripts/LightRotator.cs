using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BG.UI.Camera;

public class LightRotator : MonoBehaviour
{
    [SerializeField] protected Vector3 rot0, rot1;
    private bool state;
    void Start()
    {
        LevelManager.Default.OnLevelStarted += LightSwitch;
        LevelManager.Default.OnLevelLoad += LightSwitch;
		transform.eulerAngles = rot0;
    }
    private void OnDestroy()
    {
        LevelManager.Default.OnLevelStarted -= LightSwitch;
        LevelManager.Default.OnLevelLoad -= LightSwitch;
    }



    private void LightSwitch()
    {
        if (!LevelManager.Default.isBonus)
        {
            if (state)
                transform.eulerAngles = rot0;
            else
                transform.eulerAngles = rot1;
            state = !state;
        }
    }
}
