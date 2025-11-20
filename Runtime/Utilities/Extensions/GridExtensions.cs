
using UnityEngine;

public static class GridExtensions
{
    public static Vector3 WorldToCellCenterWorld(this Grid grid, Vector3 worldPosition)
    {
        var cellPosition = grid.WorldToCell(worldPosition);
        return grid.GetCellCenterWorld(cellPosition);
    }
}