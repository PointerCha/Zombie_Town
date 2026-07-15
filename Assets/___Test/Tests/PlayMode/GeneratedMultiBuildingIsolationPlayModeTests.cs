using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace ZombieTown.GeneratedBuilding.PlayModeTests
{
    public sealed class GeneratedMultiBuildingIsolationPlayModeTests
    {
        [UnityTest]
        public IEnumerator GameplayScenarios_EntryRoomStairsVisibilityAndCameraPass()
        {
            GameObject cameraObject = new GameObject("Gameplay Matrix Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            GeneratedTestCameraController cameraController = cameraObject.AddComponent<GeneratedTestCameraController>();
            BuildingHarness building = new BuildingHarness("Matrix", Vector3.zero);
            UnitHarness unit = new UnitHarness("Gameplay Matrix Unit");

            try
            {
                cameraController.SetTarget(unit.GameObject.transform, true);
                Assert.That(cameraController.Target, Is.SameAs(unit.GameObject.transform), "scenario=camera target");
                Assert.That(camera.transform.position, Is.EqualTo(unit.GameObject.transform.position + new Vector3(0f, 0f, -10f)), "scenario=camera snap");

                unit.Visibility.ProcessTriggerEnter(building.FloorTriggerCollider);
                yield return new WaitForSeconds(0.3f);
                Assert.That(unit.Context.CurrentRegistry, Is.SameAs(building.Registry), "floor=1, scenario=building entry");
                Assert.That(building.FloorVisual.color.a, Is.EqualTo(0.35f).Within(0.02f), "floor=1, scenario=wall fade");
                Assert.That(building.CeilingVisual.color.a, Is.EqualTo(0.05f).Within(0.02f), "floor=1, scenario=ceiling fade");

                unit.Visibility.ProcessTriggerEnter(building.RoomTriggerCollider);
                yield return new WaitForSeconds(0.3f);
                Assert.That(building.RoomVisual.color.a, Is.EqualTo(0.25f).Within(0.02f), "floor=1, room=1, scenario=room entry fade");

                unit.Controller.SetCurrentFloorIndex(1);
                AssertCollisionState(unit, building.Floor1Collider, false);
                AssertCollisionState(unit, building.Floor2Collider, true);
                unit.StairAgent.ProcessTriggerEnter(building.StairEntryCollider);
                Assert.That(unit.Controller.CurrentFloorIndex, Is.EqualTo(2), "floor=2, scenario=stair up");
                AssertCollisionState(unit, building.Floor1Collider, true);
                AssertCollisionState(unit, building.Floor2Collider, false);

                unit.StairAgent.ProcessTriggerEnter(building.StairExitCollider);
                yield return new WaitForSeconds(0.4f);
                unit.StairAgent.ProcessTriggerEnter(building.StairExitCollider);
                Assert.That(unit.Controller.CurrentFloorIndex, Is.EqualTo(1), "floor=1, scenario=stair return");

                unit.StairAgent.ProcessTriggerExit(building.StairEntryCollider);
                unit.StairAgent.ProcessTriggerExit(building.StairExitCollider);
                unit.Visibility.ProcessTriggerExit(building.RoomTriggerCollider);
                unit.Visibility.ProcessTriggerExit(building.FloorTriggerCollider);
                yield return new WaitForSeconds(0.3f);
                Assert.That(unit.Context.CurrentRegistry, Is.Null, "scenario=building exit");
                Assert.That(building.RoomVisual.color.a, Is.EqualTo(1f).Within(0.02f), "scenario=room fade restore");
                Assert.That(building.CeilingVisual.color.a, Is.EqualTo(1f).Within(0.02f), "scenario=ceiling fade restore");
            }
            finally
            {
                unit.Dispose();
                building.Dispose();
                Object.DestroyImmediate(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator TwoBuildingsAndTwoUnits_KeepRuntimeStateIsolated()
        {
            GameObject cameraObject = new GameObject("Generated Test Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            BuildingHarness buildingA = new BuildingHarness("A", new Vector3(-20f, 0f, 0f));
            BuildingHarness buildingB = new BuildingHarness("B", new Vector3(20f, 0f, 0f));
            UnitHarness unitA = new UnitHarness("Unit A");
            UnitHarness unitB = new UnitHarness("Unit B");

            try
            {
                yield return null;

                Assert.That(unitA.Context.NotifyTriggerEnter(buildingA.FloorTrigger), Is.True);
                Assert.That(unitB.Context.NotifyTriggerEnter(buildingB.FloorTrigger), Is.True);
                unitA.Controller.SetCurrentFloorIndex(1);
                unitB.Controller.SetCurrentFloorIndex(2);

                Assert.That(unitA.Context.CurrentRegistry, Is.SameAs(buildingA.Registry));
                Assert.That(unitB.Context.CurrentRegistry, Is.SameAs(buildingB.Registry));
                Assert.That(unitA.Visibility.BuildingRegistry, Is.SameAs(buildingA.Registry));
                Assert.That(unitB.Visibility.BuildingRegistry, Is.SameAs(buildingB.Registry));
                Assert.That(unitA.Context.NotifyTriggerEnter(buildingB.FloorTrigger), Is.False);
                Assert.That(unitB.Context.NotifyTriggerEnter(buildingA.FloorTrigger), Is.False);

                AssertCollisionState(unitA, buildingA.Floor1Collider, false);
                AssertCollisionState(unitA, buildingA.Floor2Collider, true);
                AssertCollisionState(unitA, buildingB.Floor1Collider, false);
                AssertCollisionState(unitA, buildingB.Floor2Collider, false);
                AssertCollisionState(unitB, buildingB.Floor1Collider, true);
                AssertCollisionState(unitB, buildingB.Floor2Collider, false);
                AssertCollisionState(unitB, buildingA.Floor1Collider, false);
                AssertCollisionState(unitB, buildingA.Floor2Collider, false);

                unitA.StairAgent.ProcessTriggerEnter(buildingA.StairEntryCollider);
                yield return null;

                Assert.That(unitA.Controller.CurrentFloorIndex, Is.EqualTo(2));
                Assert.That(unitB.Controller.CurrentFloorIndex, Is.EqualTo(2));
                Assert.That(unitA.Context.CurrentRegistry, Is.SameAs(buildingA.Registry));
                Assert.That(unitB.Context.CurrentRegistry, Is.SameAs(buildingB.Registry));
                AssertCollisionState(unitA, buildingA.Floor1Collider, true);
                AssertCollisionState(unitA, buildingA.Floor2Collider, false);
                AssertCollisionState(unitB, buildingA.Floor1Collider, false);

                Assert.That(unitA.Context.NotifyTriggerExit(buildingA.FloorTrigger), Is.True);
                Assert.That(unitA.Context.NotifyTriggerExit(buildingA.StairEntryIdentity), Is.True);
                Assert.That(unitA.Context.CurrentRegistry, Is.Null);
                Assert.That(unitA.Visibility.BuildingRegistry, Is.Null);
                AssertCollisionState(unitA, buildingA.Floor1Collider, false);
                AssertCollisionState(unitA, buildingA.Floor2Collider, false);
                Assert.That(unitB.Context.CurrentRegistry, Is.SameAs(buildingB.Registry));

                unitB.GameObject.SetActive(false);
                yield return null;

                Assert.That(unitB.Context.CurrentRegistry, Is.Null);
                Assert.That(unitB.Visibility.BuildingRegistry, Is.Null);
                AssertCollisionState(unitB, buildingB.Floor1Collider, false);
                AssertCollisionState(unitB, buildingB.Floor2Collider, false);
            }
            finally
            {
                unitA.Dispose();
                unitB.Dispose();
                buildingA.Dispose();
                buildingB.Dispose();
                Object.Destroy(cameraObject);
            }
        }

        private static void AssertCollisionState(UnitHarness unit, Collider2D target, bool expectedIgnored)
        {
            Assert.That(
                Physics2D.GetIgnoreCollision(unit.Collider, target),
                Is.EqualTo(expectedIgnored),
                $"unit={unit.GameObject.name}, collider={target.name}"
            );
        }

        private sealed class BuildingHarness
        {
            public GameObject GameObject { get; }
            public GeneratedBuildingRuntimeRegistry Registry { get; }
            public Collider2D Floor1Collider { get; }
            public Collider2D Floor2Collider { get; }
            public GeneratedRuntimeIdentity FloorTrigger { get; }
            public Collider2D FloorTriggerCollider { get; }
            public Collider2D RoomTriggerCollider { get; }
            public Collider2D StairEntryCollider { get; }
            public Collider2D StairExitCollider { get; }
            public GeneratedRuntimeIdentity StairEntryIdentity { get; }
            public Tilemap FloorVisual { get; }
            public Tilemap RoomVisual { get; }
            public Tilemap CeilingVisual { get; }

            public BuildingHarness(string suffix, Vector3 position)
            {
                GameObject = new GameObject($"Building {suffix}");
                GameObject.transform.position = position;
                GeneratedVisibilityController visibilityController = GameObject.AddComponent<GeneratedVisibilityController>();
                Registry = GameObject.AddComponent<GeneratedBuildingRuntimeRegistry>();
                int revision = Registry.BeginRebuild($"instance_{suffix}", "shared_building", "shared_profile");
                Floor1Collider = CreateIdentityCollider("Floor 1 Wall", suffix, revision, 1, false, GeneratedRuntimeObjectKind.FloorUpperWallCollider, out _);
                Floor2Collider = CreateIdentityCollider("Floor 2 Wall", suffix, revision, 2, false, GeneratedRuntimeObjectKind.FloorUpperWallCollider, out _);
                FloorTriggerCollider = CreateIdentityCollider("Floor 1 Trigger", suffix, revision, 1, true, GeneratedRuntimeObjectKind.FloorGroundTrigger, out GeneratedRuntimeIdentity floorTrigger);
                FloorTrigger = floorTrigger;
                RoomTriggerCollider = CreateIdentityCollider("Room 1 Trigger", suffix, revision, 1, true, GeneratedRuntimeObjectKind.RoomGroundTrigger, out _, 1);

                FloorVisual = CreateVisibilityGroup("Floor Visual", GeneratedVisibilityGroupKind.Floor, 1, 0);
                RoomVisual = CreateVisibilityGroup("Room Visual", GeneratedVisibilityGroupKind.Room, 1, 1);
                CeilingVisual = CreateVisibilityGroup("Ceiling Visual", GeneratedVisibilityGroupKind.Ceiling, 0, 0);

                GeneratedStairData stairData = new GeneratedStairData();
                stairData.Initialize("shared_stair", 1, 2, GeneratedStairDirection.FloorUp, new RectInt(0, 0, 2, 2));
                StairEntryCollider = CreateIdentityCollider("Stair Entry", suffix, revision, 1, true, GeneratedRuntimeObjectKind.Stair, out GeneratedRuntimeIdentity stairIdentity);
                StairEntryIdentity = stairIdentity;
                StairEntryCollider.gameObject.AddComponent<GeneratedStairZone>().Configure(
                    stairData,
                    GeneratedStairZoneKind.Entry
                );
                StairExitCollider = CreateIdentityCollider("Stair Exit", suffix, revision, 2, true, GeneratedRuntimeObjectKind.Stair, out _);
                StairExitCollider.gameObject.AddComponent<GeneratedStairZone>().Configure(
                    stairData,
                    GeneratedStairZoneKind.Exit
                );
                Registry.CompleteRebuild(GameObject.transform);
                visibilityController.RebuildCache();
            }

            private Tilemap CreateVisibilityGroup(
                string groupName,
                GeneratedVisibilityGroupKind kind,
                int floorIndex,
                int roomIndex
            )
            {
                GameObject child = new GameObject(groupName);
                child.transform.SetParent(GameObject.transform);
                Tilemap tilemap = child.AddComponent<Tilemap>();
                TilemapRenderer renderer = child.AddComponent<TilemapRenderer>();
                GeneratedVisibilityGroup group = child.AddComponent<GeneratedVisibilityGroup>();
                group.Configure(groupName, kind, floorIndex, roomIndex);
                group.AddTarget(renderer);
                return tilemap;
            }

            private Collider2D CreateIdentityCollider(
                string name,
                string suffix,
                int revision,
                int floorIndex,
                bool isTrigger,
                GeneratedRuntimeObjectKind objectKind,
                out GeneratedRuntimeIdentity identity,
                int roomIndex = 0
            )
            {
                GameObject child = new GameObject(name);
                child.transform.SetParent(GameObject.transform);
                BoxCollider2D collider = child.AddComponent<BoxCollider2D>();
                collider.isTrigger = isTrigger;
                identity = child.AddComponent<GeneratedRuntimeIdentity>();
                identity.Configure(
                    $"instance_{suffix}",
                    "shared_building",
                    "shared_profile",
                    $"{suffix}_{name}",
                    "shared_building",
                    objectKind,
                    floorIndex,
                    roomIndex,
                    new RectInt(0, 0, 2, 2),
                    true,
                    isTrigger,
                    revision,
                    Registry
                );
                return collider;
            }

            public void Dispose()
            {
                Object.DestroyImmediate(GameObject);
            }
        }

        private sealed class UnitHarness
        {
            public GameObject GameObject { get; }
            public CircleCollider2D Collider { get; }
            public GeneratedTestUnitController Controller { get; }
            public GeneratedVisibilityAgent Visibility { get; }
            public GeneratedBuildingContextAgent Context { get; }
            public GeneratedStairTransitionAgent StairAgent { get; }

            public UnitHarness(string name)
            {
                GameObject = new GameObject(name);
                GameObject.transform.position = new Vector3(0f, 10000f, 0f);
                Rigidbody2D body = GameObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                Collider = GameObject.AddComponent<CircleCollider2D>();
                GameObject.AddComponent<GeneratedFloorIsolationAgent>();
                Visibility = GameObject.AddComponent<GeneratedVisibilityAgent>();
                Context = GameObject.AddComponent<GeneratedBuildingContextAgent>();
                Controller = GameObject.AddComponent<GeneratedTestUnitController>();
                StairAgent = GameObject.AddComponent<GeneratedStairTransitionAgent>();
            }

            public void Dispose()
            {
                Object.DestroyImmediate(GameObject);
            }
        }
    }
}
