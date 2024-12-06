using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIColorBehaviour : MonoBehaviour
{
    private void Start()
    {
        Image image = GetComponent<Image>();
        image.color = GameData.Default.UIColor;
    }
}
