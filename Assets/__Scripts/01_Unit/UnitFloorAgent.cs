using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UnitFloorAgent : MonoBehaviour
{
    [SerializeField] private BuildingController buildingController;
    [SerializeField] private FloorController initialFloor;
    [SerializeField] private Collider2D[] colliders;

    private FloorController currentFloor;
    private StairFloorSwitchZone activeStairZone;
    private StairEndpointZone stairEntryEndpoint;
    private StairEndpointZone stairTargetEndpoint;
    private FloorController stairEntryFloor;
    private FloorController stairTargetFloor;
    private StairFloorSwitchZone blockedStairZone;
    private StairEndpointZone blockedStairEndpoint;
    private float blockedStairZoneUntil;
    private int lastFloorChangeFrame = -1;

    public event Action<UnitFloorAgent, FloorController, FloorController> FloorChanged;

    public FloorController CurrentFloor => currentFloor;
    public Collider2D[] Colliders => colliders;

    private void Awake()
    {
        if (colliders == null || colliders.Length == 0)
        {
            colliders = FindPhysicsColliders();
        }

        if (buildingController == null && initialFloor != null)
        {
            buildingController = initialFloor.BuildingController;
        }

        currentFloor = initialFloor;
        ApplyCurrentFloor();
    }

    private void Start()
    {
        FloorChanged?.Invoke(this, null, currentFloor);
    }

    private void FixedUpdate()
    {
        ApplyCurrentFloor();
    }

    public void SetCurrentFloor(FloorController targetFloor)
    {
        TrySetCurrentFloor(targetFloor);
    }

    public bool CanCollideWith(Collider2D target)
    {
        if (target == null || buildingController == null || currentFloor == null)
        {
            return true;
        }

        FloorController targetFloor = buildingController.GetFloorForCollider(target);
        return targetFloor == null || targetFloor == currentFloor;
    }

    public bool TrySetCurrentFloor(FloorController targetFloor)
    {
        if (targetFloor == null || targetFloor == currentFloor)
        {
            return false;
        }

        if (lastFloorChangeFrame == Time.frameCount)
        {
            return false;
        }

        FloorController previousFloor = currentFloor;
        BuildingController previousBuilding = buildingController;
        currentFloor = targetFloor;

        BuildingController targetBuilding = targetFloor.BuildingController;
        if (targetBuilding != null)
        {
            buildingController = targetBuilding;
        }

        if (previousBuilding != null && previousBuilding != buildingController && previousFloor != null)
        {
            previousBuilding.NotifyFloorExited(previousFloor);
        }

        if (buildingController != null)
        {
            buildingController.NotifyUnitFloorChanged(currentFloor);
        }

        lastFloorChangeFrame = Time.frameCount;

        ApplyCurrentFloor();
        FloorChanged?.Invoke(this, previousFloor, currentFloor);
        return true;
    }

    public bool TryEnterFloor(FloorController targetFloor)
    {
        if (targetFloor == null)
        {
            return false;
        }

        if (targetFloor == currentFloor)
        {
            return true;
        }

        BuildingController targetBuilding = targetFloor.BuildingController;
        if (currentFloor == null || buildingController == null || targetBuilding != buildingController)
        {
            return TrySetCurrentFloor(targetFloor);
        }

        return false;
    }

    public bool TryEnterStairEndpoint(StairFloorSwitchZone stairZone, StairEndpointZone endpoint)
    {
        if (stairZone == null || endpoint == null || stairZone.LowerFloor == null || stairZone.UpperFloor == null)
        {
            return false;
        }

        if (activeStairZone == stairZone)
        {
            return ResolveActiveStairEndpoint(stairZone, endpoint);
        }

        if (activeStairZone != null)
        {
            return false;
        }

        if (blockedStairZone == stairZone && blockedStairEndpoint == endpoint && Time.time < blockedStairZoneUntil)
        {
            return false;
        }

        if (endpoint == stairZone.LowerEndpoint && currentFloor == stairZone.LowerFloor)
        {
            return BeginStairTransition(stairZone, stairZone.LowerEndpoint, stairZone.UpperEndpoint, stairZone.LowerFloor, stairZone.UpperFloor);
        }

        if (endpoint == stairZone.UpperEndpoint && currentFloor == stairZone.UpperFloor)
        {
            return BeginStairTransition(stairZone, stairZone.UpperEndpoint, stairZone.LowerEndpoint, stairZone.UpperFloor, stairZone.LowerFloor);
        }

        return false;
    }

    private bool BeginStairTransition(
        StairFloorSwitchZone stairZone,
        StairEndpointZone entryEndpoint,
        StairEndpointZone targetEndpoint,
        FloorController entryFloor,
        FloorController targetFloor)
    {
        activeStairZone = stairZone;
        stairEntryEndpoint = entryEndpoint;
        stairTargetEndpoint = targetEndpoint;
        stairEntryFloor = entryFloor;
        stairTargetFloor = targetFloor;

        bool switched = TrySetCurrentFloor(stairTargetFloor);
        if (!switched)
        {
            ClearActiveStair();
        }

        return switched;
    }

    private bool ResolveActiveStairEndpoint(StairFloorSwitchZone stairZone, StairEndpointZone endpoint)
    {
        if (endpoint == stairEntryEndpoint)
        {
            TrySetCurrentFloor(stairEntryFloor);
            BlockStairReentry(stairZone, endpoint);
            ClearActiveStair();
            return true;
        }

        if (endpoint == stairTargetEndpoint)
        {
            BlockStairReentry(stairZone, endpoint);
            ClearActiveStair();
            return true;
        }

        return false;
    }

    private void ClearActiveStair()
    {
        activeStairZone = null;
        stairEntryEndpoint = null;
        stairTargetEndpoint = null;
        stairEntryFloor = null;
        stairTargetFloor = null;
    }

    private void BlockStairReentry(StairFloorSwitchZone stairZone, StairEndpointZone endpoint)
    {
        if (stairZone == null || endpoint == null)
        {
            return;
        }

        blockedStairZone = stairZone;
        blockedStairEndpoint = endpoint;
        blockedStairZoneUntil = Time.time + stairZone.CompletedReentryBlockDuration;
    }

    private void ApplyCurrentFloor()
    {
        if (buildingController == null || currentFloor == null)
        {
            return;
        }

        buildingController.ApplyFloorCollisionAccess(this, currentFloor);
    }

    private Collider2D[] FindPhysicsColliders()
    {
        Collider2D[] foundColliders = GetComponentsInChildren<Collider2D>();
        List<Collider2D> physicsColliders = new List<Collider2D>(foundColliders.Length);

        for (int i = 0; i < foundColliders.Length; i++)
        {
            Collider2D foundCollider = foundColliders[i];
            if (foundCollider == null || foundCollider.GetComponent<UnitSelectionTarget>() != null)
            {
                continue;
            }

            physicsColliders.Add(foundCollider);
        }

        return physicsColliders.ToArray();
    }
}
