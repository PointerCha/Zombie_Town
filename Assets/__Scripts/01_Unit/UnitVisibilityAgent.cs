using UnityEngine;
using System.Collections.Generic;

public sealed class UnitVisibilityAgent : MonoBehaviour
{
    private const int OverlapBufferSize = 32;

    private readonly HashSet<FloorController> registeredFloors = new HashSet<FloorController>();
    private readonly HashSet<RoomController> registeredRooms = new HashSet<RoomController>();
    private readonly HashSet<FloorController> detectedFloors = new HashSet<FloorController>();
    private readonly HashSet<RoomController> detectedRooms = new HashSet<RoomController>();
    private readonly Collider2D[] overlapBuffer = new Collider2D[OverlapBufferSize];

    private UnitFloorAgent floorAgent;
    private ContactFilter2D overlapFilter;
    private bool isSyncingOverlaps;
    private bool resyncRequested;

    public bool IsInsideFloor => registeredFloors.Count > 0;

    private void Awake()
    {
        floorAgent = GetComponent<UnitFloorAgent>();
        overlapFilter = new ContactFilter2D
        {
            useTriggers = true
        };
        overlapFilter.SetLayerMask(Physics2D.AllLayers);
    }

    private void OnEnable()
    {
        if (floorAgent != null)
        {
            floorAgent.FloorChanged += HandleFloorChanged;
        }
    }

    private void OnDisable()
    {
        if (floorAgent != null)
        {
            floorAgent.FloorChanged -= HandleFloorChanged;
        }

        ClearRegistrations();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SyncCurrentFloorOverlaps();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        SyncCurrentFloorOverlaps();
    }

    private void HandleFloorChanged(UnitFloorAgent unit, FloorController previousFloor, FloorController currentFloor)
    {
        ClearRegistrations();

        if (isSyncingOverlaps)
        {
            resyncRequested = true;
            return;
        }

        SyncCurrentFloorOverlaps();
    }

    private bool IsCurrentFloor(FloorController floorController)
    {
        return floorAgent == null || floorAgent.CurrentFloor == floorController;
    }

    private void ClearRegistrations()
    {
        foreach (FloorController floorController in registeredFloors)
        {
            if (floorController != null)
            {
                floorController.UnregisterOccupant(this);
            }
        }

        foreach (RoomController roomController in registeredRooms)
        {
            if (roomController != null)
            {
                roomController.UnregisterOccupant(this);
            }
        }

        registeredFloors.Clear();
        registeredRooms.Clear();
        detectedFloors.Clear();
        detectedRooms.Clear();
    }

    private void SyncCurrentFloorOverlaps()
    {
        if (floorAgent == null)
        {
            return;
        }

        if (isSyncingOverlaps)
        {
            resyncRequested = true;
            return;
        }

        isSyncingOverlaps = true;
        do
        {
            resyncRequested = false;
            detectedFloors.Clear();
            detectedRooms.Clear();

            Collider2D[] unitColliders = floorAgent.Colliders;
            for (int i = 0; i < unitColliders.Length; i++)
            {
                Collider2D unitCollider = unitColliders[i];
                if (unitCollider == null)
                {
                    continue;
                }

                int hitCount = unitCollider.OverlapCollider(overlapFilter, overlapBuffer);
                for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
                {
                    TryDetect(overlapBuffer[hitIndex]);
                    overlapBuffer[hitIndex] = null;
                }
            }

            ApplyDetectedRegistrations();
        }
        while (resyncRequested);

        isSyncingOverlaps = false;
    }

    private void TryDetect(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        if (floorAgent != null && !floorAgent.CanCollideWith(other))
        {
            return;
        }

        FloorController floorController = other.GetComponent<FloorController>();
        if (floorController != null && (floorAgent == null || floorAgent.TryEnterFloor(floorController)))
        {
            detectedFloors.Add(floorController);
        }

        RoomController roomController = other.GetComponent<RoomController>();
        if (roomController != null && IsCurrentFloor(roomController.FloorController))
        {
            detectedRooms.Add(roomController);
        }
    }

    private void ApplyDetectedRegistrations()
    {
        foreach (FloorController floorController in detectedFloors)
        {
            if (floorController != null && registeredFloors.Add(floorController))
            {
                floorController.RegisterOccupant(this);
            }
        }

        foreach (RoomController roomController in detectedRooms)
        {
            if (roomController != null && registeredRooms.Add(roomController))
            {
                roomController.RegisterOccupant(this);
            }
        }

        registeredFloors.RemoveWhere(floorController =>
        {
            if (floorController == null)
            {
                return true;
            }

            if (detectedFloors.Contains(floorController))
            {
                return false;
            }

            floorController.UnregisterOccupant(this);
            return true;
        });

        registeredRooms.RemoveWhere(roomController =>
        {
            if (roomController == null)
            {
                return true;
            }

            if (detectedRooms.Contains(roomController))
            {
                return false;
            }

            roomController.UnregisterOccupant(this);
            return true;
        });
    }
}
