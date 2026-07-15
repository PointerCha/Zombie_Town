using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class GeneratedStairTransitionAgent : MonoBehaviour
{
    [SerializeField] private GeneratedTestUnitController unitController;
    [SerializeField, Min(0f)] private float completedReentryBlockDuration = 0.35f;
    [SerializeField] private bool logTransitions = true;

    private readonly Dictionary<string, StairTraversalState> traversalByStair =
        new Dictionary<string, StairTraversalState>();
    private GeneratedBuildingContextAgent buildingContextAgent;

    private void Awake()
    {
        if (unitController == null)
        {
            unitController = GetComponent<GeneratedTestUnitController>();
        }

        buildingContextAgent = GetComponent<GeneratedBuildingContextAgent>();
    }

    private void OnDisable()
    {
        traversalByStair.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessTriggerEnter(other);
    }

    public void ProcessTriggerEnter(Collider2D other)
    {
        GeneratedRuntimeIdentity identity = other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null;
        buildingContextAgent?.NotifyTriggerEnter(identity);
        GeneratedStairZone zone = other != null ? other.GetComponent<GeneratedStairZone>() : null;

        if (zone == null)
        {
            return;
        }

        StairTraversalState state = GetOrCreateState(zone);

        if (!zone.IsTransitionZone)
        {
            return;
        }

        if (state.BlockedZone == zone.ZoneKind && Time.time < state.BlockedUntil)
        {
            return;
        }

        if (state.Locked)
        {
            ResolveActiveStair(zone, state);
            return;
        }

        BeginStairTransition(zone, state);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ProcessTriggerExit(other);
    }

    public void ProcessTriggerExit(Collider2D other)
    {
        GeneratedRuntimeIdentity identity = other != null ? other.GetComponent<GeneratedRuntimeIdentity>() : null;
        buildingContextAgent?.NotifyTriggerExit(identity);
    }

    private void BeginStairTransition(GeneratedStairZone zone, StairTraversalState state)
    {
        int currentFloorIndex = unitController != null ? unitController.CurrentFloorIndex : 1;

        if (currentFloorIndex != zone.SourceFloorIndex || zone.DestinationFloorIndex <= 0)
        {
            return;
        }

        state.Locked = true;
        state.OriginZone = zone.ZoneKind;
        state.TargetZone = GetOppositeTransitionZone(zone.ZoneKind);
        state.SourceFloorIndex = zone.SourceFloorIndex;
        state.DestinationFloorIndex = zone.DestinationFloorIndex;
        unitController.SetCurrentFloorIndex(zone.DestinationFloorIndex);

        if (logTransitions)
        {
            Debug.Log(
                $"{name}: stair transition {zone.StairId} {zone.SourceFloorIndex} -> {zone.DestinationFloorIndex} by {zone.ZoneKind}",
                this
            );
        }
    }

    private void ResolveActiveStair(GeneratedStairZone zone, StairTraversalState state)
    {
        if (zone.ZoneKind == state.OriginZone)
        {
            int sourceFloorIndex = state.SourceFloorIndex;
            int destinationFloorIndex = state.DestinationFloorIndex;

            if (unitController != null && unitController.CurrentFloorIndex == state.DestinationFloorIndex)
            {
                unitController.SetCurrentFloorIndex(state.SourceFloorIndex);
            }

            BlockReentry(state, zone.ZoneKind);
            state.ResetActive();

            if (logTransitions)
            {
                Debug.Log(
                    $"{name}: stair return {zone.StairId} {destinationFloorIndex} -> {sourceFloorIndex}",
                    this
                );
            }

            return;
        }

        if (zone.ZoneKind == state.TargetZone)
        {
            BlockReentry(state, zone.ZoneKind);
            state.ResetActive();
        }
    }

    private void BlockReentry(StairTraversalState state, GeneratedStairZoneKind zoneKind)
    {
        state.BlockedZone = zoneKind;
        state.BlockedUntil = Time.time + completedReentryBlockDuration;
    }

    private static GeneratedStairZoneKind GetOppositeTransitionZone(GeneratedStairZoneKind zoneKind)
    {
        if (zoneKind == GeneratedStairZoneKind.Entry)
        {
            return GeneratedStairZoneKind.Exit;
        }

        return GeneratedStairZoneKind.Entry;
    }

    private StairTraversalState GetOrCreateState(GeneratedStairZone zone)
    {
        string key = string.IsNullOrEmpty(zone.StairId) ? zone.name : zone.StairId;

        if (!traversalByStair.TryGetValue(key, out StairTraversalState state))
        {
            state = new StairTraversalState();
            traversalByStair.Add(key, state);
        }

        return state;
    }

    private sealed class StairTraversalState
    {
        public bool Locked;
        public GeneratedStairZoneKind OriginZone;
        public GeneratedStairZoneKind TargetZone;
        public GeneratedStairZoneKind BlockedZone;
        public float BlockedUntil;
        public int SourceFloorIndex;
        public int DestinationFloorIndex;

        public void ResetActive()
        {
            Locked = false;
            OriginZone = GeneratedStairZoneKind.Walkable;
            TargetZone = GeneratedStairZoneKind.Walkable;
            SourceFloorIndex = 0;
            DestinationFloorIndex = 0;
        }
    }
}
