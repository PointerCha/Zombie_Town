using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class GeneratedBuildingContextAgent : MonoBehaviour
{
    [Header("Current Building Context")]
    [SerializeField] private GeneratedBuildingRuntimeRegistry currentRegistry;
    [SerializeField] private string currentBuildingInstanceId;

    [Header("Debug")]
    [SerializeField] private bool logContextChanges;

    private readonly HashSet<GeneratedRuntimeIdentity> activeOwnedTriggers =
        new HashSet<GeneratedRuntimeIdentity>();
    private GeneratedFloorIsolationAgent floorIsolationAgent;
    private GeneratedVisibilityAgent visibilityAgent;

    public GeneratedBuildingRuntimeRegistry CurrentRegistry => currentRegistry;
    public string CurrentBuildingInstanceId => currentBuildingInstanceId;
    public bool HasBuildingContext => currentRegistry != null && currentRegistry.IsReady;
    public int ActiveOwnedTriggerCount => activeOwnedTriggers.Count;

    private void Awake()
    {
        floorIsolationAgent = GetComponent<GeneratedFloorIsolationAgent>();
        visibilityAgent = GetComponent<GeneratedVisibilityAgent>();
    }

    private void OnDisable()
    {
        ClearBuildingContext();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        NotifyTriggerEnter(other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        NotifyTriggerExit(other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null);
    }

    public bool NotifyTriggerEnter(GeneratedRuntimeIdentity identity)
    {
        GeneratedBuildingRuntimeRegistry registry = identity != null ? identity.Registry : null;

        if (registry == null || !registry.IsReady || !registry.Owns(identity) || !identity.TriggerObject)
        {
            return false;
        }

        if (currentRegistry != null && currentRegistry != registry && activeOwnedTriggers.Count > 0)
        {
            return false;
        }

        if (currentRegistry != registry)
        {
            SetBuildingContext(registry);
        }

        return activeOwnedTriggers.Add(identity);
    }

    public bool NotifyTriggerExit(GeneratedRuntimeIdentity identity)
    {
        if (identity == null || identity.Registry != currentRegistry || !activeOwnedTriggers.Remove(identity))
        {
            return false;
        }

        if (activeOwnedTriggers.Count == 0)
        {
            ClearBuildingContext();
        }

        return true;
    }

    public void SetBuildingContext(GeneratedBuildingRuntimeRegistry registry)
    {
        if (registry == currentRegistry)
        {
            return;
        }

        GeneratedBuildingRuntimeRegistry previousRegistry = currentRegistry;
        activeOwnedTriggers.Clear();
        currentRegistry = registry != null && registry.IsReady ? registry : null;
        currentBuildingInstanceId = currentRegistry != null
            ? currentRegistry.BuildingInstanceId
            : string.Empty;

        ResolveAgents();
        floorIsolationAgent?.SetBuildingRegistry(currentRegistry);
        visibilityAgent?.SetBuildingRegistry(currentRegistry);

        if (logContextChanges)
        {
            string previousId = previousRegistry != null ? previousRegistry.BuildingInstanceId : "none";
            string currentId = currentRegistry != null ? currentRegistry.BuildingInstanceId : "none";
            Debug.Log($"{name}: building context changed {previousId} -> {currentId}.", this);
        }
    }

    public void ClearBuildingContext()
    {
        activeOwnedTriggers.Clear();

        if (currentRegistry == null && string.IsNullOrEmpty(currentBuildingInstanceId))
        {
            return;
        }

        SetBuildingContext(null);
    }

    private void ResolveAgents()
    {
        if (floorIsolationAgent == null)
        {
            floorIsolationAgent = GetComponent<GeneratedFloorIsolationAgent>();
        }

        if (visibilityAgent == null)
        {
            visibilityAgent = GetComponent<GeneratedVisibilityAgent>();
        }
    }
}
