using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class GeneratedVisibilityController : MonoBehaviour
{
    private const float VisibleAlpha = 1f;

    [Header("Fade")]
    [SerializeField, Range(0f, 1f)] private float floorInsideAlpha = 0.35f;
    [SerializeField, Range(0f, 1f)] private float roomInsideAlpha = 0.25f;
    [SerializeField, Range(0f, 1f)] private float ceilingInsideAlpha = 0.05f;
    [SerializeField, Range(0f, 1f)] private float upperFloorHiddenAlpha = 0.02f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool logVisibilityChanges;

    private readonly List<GroupState> states = new List<GroupState>();
    private readonly Dictionary<GeneratedVisibilityGroup, GroupState> stateByGroup =
        new Dictionary<GeneratedVisibilityGroup, GroupState>();
    private readonly Dictionary<string, GroupState> floorStateByKey = new Dictionary<string, GroupState>();
    private readonly Dictionary<string, GroupState> roomStateByKey = new Dictionary<string, GroupState>();
    private readonly Dictionary<int, int> currentFloorByAgentId = new Dictionary<int, int>();
    private readonly List<FloorVisualState> floorVisualStates = new List<FloorVisualState>();

    private GroupState ceilingState;

    private void Awake()
    {
        RebuildCache();
    }

    private void Update()
    {
        float maxDelta = Time.deltaTime / Mathf.Max(0.01f, fadeDuration);

        for (int i = 0; i < states.Count; i++)
        {
            states[i].UpdateAlpha(maxDelta);
        }
    }

    [ContextMenu("Rebuild Visibility Cache")]
    public void RebuildCache()
    {
        states.Clear();
        stateByGroup.Clear();
        floorStateByKey.Clear();
        roomStateByKey.Clear();
        currentFloorByAgentId.Clear();
        floorVisualStates.Clear();
        ceilingState = null;

        GeneratedVisibilityGroup[] groups = GetComponentsInChildren<GeneratedVisibilityGroup>(true);

        for (int i = 0; i < groups.Length; i++)
        {
            GeneratedVisibilityGroup group = groups[i];

            if (group == null)
            {
                continue;
            }

            GroupState state = new GroupState(group);
            states.Add(state);
            stateByGroup[group] = state;

            switch (group.GroupKind)
            {
                case GeneratedVisibilityGroupKind.Floor:
                    floorStateByKey[BuildFloorKey(group.FloorIndex)] = state;
                    CacheFloorVisualState(group);
                    break;
                case GeneratedVisibilityGroupKind.Room:
                    roomStateByKey[BuildRoomKey(group.FloorIndex, group.RoomIndex)] = state;
                    break;
                case GeneratedVisibilityGroupKind.Ceiling:
                    ceilingState = state;
                    break;
            }

            state.SetImmediate(VisibleAlpha);
        }

        ShowAllFloorVisuals();
    }

    public void RegisterTrigger(GeneratedVisibilityAgent agent, GeneratedRuntimeIdentity identity)
    {
        if (agent == null || identity == null)
        {
            return;
        }

        if (identity.ObjectKind == GeneratedRuntimeObjectKind.FloorGroundTrigger)
        {
            SetCurrentFloorVisuals(agent, identity.FloorIndex);
            RegisterOccupant(floorStateByKey, BuildFloorKey(identity.FloorIndex), agent);

            if (ceilingState != null)
            {
                RegisterOccupant(ceilingState, agent);
            }
        }
        else if (identity.ObjectKind == GeneratedRuntimeObjectKind.RoomGroundTrigger)
        {
            RegisterOccupant(roomStateByKey, BuildRoomKey(identity.FloorIndex, identity.RoomIndex), agent);
        }
    }

    public void UnregisterTrigger(GeneratedVisibilityAgent agent, GeneratedRuntimeIdentity identity)
    {
        if (agent == null || identity == null)
        {
            return;
        }

        if (identity.ObjectKind == GeneratedRuntimeObjectKind.FloorGroundTrigger)
        {
            UnregisterOccupant(floorStateByKey, BuildFloorKey(identity.FloorIndex), agent);

            if (ceilingState != null)
            {
                UnregisterOccupant(ceilingState, agent);
            }

            if (!HasAnyFloorOccupant() && !HasAnyFloorContext())
            {
                ShowAllFloorVisuals();
            }
        }
        else if (identity.ObjectKind == GeneratedRuntimeObjectKind.RoomGroundTrigger)
        {
            UnregisterOccupant(roomStateByKey, BuildRoomKey(identity.FloorIndex, identity.RoomIndex), agent);
        }
    }

    public void UnregisterAgent(GeneratedVisibilityAgent agent)
    {
        if (agent == null)
        {
            return;
        }

        for (int i = 0; i < states.Count; i++)
        {
            UnregisterOccupant(states[i], agent);
        }

        currentFloorByAgentId.Remove(agent.GetInstanceID());

        if (!HasAnyFloorOccupant() && !HasAnyFloorContext())
        {
            ShowAllFloorVisuals();
        }
    }

    public void SetCurrentFloorVisuals(GeneratedVisibilityAgent agent, int currentFloorIndex)
    {
        if (agent != null)
        {
            currentFloorByAgentId[agent.GetInstanceID()] = currentFloorIndex;
        }

        SetCurrentFloorVisuals(currentFloorIndex);
        RefreshContextualFadeTargets();
    }

    public void SetCurrentFloorVisuals(int currentFloorIndex)
    {
        for (int i = 0; i < floorVisualStates.Count; i++)
        {
            FloorVisualState state = floorVisualStates[i];
            float alpha = state.FloorIndex > currentFloorIndex ? upperFloorHiddenAlpha : VisibleAlpha;
            state.SetImmediate(alpha);
        }

        if (ceilingState != null)
        {
            ceilingState.SetTarget(ceilingInsideAlpha);
        }

        RefreshContextualFadeTargets();
    }

    public void ClearCurrentFloorVisuals(GeneratedVisibilityAgent agent)
    {
        if (agent == null)
        {
            return;
        }

        currentFloorByAgentId.Remove(agent.GetInstanceID());
        RefreshContextualFadeTargets();

        if (!HasAnyFloorOccupant() && !HasAnyFloorContext())
        {
            ShowAllFloorVisuals();
        }
    }

    public void ShowAllFloorVisuals()
    {
        for (int i = 0; i < floorVisualStates.Count; i++)
        {
            floorVisualStates[i].SetImmediate(VisibleAlpha);
        }

        if (ceilingState != null)
        {
            ceilingState.SetTarget(VisibleAlpha);
        }
    }

    private void RegisterOccupant(
        Dictionary<string, GroupState> stateMap,
        string key,
        GeneratedVisibilityAgent agent
    )
    {
        if (!stateMap.TryGetValue(key, out GroupState state))
        {
            return;
        }

        RegisterOccupant(state, agent);
    }

    private void RegisterOccupant(GroupState state, GeneratedVisibilityAgent agent)
    {
        if (state == null || !state.AddOccupant(agent.GetInstanceID()))
        {
            return;
        }

        RefreshContextualFadeTargets();

        if (logVisibilityChanges)
        {
            Debug.Log($"{name}: visibility occupant entered {state.Group.GroupId}", this);
        }
    }

    private void UnregisterOccupant(
        Dictionary<string, GroupState> stateMap,
        string key,
        GeneratedVisibilityAgent agent
    )
    {
        if (!stateMap.TryGetValue(key, out GroupState state))
        {
            return;
        }

        UnregisterOccupant(state, agent);
    }

    private void UnregisterOccupant(GroupState state, GeneratedVisibilityAgent agent)
    {
        if (state == null || !state.RemoveOccupant(agent.GetInstanceID()))
        {
            return;
        }

        if (state.OccupantCount <= 0)
        {
            RefreshContextualFadeTargets();

            if (logVisibilityChanges)
            {
                Debug.Log($"{name}: visibility occupant exited {state.Group.GroupId}", this);
            }
        }
    }

    private void RefreshContextualFadeTargets()
    {
        int highestActiveFloorIndex = GetHighestActiveFloorIndex();
        bool hasActiveFloor = highestActiveFloorIndex > 0;

        foreach (KeyValuePair<string, GroupState> pair in floorStateByKey)
        {
            GroupState state = pair.Value;

            if (state == null)
            {
                continue;
            }

            bool upperFloorHidden = hasActiveFloor && state.Group.FloorIndex > highestActiveFloorIndex;
            bool shouldFade = state.OccupantCount > 0 || HasFloorContext(state.Group.FloorIndex);
            float targetAlpha = upperFloorHidden
                ? upperFloorHiddenAlpha
                : shouldFade
                    ? floorInsideAlpha
                    : VisibleAlpha;
            state.SetTarget(targetAlpha);
            state.ReapplyIfAtTarget();
        }

        foreach (KeyValuePair<string, GroupState> pair in roomStateByKey)
        {
            GroupState state = pair.Value;

            if (state == null)
            {
                continue;
            }

            bool upperFloorHidden = hasActiveFloor && state.Group.FloorIndex > highestActiveFloorIndex;
            bool lowerFloorWallHidden = hasActiveFloor &&
                                        state.Group.FloorIndex < highestActiveFloorIndex &&
                                        state.OccupantCount <= 0;
            float targetAlpha = upperFloorHidden || lowerFloorWallHidden
                ? upperFloorHiddenAlpha
                : state.OccupantCount > 0
                    ? roomInsideAlpha
                    : VisibleAlpha;
            state.SetTarget(targetAlpha);
            state.ReapplyIfAtTarget();
        }

        if (ceilingState != null)
        {
            bool shouldFadeCeiling = ceilingState.OccupantCount > 0 || HasAnyFloorContext();
            ceilingState.SetTarget(shouldFadeCeiling ? ceilingInsideAlpha : VisibleAlpha);
            ceilingState.ReapplyIfAtTarget();
        }
    }

    private bool HasFloorContext(int floorIndex)
    {
        foreach (KeyValuePair<int, int> pair in currentFloorByAgentId)
        {
            if (pair.Value == floorIndex)
            {
                return true;
            }
        }

        return false;
    }

    private int GetHighestActiveFloorIndex()
    {
        int highestFloorIndex = 0;

        foreach (KeyValuePair<int, int> pair in currentFloorByAgentId)
        {
            highestFloorIndex = Mathf.Max(highestFloorIndex, pair.Value);
        }

        foreach (KeyValuePair<string, GroupState> pair in floorStateByKey)
        {
            GroupState state = pair.Value;

            if (state != null && state.OccupantCount > 0)
            {
                highestFloorIndex = Mathf.Max(highestFloorIndex, state.Group.FloorIndex);
            }
        }

        return highestFloorIndex;
    }

    private bool HasAnyFloorOccupant()
    {
        foreach (KeyValuePair<string, GroupState> pair in floorStateByKey)
        {
            if (pair.Value != null && pair.Value.OccupantCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAnyFloorContext()
    {
        return currentFloorByAgentId.Count > 0;
    }

    private void CacheFloorVisualState(GeneratedVisibilityGroup floorGroup)
    {
        if (floorGroup == null)
        {
            return;
        }

        FloorVisualState state = new FloorVisualState(floorGroup.FloorIndex);
        Transform floorRoot = floorGroup.transform;
        state.AddTilemap(floorRoot.Find($"Floor {floorGroup.FloorIndex:00} Visual Tilemaps/Ground"));
        state.AddRoomGroundTilemaps(floorRoot.Find($"Floor {floorGroup.FloorIndex:00} Room Tilemaps"));

        if (state.TargetCount > 0)
        {
            floorVisualStates.Add(state);
        }
    }

    private static string BuildFloorKey(int floorIndex)
    {
        return floorIndex.ToString();
    }

    private static string BuildRoomKey(int floorIndex, int roomIndex)
    {
        return $"{floorIndex}:{roomIndex}";
    }

    private sealed class GroupState
    {
        private readonly List<Tilemap> targets = new List<Tilemap>();
        private readonly HashSet<int> occupants = new HashSet<int>();
        private float currentAlpha = VisibleAlpha;
        private float targetAlpha = VisibleAlpha;

        public GeneratedVisibilityGroup Group { get; }
        public int OccupantCount => occupants.Count;

        public GroupState(GeneratedVisibilityGroup group)
        {
            Group = group;

            for (int i = 0; i < group.Targets.Count; i++)
            {
                TilemapRenderer renderer = group.Targets[i];
                Tilemap tilemap = renderer != null ? renderer.GetComponent<Tilemap>() : null;

                if (tilemap != null && !targets.Contains(tilemap))
                {
                    targets.Add(tilemap);
                }
            }
        }

        public bool AddOccupant(int occupantId)
        {
            return occupants.Add(occupantId);
        }

        public bool RemoveOccupant(int occupantId)
        {
            return occupants.Remove(occupantId);
        }

        public void SetTarget(float alpha)
        {
            targetAlpha = Mathf.Clamp01(alpha);
        }

        public void SetImmediate(float alpha)
        {
            currentAlpha = Mathf.Clamp01(alpha);
            targetAlpha = currentAlpha;
            ApplyAlpha(currentAlpha);
        }

        public void ReapplyIfAtTarget()
        {
            if (Mathf.Approximately(currentAlpha, targetAlpha))
            {
                ApplyAlpha(currentAlpha);
            }
        }

        public void UpdateAlpha(float maxDelta)
        {
            if (Mathf.Approximately(currentAlpha, targetAlpha))
            {
                return;
            }

            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, maxDelta);
            ApplyAlpha(currentAlpha);
        }

        private void ApplyAlpha(float alpha)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Tilemap tilemap = targets[i];

                if (tilemap == null)
                {
                    continue;
                }

                Color color = tilemap.color;
                color.a = alpha;
                tilemap.color = color;
            }
        }
    }

    private sealed class FloorVisualState
    {
        private readonly List<Tilemap> targets = new List<Tilemap>();

        public int FloorIndex { get; }
        public int TargetCount => targets.Count;

        public FloorVisualState(int floorIndex)
        {
            FloorIndex = floorIndex;
        }

        public void AddTilemap(Transform target)
        {
            if (target == null)
            {
                return;
            }

            Tilemap tilemap = target.GetComponent<Tilemap>();

            if (tilemap != null && !targets.Contains(tilemap))
            {
                targets.Add(tilemap);
            }
        }

        public void AddRoomGroundTilemaps(Transform roomRoot)
        {
            if (roomRoot == null)
            {
                return;
            }

            for (int i = 0; i < roomRoot.childCount; i++)
            {
                Transform room = roomRoot.GetChild(i);
                AddTilemap(room != null ? room.Find("Ground") : null);
            }
        }

        public void SetImmediate(float alpha)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Tilemap tilemap = targets[i];

                if (tilemap == null)
                {
                    continue;
                }

                Color color = tilemap.color;
                color.a = alpha;
                tilemap.color = color;
            }
        }
    }
}
