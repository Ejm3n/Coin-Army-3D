using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point : MonoBehaviour
{
    public Vector3 position { get { return transform.position; } set { transform.position = value; } }
    public Vector3 direction { get { return transform.up; } set { transform.up = value; } }
    public Vector3 prevPosition;
    public bool locked;

}
