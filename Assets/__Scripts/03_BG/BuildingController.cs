using System.Collections.Generic;
using UnityEngine;

public sealed class BuildingController : MonoBehaviour
{
    [SerializeField] private List<FloorController> floors = new List<FloorController>();
    [SerializeField] private VisibilityTargetGroup[] overheadVisibilityGroups;
    [SerializeField] private VisibilityTargetGroup[] backOcclusionGroups;
    [SerializeField] private float overheadTransitionDuration = 0.5f;
    [SerializeField] private float backOcclusionTransitionDuration = 0.15f;
    [SerializeField, Range(0f, 1f)] private float backOcclusionGroundFloorAlpha = 0.35f;

    private FloorController currentFloor;
    private int backOcclusionRequestCount;

    public FloorController CurrentFloor => currentFloor;

    private void OnValidate()
    {
        overheadTransitionDuration = Mathf.Max(0f, overheadTransitionDuration);
        backOcclusionTransitionDuration = Mathf.Max(0f, backOcclusionTransitionDuration);
        backOcclusionGroundFloorAlpha = Mathf.Clamp01(backOcclusionGroundFloorAlpha);
    }

    public void ApplyFloorCollisionAccess(UnitFloorAgent agent, FloorController activeFloor)
    {
        if (agent == null)
        {
            return;
        }

        for (int i = 0; i < floors.Count; i++)
        {
            FloorController floor = floors[i];
            if (floor == null)
            {
                continue;
            }

            floor.SetCollisionAccess(agent, floor == activeFloor);
        }

    }

    public void ApplyActiveFloor(FloorController activeFloor)
    {
        for (int i = 0; i < floors.Count; i++)
        {
            FloorController floor = floors[i];
            if (floor == null)
            {
                continue;
            }

            floor.SetFloorCollisionEnabled(floor == activeFloor);
        }
    }

    public FloorController GetFloorForCollider(Collider2D target)
    {
        if (target == null)
        {
            return null;
        }

        for (int i = 0; i < floors.Count; i++)
        {
            FloorController floor = floors[i];
            if (floor != null && floor.ContainsCollisionCollider(target))
            {
                return floor;
            }
        }

        return null;
    }

    public void NotifyFloorEntered(FloorController floor)
    {
        currentFloor = floor;
        ApplyFloorVisibility(floor);
    }

    public void NotifyUnitFloorChanged(FloorController floor)
    {
        currentFloor = floor;
        ApplyFloorVisibility(floor);
    }

    public void NotifyFloorExited(FloorController floor)
    {
        if (currentFloor == floor)
        {
            currentFloor = null;
            ApplyFloorVisibility(null);
        }
    }

    public void RequestBackOcclusion()
    {
        backOcclusionRequestCount++;
        if (backOcclusionRequestCount == 1)
        {
            ApplyBackOcclusionAlpha(0f, backOcclusionTransitionDuration);
        }
    }

    public void ReleaseBackOcclusion()
    {
        if (backOcclusionRequestCount == 0)
        {
            return;
        }

        backOcclusionRequestCount--;
        if (backOcclusionRequestCount == 0)
        {
            ApplyBackOcclusionAlpha(1f, backOcclusionTransitionDuration);
        }
    }

    private void ApplyFloorVisibility(FloorController activeFloor)
    {
        int activeFloorIndex = floors.IndexOf(activeFloor);

        for (int i = 0; i < floors.Count; i++)
        {
            FloorController floor = floors[i];
            if (floor == null)
            {
                continue;
            }

            bool isVisible = activeFloorIndex < 0 || i <= activeFloorIndex;
            floor.SetVisualsVisible(isVisible);
        }

        ApplyOverheadVisibility(activeFloor == null);
    }

    private void ApplyOverheadVisibility(bool visible)
    {
        if (overheadVisibilityGroups == null)
        {
            return;
        }

        float targetAlpha = visible ? 1f : 0f;
        for (int i = 0; i < overheadVisibilityGroups.Length; i++)
        {
            VisibilityTargetGroup targetGroup = overheadVisibilityGroups[i];
            if (targetGroup == null)
            {
                continue;
            }

            targetGroup.SetFloorVisibilityAlpha(targetAlpha, overheadTransitionDuration);
        }
    }

    private void ApplyBackOcclusionAlpha(float alpha, float duration)
    {
        for (int i = 0; i < floors.Count; i++)
        {
            FloorController floor = floors[i];
            if (floor == null)
            {
                continue;
            }

            float groundTilemapAlpha = alpha < 1f && i == 0
                ? backOcclusionGroundFloorAlpha
                : alpha;
            floor.SetBackOcclusionAlpha(alpha, groundTilemapAlpha, duration);
        }

        ApplyBackOcclusionToGroups(overheadVisibilityGroups, alpha, duration);
        ApplyBackOcclusionToGroups(backOcclusionGroups, alpha, duration);
    }

    private static void ApplyBackOcclusionToGroups(VisibilityTargetGroup[] targetGroups, float alpha, float duration)
    {
        if (targetGroups == null)
        {
            return;
        }

        for (int i = 0; i < targetGroups.Length; i++)
        {
            VisibilityTargetGroup targetGroup = targetGroups[i];
            if (targetGroup == null)
            {
                continue;
            }

            targetGroup.SetBackOcclusionAlpha(alpha, duration);
        }
    }
}
