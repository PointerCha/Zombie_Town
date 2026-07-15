using System;
using UnityEngine;

[Serializable]
public class GeneratedBuildingGridProfile
{
    [SerializeField] private Vector3 visualCellSize = new Vector3(1f, 0.5f, 1f);
    [SerializeField] private Vector3 wallColliderCellSize = new Vector3(0.3f, 0.15f, 1f);
    [SerializeField] private Vector3 stairColliderCellSize = new Vector3(0.2f, 0.1f, 1f);
    [SerializeField] private GridLayout.CellLayout cellLayout = GridLayout.CellLayout.Isometric;

    public Vector3 VisualCellSize => visualCellSize;
    public Vector3 WallColliderCellSize => wallColliderCellSize;
    public Vector3 StairColliderCellSize => stairColliderCellSize;
    public GridLayout.CellLayout CellLayout => cellLayout;
}
