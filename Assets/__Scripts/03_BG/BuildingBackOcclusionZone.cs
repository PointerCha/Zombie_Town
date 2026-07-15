using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BuildingBackOcclusionZone : MonoBehaviour
{
    [SerializeField] private BuildingController buildingController;

    private readonly HashSet<UnitFloorAgent> occupants = new HashSet<UnitFloorAgent>();

    private void Awake()
    {
        if (buildingController == null)
        {
            buildingController = GetComponentInParent<BuildingController>();
        }
    }

    private void OnDisable()
    {
        if (buildingController != null)
        {
            foreach (UnitFloorAgent occupant in occupants)
            {
                if (occupant != null)
                {
                    buildingController.ReleaseBackOcclusion();
                }
            }
        }

        occupants.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        UnitFloorAgent unit = GetValidUnit(other);
        if (unit == null || !occupants.Add(unit) || buildingController == null)
        {
            return;
        }

        buildingController.RequestBackOcclusion();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        UnitFloorAgent unit = GetValidUnit(other);
        if (unit == null || !occupants.Remove(unit) || buildingController == null)
        {
            return;
        }

        buildingController.ReleaseBackOcclusion();
    }

    private static UnitFloorAgent GetValidUnit(Collider2D other)
    {
        if (other == null || other.GetComponent<UnitSelectionTarget>() != null)
        {
            return null;
        }

        UnitFloorAgent unit = other.GetComponentInParent<UnitFloorAgent>();
        if (unit == null || !IsRegisteredUnitCollider(unit, other))
        {
            return null;
        }

        return unit;
    }

    private static bool IsRegisteredUnitCollider(UnitFloorAgent unit, Collider2D target)
    {
        Collider2D[] colliders = unit.Colliders;
        if (colliders == null)
        {
            return false;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == target)
            {
                return true;
            }
        }

        return false;
    }
}
