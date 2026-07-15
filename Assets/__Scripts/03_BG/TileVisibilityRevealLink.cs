using System.Collections.Generic;
using UnityEngine;

public sealed class TileVisibilityRevealLink : MonoBehaviour
{
    [SerializeField] private MonoBehaviour targetGroup;
    [SerializeField] private string roomId;
    [SerializeField] private List<MonoBehaviour> revealGroups = new List<MonoBehaviour>();

    public MonoBehaviour TargetGroup => targetGroup;
    public string RoomId => roomId;
    public IReadOnlyList<MonoBehaviour> RevealGroups => revealGroups;
}
