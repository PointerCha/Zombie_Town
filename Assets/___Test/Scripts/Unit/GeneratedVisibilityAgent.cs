using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class GeneratedVisibilityAgent : MonoBehaviour
{
    [SerializeField] private GeneratedFloorIsolationAgent floorIsolationAgent;
    [SerializeField] private bool logVisibilityTriggers;

    private readonly HashSet<GeneratedRuntimeIdentity> activeIdentities = new HashSet<GeneratedRuntimeIdentity>();
    private GeneratedVisibilityController visibilityController;
    private GeneratedBuildingRuntimeRegistry buildingRegistry;
    private GeneratedBuildingContextAgent buildingContextAgent;

    public GeneratedBuildingRuntimeRegistry BuildingRegistry => buildingRegistry;

    private void Awake()
    {
        if (floorIsolationAgent == null)
        {
            floorIsolationAgent = GetComponent<GeneratedFloorIsolationAgent>();
        }

        buildingContextAgent = GetComponent<GeneratedBuildingContextAgent>();
    }

    private void OnDisable()
    {
        if (visibilityController != null)
        {
            visibilityController.UnregisterAgent(this);
            visibilityController.ClearCurrentFloorVisuals(this);
        }

        activeIdentities.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessTriggerEnter(other);
    }

    public void ProcessTriggerEnter(Collider2D other)
    {
        ResolveBuildingContextAgent();
        GeneratedRuntimeIdentity identity = other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null;
        buildingContextAgent?.NotifyTriggerEnter(identity);

        if (!CanUseIdentity(identity) || !activeIdentities.Add(identity))
        {
            return;
        }

        GeneratedVisibilityController controller = ResolveController(identity);

        if (controller == null)
        {
            return;
        }

        controller.RegisterTrigger(this, identity);

        if (logVisibilityTriggers)
        {
            Debug.Log($"{name}: visibility enter {identity.ObjectKind} floor={identity.FloorIndex} room={identity.RoomIndex}", this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ProcessTriggerExit(other);
    }

    public void ProcessTriggerExit(Collider2D other)
    {
        ResolveBuildingContextAgent();
        GeneratedRuntimeIdentity identity = other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null;

        if (identity == null || !activeIdentities.Remove(identity))
        {
            buildingContextAgent?.NotifyTriggerExit(identity);
            return;
        }

        GeneratedVisibilityController controller = ResolveController(identity);

        if (controller == null)
        {
            buildingContextAgent?.NotifyTriggerExit(identity);
            return;
        }

        controller.UnregisterTrigger(this, identity);
        buildingContextAgent?.NotifyTriggerExit(identity);

        if (logVisibilityTriggers)
        {
            Debug.Log($"{name}: visibility exit {identity.ObjectKind} floor={identity.FloorIndex} room={identity.RoomIndex}", this);
        }
    }

    public void ClearVisibilityRegistrations()
    {
        if (visibilityController != null)
        {
            visibilityController.UnregisterAgent(this);
        }

        activeIdentities.Clear();
    }

    public void SetBuildingRegistry(GeneratedBuildingRuntimeRegistry newRegistry)
    {
        if (buildingRegistry == newRegistry)
        {
            return;
        }

        if (visibilityController != null)
        {
            visibilityController.UnregisterAgent(this);
            visibilityController.ClearCurrentFloorVisuals(this);
        }

        activeIdentities.Clear();
        buildingRegistry = newRegistry != null && newRegistry.IsReady ? newRegistry : null;
        visibilityController = buildingRegistry != null
            ? buildingRegistry.VisibilityController
            : null;
    }

    private void ResolveBuildingContextAgent()
    {
        if (buildingContextAgent == null)
        {
            buildingContextAgent = GetComponent<GeneratedBuildingContextAgent>();
        }
    }

    public void ApplyCurrentFloorVisuals(int currentFloorIndex)
    {
        GeneratedVisibilityController controller = ResolveController(null);

        if (controller != null)
        {
            controller.SetCurrentFloorVisuals(this, currentFloorIndex);
        }
    }

    public void ClearCurrentFloorVisuals()
    {
        GeneratedVisibilityController controller = ResolveController(null);

        if (controller != null)
        {
            controller.ClearCurrentFloorVisuals(this);
        }
    }

    private bool CanUseIdentity(GeneratedRuntimeIdentity identity)
    {
        if (identity == null || !identity.TriggerObject)
        {
            return false;
        }

        if (buildingRegistry == null || !buildingRegistry.Owns(identity))
        {
            return false;
        }

        if (floorIsolationAgent != null && !floorIsolationAgent.IsIdentityActive(identity))
        {
            return false;
        }

        return identity.ObjectKind == GeneratedRuntimeObjectKind.FloorGroundTrigger ||
               identity.ObjectKind == GeneratedRuntimeObjectKind.RoomGroundTrigger;
    }

    private GeneratedVisibilityController ResolveController(GeneratedRuntimeIdentity identity)
    {
        if (buildingRegistry == null ||
            (identity != null && !buildingRegistry.Owns(identity)))
        {
            return null;
        }

        return visibilityController;
    }
}
