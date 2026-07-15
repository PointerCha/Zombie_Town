using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class GeneratedFloorIsolationAgent : MonoBehaviour
{
    [Header("Floor Isolation")]
    [SerializeField, Min(1)] private int currentFloorIndex = 1;
    [SerializeField] private bool ignoreOtherFloorColliders = true;
    [SerializeField] private bool refreshOnEnable = true;

    [Header("Debug")]
    [SerializeField] private bool logRefreshSummary;

    private Collider2D bodyCollider;
    private GeneratedBuildingRuntimeRegistry buildingRegistry;
    private readonly List<Collider2D> configuredColliders = new List<Collider2D>();

    public int CurrentFloorIndex => currentFloorIndex;
    public bool IgnoreOtherFloorColliders => ignoreOtherFloorColliders;
    public GeneratedBuildingRuntimeRegistry BuildingRegistry => buildingRegistry;

    private void Awake()
    {
        bodyCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (refreshOnEnable)
        {
            RefreshFloorCollisionIsolation();
        }
    }

    private void OnDisable()
    {
        RestoreConfiguredCollisions();
    }

    public void SetBuildingRegistry(GeneratedBuildingRuntimeRegistry newRegistry)
    {
        if (buildingRegistry == newRegistry)
        {
            RefreshFloorCollisionIsolation();
            return;
        }

        RestoreConfiguredCollisions();
        buildingRegistry = newRegistry != null && newRegistry.IsReady ? newRegistry : null;
        RefreshFloorCollisionIsolation();
    }

    public void SetCurrentFloorIndex(int newFloorIndex)
    {
        int clampedFloorIndex = Mathf.Max(1, newFloorIndex);

        if (currentFloorIndex == clampedFloorIndex)
        {
            RefreshFloorCollisionIsolation();
            return;
        }

        currentFloorIndex = clampedFloorIndex;
        RefreshFloorCollisionIsolation();
    }

    public bool IsColliderActive(Collider2D targetCollider)
    {
        if (!ignoreOtherFloorColliders || targetCollider == null)
        {
            return true;
        }

        return IsIdentityActive(targetCollider.GetComponent<GeneratedRuntimeIdentity>());
    }

    public bool IsIdentityActive(GeneratedRuntimeIdentity identity)
    {
        return !ignoreOtherFloorColliders ||
               identity == null ||
               buildingRegistry == null ||
               !buildingRegistry.Owns(identity) ||
               identity.FloorIndex <= 0 ||
               identity.FloorIndex == currentFloorIndex;
    }

    [ContextMenu("Refresh Floor Collision Isolation")]
    public void RefreshFloorCollisionIsolation()
    {
        if (!ignoreOtherFloorColliders)
        {
            RestoreGeneratedCollisions();
            return;
        }

        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider2D>();
        }

        if (bodyCollider == null)
        {
            return;
        }

        RestoreConfiguredCollisions();

        if (buildingRegistry == null || !buildingRegistry.IsReady)
        {
            return;
        }

        int ignoredCount = 0;
        int activeCount = 0;

        for (int i = 0; i < buildingRegistry.Colliders.Count; i++)
        {
            Collider2D targetCollider = buildingRegistry.Colliders[i];

            if (targetCollider == null || targetCollider == bodyCollider)
            {
                continue;
            }

            GeneratedRuntimeIdentity identity = targetCollider.GetComponent<GeneratedRuntimeIdentity>();
            bool ignoreCollision = !IsIdentityActive(identity);
            Physics2D.IgnoreCollision(bodyCollider, targetCollider, ignoreCollision);
            configuredColliders.Add(targetCollider);

            if (ignoreCollision)
            {
                ignoredCount++;
            }
            else
            {
                activeCount++;
            }
        }

        if (logRefreshSummary)
        {
            Debug.Log(
                $"{name}: floor isolation refreshed. currentFloor={currentFloorIndex}, active={activeCount}, ignored={ignoredCount}",
                this
            );
        }
    }

    private void RestoreGeneratedCollisions()
    {
        RestoreConfiguredCollisions();
    }

    private void RestoreConfiguredCollisions()
    {
        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider2D>();
        }

        if (bodyCollider != null)
        {
            for (int i = 0; i < configuredColliders.Count; i++)
            {
                Collider2D targetCollider = configuredColliders[i];

                if (targetCollider != null && targetCollider != bodyCollider)
                {
                    Physics2D.IgnoreCollision(bodyCollider, targetCollider, false);
                }
            }
        }

        configuredColliders.Clear();
    }
}
