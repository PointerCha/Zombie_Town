using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GeneratedBuildingRuntimeRegistry : MonoBehaviour
{
    [Header("Building Ownership")]
    [SerializeField] private string buildingInstanceId;
    [SerializeField] private string buildingId;
    [SerializeField] private string profileId;
    [SerializeField] private int bindingRevision;
    [SerializeField] private bool ready;

    [Header("Cached Runtime Objects")]
    [SerializeField] private GeneratedRuntimeIdentity[] identities = Array.Empty<GeneratedRuntimeIdentity>();
    [SerializeField] private Collider2D[] colliders = Array.Empty<Collider2D>();
    [SerializeField] private Collider2D[] triggerColliders = Array.Empty<Collider2D>();
    [SerializeField] private GeneratedRuntimeIdentity[] floorIdentities = Array.Empty<GeneratedRuntimeIdentity>();
    [SerializeField] private GeneratedRuntimeIdentity[] roomIdentities = Array.Empty<GeneratedRuntimeIdentity>();
    [SerializeField] private GeneratedVisibilityController visibilityController;

    public string BuildingInstanceId => buildingInstanceId;
    public string BuildingId => buildingId;
    public string ProfileId => profileId;
    public int BindingRevision => bindingRevision;
    public bool IsReady => ready;
    public IReadOnlyList<GeneratedRuntimeIdentity> Identities => identities;
    public IReadOnlyList<Collider2D> Colliders => colliders;
    public IReadOnlyList<Collider2D> TriggerColliders => triggerColliders;
    public IReadOnlyList<GeneratedRuntimeIdentity> FloorIdentities => floorIdentities;
    public IReadOnlyList<GeneratedRuntimeIdentity> RoomIdentities => roomIdentities;
    public GeneratedVisibilityController VisibilityController => visibilityController;

    public int BeginRebuild(
        string newBuildingInstanceId,
        string newBuildingId,
        string newProfileId
    )
    {
        buildingInstanceId = newBuildingInstanceId ?? string.Empty;
        buildingId = newBuildingId ?? string.Empty;
        profileId = newProfileId ?? string.Empty;
        bindingRevision = bindingRevision == int.MaxValue ? 1 : bindingRevision + 1;
        ready = false;
        ClearCaches();
        return bindingRevision;
    }

    public void CompleteRebuild(Transform ownershipRoot)
    {
        ClearCaches();

        if (ownershipRoot == null || string.IsNullOrWhiteSpace(buildingInstanceId))
        {
            return;
        }

        GeneratedRuntimeIdentity[] discovered =
            ownershipRoot.GetComponentsInChildren<GeneratedRuntimeIdentity>(true);
        List<GeneratedRuntimeIdentity> currentIdentities =
            new List<GeneratedRuntimeIdentity>(discovered.Length);
        List<Collider2D> currentColliders = new List<Collider2D>();
        List<Collider2D> currentTriggers = new List<Collider2D>();
        List<GeneratedRuntimeIdentity> currentFloors = new List<GeneratedRuntimeIdentity>();
        List<GeneratedRuntimeIdentity> currentRooms = new List<GeneratedRuntimeIdentity>();

        for (int i = 0; i < discovered.Length; i++)
        {
            GeneratedRuntimeIdentity identity = discovered[i];

            if (identity == null ||
                identity.Registry != this ||
                identity.BindingRevision != bindingRevision ||
                !string.Equals(identity.BuildingInstanceId, buildingInstanceId, StringComparison.Ordinal))
            {
                continue;
            }

            currentIdentities.Add(identity);

            if (identity.ObjectKind == GeneratedRuntimeObjectKind.Floor)
            {
                currentFloors.Add(identity);
            }
            else if (identity.ObjectKind == GeneratedRuntimeObjectKind.Room)
            {
                currentRooms.Add(identity);
            }

            Collider2D[] ownedColliders = identity.GetComponents<Collider2D>();

            for (int colliderIndex = 0; colliderIndex < ownedColliders.Length; colliderIndex++)
            {
                Collider2D collider = ownedColliders[colliderIndex];

                if (collider == null || currentColliders.Contains(collider))
                {
                    continue;
                }

                currentColliders.Add(collider);

                if (collider.isTrigger)
                {
                    currentTriggers.Add(collider);
                }
            }
        }

        identities = currentIdentities.ToArray();
        colliders = currentColliders.ToArray();
        triggerColliders = currentTriggers.ToArray();
        floorIdentities = currentFloors.ToArray();
        roomIdentities = currentRooms.ToArray();
        visibilityController = ownershipRoot.GetComponent<GeneratedVisibilityController>();
        ready = true;
    }

    public bool Owns(GeneratedRuntimeIdentity identity)
    {
        return identity != null &&
               identity.Registry == this &&
               identity.BindingRevision == bindingRevision &&
               string.Equals(identity.BuildingInstanceId, buildingInstanceId, StringComparison.Ordinal);
    }

    private void ClearCaches()
    {
        identities = Array.Empty<GeneratedRuntimeIdentity>();
        colliders = Array.Empty<Collider2D>();
        triggerColliders = Array.Empty<Collider2D>();
        floorIdentities = Array.Empty<GeneratedRuntimeIdentity>();
        roomIdentities = Array.Empty<GeneratedRuntimeIdentity>();
        visibilityController = null;
    }
}
