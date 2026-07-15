using System;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GeneratedBuildingSceneRoot : MonoBehaviour
{
    [Header("Instance Ownership")]
    [SerializeField] private string buildingInstanceId;

    [Header("Profile")]
    [SerializeField] private GeneratedBuildingProfile profile;

    [Header("Root Objects")]
    [SerializeField] private Transform generatedHouseRoot;
    [SerializeField] private Transform floorsRoot;
    [SerializeField] private Transform floor01Root;
    [SerializeField] private Transform floor02Root;
    [SerializeField] private Transform stairRoot;
    [SerializeField] private Transform ceilingRoot;
    [SerializeField] private Transform backOcclusionRoot;

    public string BuildingInstanceId => buildingInstanceId;
    public GeneratedBuildingProfile Profile => profile;
    public Transform GeneratedHouseRoot => generatedHouseRoot;
    public Transform FloorsRoot => floorsRoot;
    public Transform Floor01Root => floor01Root;
    public Transform Floor02Root => floor02Root;
    public Transform StairRoot => stairRoot;
    public Transform CeilingRoot => ceilingRoot;
    public Transform BackOcclusionRoot => backOcclusionRoot;

    public void SetProfile(GeneratedBuildingProfile newProfile)
    {
        profile = newProfile;
    }

    public string EnsureBuildingInstanceId()
    {
        if (string.IsNullOrWhiteSpace(buildingInstanceId) || IsInstanceIdUsedByAnotherRoot(buildingInstanceId))
        {
            buildingInstanceId = Guid.NewGuid().ToString("N");
        }

        return buildingInstanceId;
    }

    private bool IsInstanceIdUsedByAnotherRoot(string instanceId)
    {
        GeneratedBuildingSceneRoot[] sceneRoots = FindObjectsOfType<GeneratedBuildingSceneRoot>(true);

        for (int i = 0; i < sceneRoots.Length; i++)
        {
            GeneratedBuildingSceneRoot other = sceneRoots[i];

            if (other != null &&
                other != this &&
                string.Equals(other.buildingInstanceId, instanceId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public bool SetBuildingInstanceId(string newInstanceId)
    {
        if (string.IsNullOrWhiteSpace(newInstanceId))
        {
            return false;
        }

        buildingInstanceId = newInstanceId.Trim();
        return true;
    }

    [ContextMenu("Regenerate Building Instance Id")]
    public void RegenerateBuildingInstanceId()
    {
        buildingInstanceId = Guid.NewGuid().ToString("N");
        Debug.Log(
            $"{name}: building instance id regenerated. Regenerate the building to rebind runtime ownership. id={buildingInstanceId}",
            this
        );
    }

    public Transform GetFloorRoot(int floorIndex)
    {
        if (floorIndex <= 0)
        {
            return null;
        }

        if (floorIndex == 1 && floor01Root != null)
        {
            return floor01Root;
        }

        if (floorIndex == 2 && floor02Root != null)
        {
            return floor02Root;
        }

        return floorsRoot != null ? floorsRoot.Find(GetFloorRootName(floorIndex)) : null;
    }

    public Transform EnsureFloorRoot(int floorIndex)
    {
        if (floorIndex <= 0 || floorsRoot == null)
        {
            return null;
        }

        Transform floorRoot = GetFloorRoot(floorIndex);

        if (floorRoot == null)
        {
            floorRoot = GeneratedTilemapUtility.EnsureChild(floorsRoot, GetFloorRootName(floorIndex));
        }

        if (floorIndex == 1 && floor01Root == null)
        {
            floor01Root = floorRoot;
        }
        else if (floorIndex == 2 && floor02Root == null)
        {
            floor02Root = floorRoot;
        }

        return floorRoot;
    }

    public Transform EnsureFloorVisualRoot(int floorIndex)
    {
        Transform floorRoot = EnsureFloorRoot(floorIndex);
        return floorRoot != null
            ? GeneratedTilemapUtility.EnsureChild(floorRoot, GetFloorVisualRootName(floorIndex))
            : null;
    }

    public Transform EnsureFloorRoomRoot(int floorIndex)
    {
        Transform floorRoot = EnsureFloorRoot(floorIndex);
        return floorRoot != null
            ? GeneratedTilemapUtility.EnsureChild(floorRoot, GetFloorRoomRootName(floorIndex))
            : null;
    }

    public Transform EnsureFloorVisualTilemap(int floorIndex, string tilemapName)
    {
        Transform visualRoot = EnsureFloorVisualRoot(floorIndex);
        return visualRoot != null ? GeneratedTilemapUtility.EnsureChild(visualRoot, tilemapName) : null;
    }

    public Transform FindFloorVisualTilemap(int floorIndex, string tilemapName)
    {
        Transform floorRoot = GetFloorRoot(floorIndex);
        Transform visualRoot = floorRoot != null ? floorRoot.Find(GetFloorVisualRootName(floorIndex)) : null;
        return visualRoot != null ? visualRoot.Find(tilemapName) : null;
    }

    public void ApplyFloorLocalPositions(GeneratedBuildingProfile sourceProfile)
    {
        if (sourceProfile == null)
        {
            return;
        }

        for (int i = 0; i < sourceProfile.FloorCount; i++)
        {
            Transform floorRoot = EnsureFloorRoot(i + 1);

            if (floorRoot != null)
            {
                floorRoot.localPosition = sourceProfile.GetFloorLocalPosition(i);
            }
        }
    }

    public void ClearUnusedFloorTilemaps(int activeFloorCount)
    {
        if (floorsRoot == null)
        {
            return;
        }

        for (int i = floorsRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = floorsRoot.GetChild(i);

            if (child == null || !TryParseFloorIndex(child.name, out int floorIndex))
            {
                continue;
            }

            if (floorIndex > activeFloorCount)
            {
                if (child == floor01Root)
                {
                    floor01Root = null;
                }

                if (child == floor02Root)
                {
                    floor02Root = null;
                }

                GeneratedTilemapUtility.DestroyGeneratedChild(child);
            }
        }
    }

    public bool IsValid(out string errorMessage)
    {
        StringBuilder builder = new StringBuilder();

        AppendMissing(builder, profile == null, "Profile is missing.");
        AppendMissing(builder, generatedHouseRoot == null, "Generated House Root is missing.");
        AppendMissing(builder, floorsRoot == null, "Floors Root is missing.");
        AppendMissing(builder, stairRoot == null, "Stair Root is missing.");
        AppendMissing(builder, ceilingRoot == null, "Ceiling Root is missing.");
        AppendMissing(builder, backOcclusionRoot == null, "Back Occlusion Root is missing.");

        if (profile != null && !profile.IsValid(out string profileError))
        {
            AppendMissing(builder, true, profileError);
        }

        errorMessage = builder.ToString();
        return errorMessage.Length == 0;
    }

    [ContextMenu("Log Scene Root Summary")]
    public void LogSummary()
    {
        if (!IsValid(out string errorMessage))
        {
            Debug.LogWarning($"{name}: scene root binding is invalid. {errorMessage}", this);
            return;
        }

        Debug.Log(
            $"{name}: scene root binding is valid.\n" +
            $"- Instance Id: {EnsureBuildingInstanceId()}\n" +
            $"- Profile: {profile.DisplayName}\n" +
            $"- Footprint: {profile.FootprintSize}\n" +
            $"- Floors: {profile.FloorCount}\n" +
            $"- Generated House Root: {generatedHouseRoot.name}",
            this
        );
    }

    [ContextMenu("Log Runtime Identity Summary")]
    public void LogRuntimeIdentitySummary()
    {
        if (generatedHouseRoot == null)
        {
            Debug.LogWarning($"{name}: Generated House Root is missing.", this);
            return;
        }

        GeneratedRuntimeIdentity[] identities = generatedHouseRoot.GetComponentsInChildren<GeneratedRuntimeIdentity>(true);
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"{name}: runtime identity summary");
        builder.AppendLine($"- Total: {identities.Length}");

        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.Building);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.Floor);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.Room);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.FloorGroundTrigger);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.FloorUpperWallCollider);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.FloorUnderWallCollider);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.RoomGroundTrigger);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.RoomUpperWallCollider);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.RoomUnderWallCollider);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.Ceiling);
        AppendIdentityCount(builder, identities, GeneratedRuntimeObjectKind.Stair);

        Debug.Log(builder.ToString(), this);
    }

    [ContextMenu("Validate Runtime Scene")]
    public void ValidateRuntimeScene()
    {
        GeneratedBuildingValidationResult result = GeneratedRuntimeSceneValidator.Validate(this, profile);
        result.Log(this);
    }

    [ContextMenu("Create Or Reset Test Unit")]
    public void CreateOrResetTestUnit()
    {
        GameObject unitObject = GameObject.Find("Generated Test Unit");

        if (unitObject == null)
        {
            unitObject = new GameObject("Generated Test Unit");
        }

        unitObject.transform.position = GetDefaultUnitSpawnPosition();
        unitObject.transform.rotation = Quaternion.identity;
        unitObject.transform.localScale = Vector3.one;

        Rigidbody2D body = unitObject.GetComponent<Rigidbody2D>();

        if (body == null)
        {
            body = unitObject.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D collider = unitObject.GetComponent<CircleCollider2D>();

        if (collider == null)
        {
            collider = unitObject.AddComponent<CircleCollider2D>();
        }

        collider.radius = 0.25f;
        collider.offset = Vector2.zero;
        collider.isTrigger = false;

        GeneratedFloorIsolationAgent floorIsolationAgent = unitObject.GetComponent<GeneratedFloorIsolationAgent>();

        if (floorIsolationAgent == null)
        {
            floorIsolationAgent = unitObject.AddComponent<GeneratedFloorIsolationAgent>();
        }

        floorIsolationAgent.SetCurrentFloorIndex(1);

        GeneratedTestUnitController unit = unitObject.GetComponent<GeneratedTestUnitController>();

        if (unit == null)
        {
            unit = unitObject.AddComponent<GeneratedTestUnitController>();
        }

        unit.SetSelected(true);

        if (unitObject.GetComponent<GeneratedVisibilityAgent>() == null)
        {
            unitObject.AddComponent<GeneratedVisibilityAgent>();
        }

        if (unitObject.GetComponent<GeneratedBuildingContextAgent>() == null)
        {
            unitObject.AddComponent<GeneratedBuildingContextAgent>();
        }

        if (unitObject.GetComponent<GeneratedStairTransitionAgent>() == null)
        {
            unitObject.AddComponent<GeneratedStairTransitionAgent>();
        }

        if (unitObject.GetComponent<GeneratedTestUnitGizmos>() == null)
        {
            unitObject.AddComponent<GeneratedTestUnitGizmos>();
        }

        BindTestCamera(unitObject.transform);

        Debug.Log($"{name}: test Unit is ready at {unitObject.transform.position}. Right-click to move it.", unitObject);
    }

    private static void BindTestCamera(Transform target)
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            camera = FindObjectOfType<Camera>();
        }

        if (camera == null)
        {
            return;
        }

        GeneratedTestCameraController cameraController = camera.GetComponent<GeneratedTestCameraController>();

        if (cameraController == null)
        {
            cameraController = camera.gameObject.AddComponent<GeneratedTestCameraController>();
        }

        cameraController.SetTarget(target, true);
    }

    private static void AppendIdentityCount(
        StringBuilder builder,
        GeneratedRuntimeIdentity[] identities,
        GeneratedRuntimeObjectKind objectKind
    )
    {
        int count = 0;

        for (int i = 0; i < identities.Length; i++)
        {
            if (identities[i] != null && identities[i].ObjectKind == objectKind)
            {
                count++;
            }
        }

        builder.AppendLine($"- {objectKind}: {count}");
    }

    private Vector3 GetDefaultUnitSpawnPosition()
    {
        Vector2Int spawnCell = GetDefaultEntranceCell(out int entranceFloorIndex);
        Transform groundTransform = FindFloorVisualTilemap(entranceFloorIndex, "Ground");
        Tilemap groundTilemap = groundTransform != null ? groundTransform.GetComponent<Tilemap>() : null;

        if (groundTilemap != null)
        {
            return groundTilemap.GetCellCenterWorld(new Vector3Int(spawnCell.x, spawnCell.y, 0));
        }

        return new Vector3(spawnCell.x, spawnCell.y, 0f);
    }

    private Vector2Int GetDefaultEntranceCell(out int entranceFloorIndex)
    {
        GeneratedBuildingDataView dataView = GetComponent<GeneratedBuildingDataView>();

        if (dataView != null &&
            dataView.BuildingData != null &&
            dataView.BuildingData.Entrances.Count > 0 &&
            dataView.BuildingData.Entrances[0] != null)
        {
            GeneratedEntranceData entrance = dataView.BuildingData.Entrances[0];
            entranceFloorIndex = Mathf.Max(1, entrance.FloorIndex);
            return entrance.Cell;
        }

        GeneratedBuildingInstanceDataView instanceDataView = GetComponent<GeneratedBuildingInstanceDataView>();
        GeneratedBuildingInstanceData instanceData = instanceDataView != null
            ? instanceDataView.InstanceData
            : null;

        if (instanceData != null && instanceData.PrimaryEntranceFloorIndex > 0)
        {
            entranceFloorIndex = instanceData.PrimaryEntranceFloorIndex;
            return instanceData.PrimaryEntranceCell;
        }

        entranceFloorIndex = 1;

        if (profile == null)
        {
            return Vector2Int.zero;
        }

        entranceFloorIndex = Mathf.Max(1, profile.DefaultEntranceFloorIndex);
        RectInt bounds = new RectInt(profile.FootprintOrigin, profile.FootprintSize);

        switch (profile.DefaultEntranceSide)
        {
            case GeneratedWallSide.North:
                return new Vector2Int(bounds.xMax - 1, bounds.yMin + bounds.height / 2);
            case GeneratedWallSide.South:
                return new Vector2Int(bounds.xMin, bounds.yMin + bounds.height / 2);
            case GeneratedWallSide.West:
                return new Vector2Int(bounds.xMin + bounds.width / 2, bounds.yMax - 1);
            case GeneratedWallSide.East:
            default:
                return new Vector2Int(bounds.xMin + bounds.width / 2, bounds.yMin);
        }
    }

    public static string GetFloorRootName(int floorIndex)
    {
        return $"Floor {floorIndex:00}";
    }

    public static string GetFloorVisualRootName(int floorIndex)
    {
        return $"Floor {floorIndex:00} Visual Tilemaps";
    }

    public static string GetFloorRoomRootName(int floorIndex)
    {
        return $"Floor {floorIndex:00} Room Tilemaps";
    }

    public static string GetFloorColliderRootName(int floorIndex)
    {
        return $"Floor {floorIndex:00} Collider Tilemaps";
    }

    private static bool TryParseFloorIndex(string floorName, out int floorIndex)
    {
        floorIndex = 0;

        if (string.IsNullOrWhiteSpace(floorName) || !floorName.StartsWith("Floor "))
        {
            return false;
        }

        return int.TryParse(floorName.Substring("Floor ".Length), out floorIndex);
    }

    private static void AppendMissing(StringBuilder builder, bool missing, string message)
    {
        if (!missing)
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(message);
    }
}
