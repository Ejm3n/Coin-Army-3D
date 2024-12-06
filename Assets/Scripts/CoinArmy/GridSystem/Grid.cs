using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector2Int Size;
    public List<Cell> Cells = new List<Cell>();
    public bool IsEnemyGrid;

    public Vector3 GetCellPosition(int cellIndex)
    {
        return Cells[Mathf.Clamp(cellIndex, 0, Cells.Count - 1)].transform.position;
    }

    private int[] pickAnyFreeCellTable =
    {
        4, 3, 2, 1, 0,
        9, 8, 7, 6, 5,
        14, 13, 12, 11, 10,
        15, 14, 13, 12, 11,
        20, 19, 18, 17, 16
    };

    public int PickAnyFreeCell(bool frontToBack)
    {
        if (frontToBack)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (!IsCellOccupied(pickAnyFreeCellTable[i], out Unit overlap))
                {
                    return pickAnyFreeCellTable[i];
                }
            }
        }
        else
        {
            for (int i = Cells.Count - 1; i >= 0; i--)
            {
                if (!IsCellOccupied(pickAnyFreeCellTable[i], out Unit overlap))
                {
                    return pickAnyFreeCellTable[i];
                }
            }
        }

        return -1;
    }

    public bool HasFreeSpace()
    {
        return PickAnyFreeCell(true) != -1;
    }

    public bool IsEmpty()
    {
        for (int i = Cells.Count - 1; i >= 0; i--)
        {
            if (IsCellOccupied(i, out Unit overlap))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsCellOccupied(int cellIndex, out Unit overlap)
    {
        overlap = null;

        var unitList = IsEnemyGrid ? UnitManager.Default.EnemyUnits : UnitManager.Default.PlayerUnits;

        foreach (var unit in unitList)
        {
            if (unit.GridCellIndex == cellIndex)
            {
                overlap = unit;

                return true;
            }
        }

        return false;
    }

    public int GetClosestCell(Vector3 position, Unit currentUnit, out Unit overlap)
    {
        overlap = null;

        int closestCell = -1;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < Cells.Count; i++)
        {
            if (IsCellOccupied(i, out Unit overlapTemp))
            {
                /*if (currentUnit.Description.TurnInto == null && overlapTemp != currentUnit || overlapTemp.Description != currentUnit.Description)
                {
                    continue;
                }*/
            }

            float dist = Vector3.Distance(position, GetCellPosition(i));

            if (i == 0 || dist < closestDistance)
            {
                closestCell = i;
                closestDistance = dist;
                overlap = overlapTemp;
            }
        }

        return closestCell;
    }
}
