using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScroll : MonoBehaviour
{
    public MeshRenderer Renderer;
    public Vector2 Speed;

    void Update()
	{
        Renderer.material.mainTextureOffset += Speed * Time.deltaTime;
    }
}
