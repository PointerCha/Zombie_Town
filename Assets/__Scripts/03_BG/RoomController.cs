using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoomController : MonoBehaviour
{
    [SerializeField] private FloorController floorController;
    [SerializeField] private VisibilityTargetGroup underWallGroup;

    private readonly HashSet<UnitVisibilityAgent> occupants = new HashSet<UnitVisibilityAgent>();

    public FloorController FloorController => floorController;

    private void Awake()
    {
        if (floorController == null)
        {
            floorController = GetComponentInParent<FloorController>();
        }
    }

    public void RegisterOccupant(UnitVisibilityAgent agent)
    {
        if (agent == null || !occupants.Add(agent))
        {
            return;
        }

        if (occupants.Count == 1 && underWallGroup != null)
        {
            underWallGroup.RequestReveal();
        }
    }

    public void UnregisterOccupant(UnitVisibilityAgent agent)
    {
        if (agent == null || !occupants.Remove(agent))
        {
            return;
        }

        if (occupants.Count == 0 && underWallGroup != null)
        {
            underWallGroup.ReleaseReveal();
        }
    }
}
