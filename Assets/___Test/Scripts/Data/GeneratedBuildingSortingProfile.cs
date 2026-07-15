using System;
using UnityEngine;

[Serializable]
public class GeneratedBuildingSortingProfile
{
    [SerializeField] private int floorOrderStep = 35;
    [SerializeField] private int groundOrderOffset = 0;
    [SerializeField] private int outerUpperWallOrderOffset = 5;
    [SerializeField] private int roomOrderStep = 20;
    [SerializeField] private int roomGroundOrderOffset = 1;
    [SerializeField] private int roomUpperWallOrderOffset = 10;
    [SerializeField] private int roomUnderWallOrderOffset = 15;
    [SerializeField] private int stairVisualOrder = 30;
    [SerializeField] private int outerUnderWallOrder = 100;
    [SerializeField] private int ceilingOrder = 105;

    public int FloorOrderStep => floorOrderStep;
    public int GroundOrderOffset => groundOrderOffset;
    public int OuterUpperWallOrderOffset => outerUpperWallOrderOffset;
    public int RoomOrderStep => roomOrderStep;
    public int RoomGroundOrderOffset => roomGroundOrderOffset;
    public int RoomUpperWallOrderOffset => roomUpperWallOrderOffset;
    public int RoomUnderWallOrderOffset => roomUnderWallOrderOffset;
    public int StairVisualOrder => stairVisualOrder;
    public int OuterUnderWallOrder => outerUnderWallOrder;
    public int CeilingOrder => ceilingOrder;

    public int GetFloorBaseOrder(int zeroBasedFloorIndex)
    {
        return Mathf.Max(0, zeroBasedFloorIndex) * floorOrderStep;
    }

    public int GetGroundOrder(int zeroBasedFloorIndex)
    {
        return GetFloorBaseOrder(zeroBasedFloorIndex) + groundOrderOffset;
    }

    public int GetOuterUpperWallOrder(int zeroBasedFloorIndex)
    {
        return GetFloorBaseOrder(zeroBasedFloorIndex) + outerUpperWallOrderOffset;
    }

    public int GetRoomGroundOrder(int zeroBasedFloorIndex, int zeroBasedRoomIndex)
    {
        return GetFloorBaseOrder(zeroBasedFloorIndex)
            + zeroBasedRoomIndex * roomOrderStep
            + roomGroundOrderOffset;
    }

    public int GetRoomUpperWallOrder(int zeroBasedFloorIndex, int zeroBasedRoomIndex)
    {
        return GetFloorBaseOrder(zeroBasedFloorIndex)
            + zeroBasedRoomIndex * roomOrderStep
            + roomUpperWallOrderOffset;
    }

    public int GetRoomUnderWallOrder(int zeroBasedFloorIndex, int zeroBasedRoomIndex)
    {
        return GetFloorBaseOrder(zeroBasedFloorIndex)
            + zeroBasedRoomIndex * roomOrderStep
            + roomUnderWallOrderOffset;
    }
}
