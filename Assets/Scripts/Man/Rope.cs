using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [Header("Points")]
    public List<Point> points;
    public Point Last { get { return points[points.Count - 1]; } }
    public Point First { get { return points[0]; } }
    public int NumIteraion;
    [Header("Lenght Settings")]
    public bool FixedLenght;
    public float RopeLenght;
    public float LenghtRatio;
    [Header("Other Settings")]
    public float Smooth;
    public bool UseUniyGravity;
    public Vector3 GravityScale;
    public Vector3 Gravity()
    {
        if(UseUniyGravity)
        {
            return Physics.gravity;
        }
        else
        {
            return GravityScale;
        }
    }
    private float Lenght()
    {
        if (FixedLenght)
        {
            return RopeLenght / points.Count;
        }
        else
        {
            return (First.position - Last.position).magnitude / points.Count * LenghtRatio;
        }
    }

    private bool CanSimulate;

    private void Init()
    {
        UseUniyGravity = false;

        foreach (Point p in points)
        {
            p.prevPosition = p.position;
        }
        Simulate();
    }
    private void Simulate()
    {
        if (!CanSimulate)
            return;
        foreach(Point p in points)
        {
            if(!p.locked)
            {
                Vector3 positionBeforeUpdate = p.position;
                Vector3 directionBeforeUpdate = p.position - p.prevPosition;
                p.position += p.position - p.prevPosition;
                p.position = Vector3.Lerp(p.position, positionBeforeUpdate, Smooth);
                p.position += Gravity() * Time.deltaTime * Time.deltaTime;
                p.prevPosition = positionBeforeUpdate;
            }
        }
        for (int i = 0; i < NumIteraion; i++)
        {
            for(int a = 0; a < points.Count - 1; a++)
            {
                Vector3 StickCentre = (points[a].position + points[a + 1].position) / 2;
                Vector3 StickDir = (points[a].position - points[a + 1].position).normalized;
                if (!points[a].locked)
                {
                    points[a].position = StickCentre + StickDir * Lenght() / 2;
                }
                if (!points[a + 1].locked)
                {
                    points[a + 1].position = StickCentre - StickDir * Lenght() / 2;
                    points[a + 1].direction = -StickDir;

                }
                
            }
        }

        Vector3 LastDirection = (Last.position - First.position).normalized;
        Last.direction = LastDirection;
    }

    private IEnumerator DelaySimulation()
    {

        for (int i = 0; i < 100; i++)
        {
            for (int a = 0; a < points.Count - 1; a++)
            {
                Vector3 StickCentre = (points[a].position + points[a + 1].position) / 2;
                Vector3 StickDir = (points[a].position - points[a + 1].position).normalized;
                if (!points[a].locked)
                {
                    points[a].position = StickCentre + StickDir * Lenght() / 2;
                }
                if (!points[a + 1].locked)
                {
                    points[a + 1].position = StickCentre - StickDir * Lenght() / 2;
                    points[a + 1].direction = -StickDir;

                }

            }
        }
        CanSimulate = true;
        yield break;
    }

    private void Start()
    {
        StartCoroutine(DelaySimulation());
        Init();
    }
    private void Update()
    {
        Simulate();
    }
}