using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GeneratedVisibilityGroup : MonoBehaviour
{
    [SerializeField] private string groupId;
    [SerializeField] private GeneratedVisibilityGroupKind groupKind;
    [SerializeField] private int floorIndex;
    [SerializeField] private int roomIndex;
    [SerializeField] private List<TilemapRenderer> targets = new List<TilemapRenderer>();

    public string GroupId => groupId;
    public GeneratedVisibilityGroupKind GroupKind => groupKind;
    public int FloorIndex => floorIndex;
    public int RoomIndex => roomIndex;
    public IReadOnlyList<TilemapRenderer> Targets => targets;
    public int TargetCount => targets.Count;

    public void Configure(
        string newGroupId,
        GeneratedVisibilityGroupKind newGroupKind,
        int newFloorIndex,
        int newRoomIndex
    )
    {
        groupId = newGroupId;
        groupKind = newGroupKind;
        floorIndex = newFloorIndex;
        roomIndex = newRoomIndex;
        targets.Clear();
    }

    public void AddTarget(TilemapRenderer target)
    {
        if (target == null || targets.Contains(target))
        {
            return;
        }

        targets.Add(target);
    }
}
