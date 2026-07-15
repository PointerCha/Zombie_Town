using UnityEngine;

public class GeneratedRuntimeIdentity : MonoBehaviour
{
    [SerializeField] private string buildingInstanceId;
    [SerializeField] private string buildingId;
    [SerializeField] private string profileId;
    [SerializeField] private string objectId;
    [SerializeField] private string ownerId;
    [SerializeField] private GeneratedRuntimeObjectKind objectKind;
    [SerializeField] private int floorIndex;
    [SerializeField] private int roomIndex;
    [SerializeField] private RectInt bounds;
    [SerializeField] private bool colliderObject;
    [SerializeField] private bool triggerObject;
    [SerializeField] private int bindingRevision;
    [SerializeField] private GeneratedBuildingRuntimeRegistry registry;

    public string BuildingInstanceId => buildingInstanceId;
    public string BuildingId => buildingId;
    public string ProfileId => profileId;
    public string ObjectId => objectId;
    public string OwnerId => ownerId;
    public GeneratedRuntimeObjectKind ObjectKind => objectKind;
    public int FloorIndex => floorIndex;
    public int RoomIndex => roomIndex;
    public RectInt Bounds => bounds;
    public bool ColliderObject => colliderObject;
    public bool TriggerObject => triggerObject;
    public int BindingRevision => bindingRevision;
    public GeneratedBuildingRuntimeRegistry Registry => registry;

    public void Configure(
        string newBuildingInstanceId,
        string newBuildingId,
        string newProfileId,
        string newObjectId,
        string newOwnerId,
        GeneratedRuntimeObjectKind newObjectKind,
        int newFloorIndex,
        int newRoomIndex,
        RectInt newBounds,
        bool newColliderObject,
        bool newTriggerObject,
        int newBindingRevision,
        GeneratedBuildingRuntimeRegistry newRegistry
    )
    {
        buildingInstanceId = newBuildingInstanceId;
        buildingId = newBuildingId;
        profileId = newProfileId;
        objectId = newObjectId;
        ownerId = newOwnerId;
        objectKind = newObjectKind;
        floorIndex = newFloorIndex;
        roomIndex = newRoomIndex;
        bounds = newBounds;
        colliderObject = newColliderObject;
        triggerObject = newTriggerObject;
        bindingRevision = newBindingRevision;
        registry = newRegistry;
    }
}
