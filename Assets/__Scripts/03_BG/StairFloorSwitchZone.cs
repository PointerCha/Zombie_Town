using UnityEngine;

[DisallowMultipleComponent]
public sealed class StairFloorSwitchZone : MonoBehaviour
{
    [SerializeField] private FloorController lowerFloor;
    [SerializeField] private FloorController upperFloor;
    [SerializeField] private StairEndpointZone lowerEndpoint;
    [SerializeField] private StairEndpointZone upperEndpoint;
    [SerializeField] private float completedReentryBlockDuration = 0.5f;

    public FloorController LowerFloor => lowerFloor;
    public FloorController UpperFloor => upperFloor;
    public StairEndpointZone LowerEndpoint => lowerEndpoint;
    public StairEndpointZone UpperEndpoint => upperEndpoint;
    public float CompletedReentryBlockDuration => completedReentryBlockDuration;

    private void OnValidate()
    {
        completedReentryBlockDuration = Mathf.Max(0f, completedReentryBlockDuration);
    }

    private void Awake()
    {
        if (lowerEndpoint != null)
        {
            lowerEndpoint.Initialize(this);
        }

        if (upperEndpoint != null)
        {
            upperEndpoint.Initialize(this);
        }
    }
}
