using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelIndicator : MonoBehaviour
{
    public Image[] Segments;
    public Sprite WhiteIcon;
    public Sprite YellowIcon;
    public Sprite GreenIcon;
    public TextMeshProUGUI Text;

    void Update()
    {
        Text.text = string.Format(Language.Text("LevelNum"), LevelManager.Default.CurrentLevelCount);

        for (int i = 0; i < Segments.Length; i++)
        {
            Segments[i].gameObject.SetActive(i < LevelSettings.Default.Description.LocationLength);
            Segments[i].sprite = i == LevelSettings.Default.Description.LocationProgress - 1 ? YellowIcon : i < LevelSettings.Default.Description.LocationProgress - 1 ? GreenIcon : WhiteIcon;
        }
    }
}
