using UnityEngine;

public class GeneratedStairZone : MonoBehaviour
{
    [SerializeField] private string stairId;
    [SerializeField] private GeneratedStairZoneKind zoneKind;
    [SerializeField] private int lowerFloorIndex;
    [SerializeField] private int upperFloorIndex;
    [SerializeField] private int sourceFloorIndex;
    [SerializeField] private int destinationFloorIndex;

    public string StairId => stairId;
    public GeneratedStairZoneKind ZoneKind => zoneKind;
    public int LowerFloorIndex => lowerFloorIndex;
    public int UpperFloorIndex => upperFloorIndex;
    public int SourceFloorIndex => sourceFloorIndex;
    public int DestinationFloorIndex => destinationFloorIndex;
    public bool IsTransitionZone => zoneKind == GeneratedStairZoneKind.Entry ||
                                    zoneKind == GeneratedStairZoneKind.Exit;

    public void Configure(GeneratedStairData stairData, GeneratedStairZoneKind newZoneKind)
    {
        if (stairData == null)
        {
            Clear();
            return;
        }

        stairId = stairData.StairId;
        zoneKind = newZoneKind;
        lowerFloorIndex = stairData.LowerFloorIndex;
        upperFloorIndex = stairData.UpperFloorIndex;

        switch (zoneKind)
        {
            case GeneratedStairZoneKind.Entry:
                sourceFloorIndex = lowerFloorIndex;
                destinationFloorIndex = upperFloorIndex;
                break;
            case GeneratedStairZoneKind.Exit:
                sourceFloorIndex = upperFloorIndex;
                destinationFloorIndex = lowerFloorIndex;
                break;
            default:
                sourceFloorIndex = 0;
                destinationFloorIndex = 0;
                break;
        }
    }

    private void Clear()
    {
        stairId = string.Empty;
        zoneKind = GeneratedStairZoneKind.Walkable;
        lowerFloorIndex = 0;
        upperFloorIndex = 0;
        sourceFloorIndex = 0;
        destinationFloorIndex = 0;
    }
}
