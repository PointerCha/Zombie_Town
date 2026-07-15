using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZombieTown.GeneratedBuilding.Tests
{
    public class GeneratedBuildingRegressionTests
    {
        private const string ProfileRoot = "Assets/___Test/Data/Profiles";

        private static readonly int[] FixedSeeds =
        {
            1001,
            1002,
            2027,
            4099,
            7919
        };

        [Test]
        public void BatchedTilePainting_MatchesLegacyTilesAndBounds()
        {
            GameObject root = new GameObject("27D-A Tilemap Test");
            Tile tile = ScriptableObject.CreateInstance<Tile>();

            try
            {
                Transform target = GeneratedTilemapUtility.EnsureChild(root.transform, "Tilemap");
                Tilemap tilemap = GeneratedTilemapUtility.EnsureTilemap(
                    target,
                    Vector3.one,
                    GridLayout.CellLayout.Rectangle,
                    0
                );
                GeneratedCellMask mask = new GeneratedCellMask();
                mask.Initialize("Sparse", new Vector2Int(-2, -1), new Vector2Int(8, 6));
                mask.AddCell(new Vector2Int(-2, -1));
                mask.AddCell(new Vector2Int(0, 0));
                mask.AddCell(new Vector2Int(5, 4));

                GeneratedTilemapUtility.UseBatchedPainting = false;
                GeneratedTilemapUtility.PaintCells(tilemap, mask, tile);
                BoundsInt legacyBounds = tilemap.cellBounds;

                GeneratedTilemapUtility.Clear(tilemap);
                GeneratedTilemapUtility.UseBatchedPainting = true;
                GeneratedTilemapUtility.PaintCells(tilemap, mask, tile);

                Assert.That(tilemap.cellBounds, Is.EqualTo(legacyBounds));

                for (int i = 0; i < mask.Cells.Count; i++)
                {
                    Vector2Int cell = mask.Cells[i].Position;
                    Assert.That(tilemap.GetTile(new Vector3Int(cell.x, cell.y, 0)), Is.SameAs(tile));
                }
            }
            finally
            {
                GeneratedTilemapUtility.UseBatchedPainting = true;
                Object.DestroyImmediate(tile);
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TransientGenerationData_CanReleaseWithoutLosingCompactRuntimeMetadata()
        {
            GeneratedBuildingProfile profile = LoadProfile(GetProfilePaths()[0]);
            GeneratedBuildingData generatedData = GeneratedBuildingDataGenerator.GenerateFull(profile, 1001);
            GeneratedBuildingInstanceData instanceData = new GeneratedBuildingInstanceData();
            instanceData.Initialize(generatedData, profile);
            GameObject owner = new GameObject("27D-B Data Lifetime Test");

            try
            {
                GeneratedBuildingDataView view = owner.AddComponent<GeneratedBuildingDataView>();
                view.SetData(generatedData);

                Assert.That(view.HasTransientData, Is.True);
                Assert.That(view.ReleaseTransientData(true), Is.True);
                Assert.That(view.HasTransientData, Is.False);
                Assert.That(instanceData.BuildingId, Is.EqualTo(generatedData.BuildingId));
                Assert.That(instanceData.DeterministicHash, Is.EqualTo(generatedData.BuildDeterministicHash()));
                Assert.That(instanceData.FloorCount, Is.EqualTo(generatedData.Floors.Count));
                Assert.That(instanceData.PrimaryEntranceFloorIndex, Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [TestCaseSource(nameof(ProfileSeedCases))]
        public void GameplayMatrix_DataContractsPassForEveryProfileAndSeed(string profilePath, int seed)
        {
            GeneratedBuildingProfile profile = LoadProfile(profilePath);
            GeneratedBuildingData data = GeneratedBuildingDataGenerator.GenerateFull(profile, seed);
            GeneratedBuildingValidationResult result = GeneratedBuildingValidator.Validate(data, profile);
            string context = BuildContext(profile, seed, "scenario=data contracts");

            Assert.That(result.IsValid, Is.True, $"{context}\n{result.BuildSummary()}");

            if (profile.FloorCount == 1)
            {
                Assert.That(data.Floors.Count, Is.EqualTo(1), $"{context}, floor=1, scenario=one-floor isolation");
                Assert.That(data.Floors[0].Stairs.Count, Is.Zero, $"{context}, floor=1, scenario=no stale stairs");
                Assert.That(data.GetFloor(2), Is.Null, $"{context}, floor=2, scenario=no stale second floor");
            }

            for (int floorIndex = 0; floorIndex < data.Floors.Count; floorIndex++)
            {
                GeneratedFloorData floor = data.Floors[floorIndex];

                for (int roomIndex = 0; roomIndex < floor.Rooms.Count; roomIndex++)
                {
                    GeneratedRoomData room = floor.Rooms[roomIndex];

                    for (int doorIndex = 0; doorIndex < room.Doors.Count; doorIndex++)
                    {
                        Assert.That(
                            room.Doors[doorIndex].Cells.Count,
                            Is.GreaterThanOrEqualTo(3),
                            $"{context}, floor={floor.FloorIndex}, room={roomIndex + 1}, scenario=door clearance"
                        );
                    }
                }
            }

            for (int i = 0; i < data.Entrances.Count; i++)
            {
                Assert.That(
                    data.Entrances[i].Cells.Count,
                    Is.GreaterThanOrEqualTo(3),
                    $"{context}, floor={data.Entrances[i].FloorIndex}, scenario=entrance clearance"
                );
                float physicalOpening = data.Entrances[i].Cells.Count *
                                        Mathf.Min(profile.GridProfile.VisualCellSize.x, profile.GridProfile.VisualCellSize.y);
                Assert.That(
                    physicalOpening,
                    Is.GreaterThanOrEqualTo(0.5f),
                    $"{context}, floor={data.Entrances[i].FloorIndex}, scenario=unit collider entrance clearance"
                );
            }
        }

        public static IEnumerable<TestCaseData> ProfileSeedCases()
        {
            List<string> profilePaths = GetProfilePaths();

            for (int profileIndex = 0; profileIndex < profilePaths.Count; profileIndex++)
            {
                string profilePath = profilePaths[profileIndex];
                GeneratedBuildingProfile profile = AssetDatabase.LoadAssetAtPath<GeneratedBuildingProfile>(profilePath);
                string profileName = profile != null ? profile.BuildingId : profilePath;

                for (int seedIndex = 0; seedIndex < FixedSeeds.Length; seedIndex++)
                {
                    int seed = FixedSeeds[seedIndex];
                    yield return new TestCaseData(profilePath, seed)
                        .SetName($"Generate_{profileName}_Seed_{seed}");
                }
            }
        }

        public static IEnumerable<TestCaseData> ProfileCases()
        {
            List<string> profilePaths = GetProfilePaths();

            for (int i = 0; i < profilePaths.Count; i++)
            {
                string profilePath = profilePaths[i];
                GeneratedBuildingProfile profile = AssetDatabase.LoadAssetAtPath<GeneratedBuildingProfile>(profilePath);
                string profileName = profile != null ? profile.BuildingId : profilePath;
                yield return new TestCaseData(profilePath).SetName($"SeedVariation_{profileName}");
            }
        }

        [TestCaseSource(nameof(ProfileSeedCases))]
        public void FullGeneration_IsValidAndDeterministic(string profilePath, int seed)
        {
            GeneratedBuildingProfile profile = LoadProfile(profilePath);
            GeneratedBuildingData first = GeneratedBuildingDataGenerator.GenerateFull(profile, seed);
            GeneratedBuildingData second = GeneratedBuildingDataGenerator.GenerateFull(profile, seed);

            Assert.That(first, Is.Not.Null, BuildContext(profile, seed, "first result is null"));
            Assert.That(second, Is.Not.Null, BuildContext(profile, seed, "second result is null"));

            GeneratedBuildingValidationResult validation = GeneratedBuildingValidator.Validate(first, profile);
            GeneratedBuildingInstanceData instance = new GeneratedBuildingInstanceData();
            instance.Initialize(first, profile);
            RectInt expectedCeiling = GeneratedCeilingRenderer.CalculateFootprint(
                first.Footprint,
                profile.TilePalette
            );

            Assert.That(
                validation.IsValid,
                Is.True,
                BuildContext(profile, seed, validation.BuildSummary())
            );
            Assert.That(
                validation.WarningCount,
                Is.Zero,
                BuildContext(profile, seed, validation.BuildSummary())
            );
            Assert.That(
                second.BuildDeterministicFingerprint(),
                Is.EqualTo(first.BuildDeterministicFingerprint()),
                BuildContext(profile, seed, "same seed fingerprint mismatch")
            );
            Assert.That(
                second.BuildDeterministicHash(),
                Is.EqualTo(first.BuildDeterministicHash()),
                BuildContext(profile, seed, "same seed hash mismatch")
            );
            Assert.That(first.Floors.Count, Is.EqualTo(profile.FloorCount));
            Assert.That(instance.Footprint, Is.EqualTo(first.Footprint));
            Assert.That(instance.FloorCount, Is.EqualTo(first.Floors.Count));
            Assert.That(instance.EntranceCount, Is.EqualTo(first.Entrances.Count));
            Assert.That(expectedCeiling.position, Is.EqualTo(first.Footprint.position + profile.TilePalette.CeilingCellOffset));
            Assert.That(expectedCeiling.size, Is.EqualTo(first.Footprint.size));
            Assert.That(
                GeneratedCeilingRenderer.CalculateLocalPosition(profile),
                Is.EqualTo(profile.GetFloorLocalPosition(profile.FloorCount - 1))
            );

            for (int floorIndex = 0; floorIndex < first.Floors.Count; floorIndex++)
            {
                AssertOuterCornerCoverage(first.Floors[floorIndex], profile, seed);
            }
        }

        [TestCaseSource(nameof(ProfileCases))]
        public void DifferentSeed_ProducesDifferentValidData(string profilePath)
        {
            GeneratedBuildingProfile profile = LoadProfile(profilePath);
            GeneratedBuildingData first = GeneratedBuildingDataGenerator.GenerateFull(profile, FixedSeeds[0]);
            GeneratedBuildingData different = GeneratedBuildingDataGenerator.GenerateFull(profile, FixedSeeds[1]);
            GeneratedBuildingValidationResult validation = GeneratedBuildingValidator.Validate(different, profile);

            Assert.That(
                validation.IsValid,
                Is.True,
                BuildContext(profile, FixedSeeds[1], validation.BuildSummary())
            );
            Assert.That(
                different.BuildDeterministicFingerprint(),
                Is.Not.EqualTo(first.BuildDeterministicFingerprint()),
                BuildContext(profile, FixedSeeds[1], "different seed did not change generated data")
            );
        }

        [Test]
        public void ProfileCatalog_HasUniqueValidIds()
        {
            List<string> profilePaths = GetProfilePaths();
            HashSet<string> ids = new HashSet<string>();

            Assert.That(profilePaths.Count, Is.GreaterThanOrEqualTo(4), "Expected at least four building profiles.");

            for (int i = 0; i < profilePaths.Count; i++)
            {
                GeneratedBuildingProfile profile = LoadProfile(profilePaths[i]);
                Assert.That(profile.IsValid(out string error), Is.True, $"{profilePaths[i]}: {error}");
                Assert.That(ids.Add(profile.BuildingId), Is.True, $"Duplicate building id: {profile.BuildingId}");
            }
        }

        [TestCase(1, 0f)]
        [TestCase(2, 1.5f)]
        [TestCase(3, 3f)]
        public void CeilingLocalPosition_MatchesTopFloorForAnyFloorCount(
            int floorCount,
            float expectedY
        )
        {
            Vector3 result = GeneratedCeilingRenderer.CalculateLocalPosition(
                floorCount,
                new Vector3(0f, 1.5f, 0f)
            );

            Assert.That(result, Is.EqualTo(new Vector3(0f, expectedY, 0f)));
        }

        [Test]
        public void BuildingInstanceIds_AreUniqueAcrossSceneRoots()
        {
            GameObject firstObject = new GameObject("First Building Generator");
            GameObject secondObject = new GameObject("Second Building Generator");

            try
            {
                GeneratedBuildingSceneRoot first = firstObject.AddComponent<GeneratedBuildingSceneRoot>();
                GeneratedBuildingSceneRoot second = secondObject.AddComponent<GeneratedBuildingSceneRoot>();

                const string duplicatedSerializedId = "duplicated_serialized_building_id";
                Assert.That(first.SetBuildingInstanceId(duplicatedSerializedId), Is.True);
                Assert.That(second.SetBuildingInstanceId(duplicatedSerializedId), Is.True);

                Assert.That(first.EnsureBuildingInstanceId(), Is.Not.EqualTo(duplicatedSerializedId));
                Assert.That(second.EnsureBuildingInstanceId(), Is.EqualTo(duplicatedSerializedId));
                Assert.That(second.BuildingInstanceId, Is.Not.EqualTo(first.BuildingInstanceId));
                Assert.That(first.EnsureBuildingInstanceId(), Is.EqualTo(first.BuildingInstanceId));
            }
            finally
            {
                Object.DestroyImmediate(firstObject);
                Object.DestroyImmediate(secondObject);
            }
        }

        [Test]
        public void RuntimeRegistry_RebuildExcludesStaleOwnership()
        {
            GameObject building = new GameObject("Generated Building");
            GameObject colliderObject = new GameObject("Owned Collider");
            colliderObject.transform.SetParent(building.transform);

            try
            {
                GeneratedBuildingRuntimeRegistry registry =
                    building.AddComponent<GeneratedBuildingRuntimeRegistry>();
                BoxCollider2D collider = colliderObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                GeneratedRuntimeIdentity identity =
                    colliderObject.AddComponent<GeneratedRuntimeIdentity>();

                int firstRevision = registry.BeginRebuild("instance_a", "building_a", "profile_a");
                ConfigureTestIdentity(identity, registry, "instance_a", firstRevision);
                registry.CompleteRebuild(building.transform);

                Assert.That(registry.IsReady, Is.True);
                Assert.That(registry.Identities.Count, Is.EqualTo(1));
                Assert.That(registry.Colliders.Count, Is.EqualTo(1));
                Assert.That(registry.TriggerColliders.Count, Is.EqualTo(1));
                Assert.That(registry.Owns(identity), Is.True);

                int secondRevision = registry.BeginRebuild("instance_a", "building_a", "profile_a");
                registry.CompleteRebuild(building.transform);

                Assert.That(secondRevision, Is.GreaterThan(firstRevision));
                Assert.That(registry.IsReady, Is.True);
                Assert.That(registry.Identities, Is.Empty);
                Assert.That(registry.Colliders, Is.Empty);
                Assert.That(registry.Owns(identity), Is.False);

                ConfigureTestIdentity(identity, registry, "instance_a", secondRevision);
                registry.CompleteRebuild(building.transform);

                Assert.That(registry.Identities.Count, Is.EqualTo(1));
                Assert.That(registry.Owns(identity), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(building);
            }
        }

        [Test]
        public void RuntimeBinder_ReusesRegistryAndAtomicallyAdvancesRevision()
        {
            GeneratedBuildingProfile profile = LoadProfile(GetProfilePaths()[0]);
            GeneratedBuildingData data = GeneratedBuildingDataGenerator.GenerateFull(profile, FixedSeeds[0]);
            GameObject generatorObject = new GameObject("Building Generator");
            GameObject houseObject = new GameObject("Generated House Root");
            houseObject.transform.SetParent(generatorObject.transform);

            try
            {
                GeneratedBuildingSceneRoot sceneRoot =
                    generatorObject.AddComponent<GeneratedBuildingSceneRoot>();
                SerializedObject serializedRoot = new SerializedObject(sceneRoot);
                serializedRoot.FindProperty("generatedHouseRoot").objectReferenceValue = houseObject.transform;
                serializedRoot.ApplyModifiedPropertiesWithoutUndo();

                GeneratedRuntimeBinder.Bind(sceneRoot, data, profile);

                GeneratedBuildingRuntimeRegistry registry =
                    houseObject.GetComponent<GeneratedBuildingRuntimeRegistry>();
                GeneratedRuntimeIdentity buildingIdentity =
                    houseObject.GetComponent<GeneratedRuntimeIdentity>();

                Assert.That(registry, Is.Not.Null);
                Assert.That(registry.IsReady, Is.True);
                Assert.That(registry.BuildingInstanceId, Is.EqualTo(sceneRoot.BuildingInstanceId));
                Assert.That(registry.BuildingId, Is.EqualTo(data.BuildingId));
                Assert.That(registry.ProfileId, Is.EqualTo(profile.BuildingId));
                Assert.That(registry.Owns(buildingIdentity), Is.True);
                Assert.That(registry.Identities, Does.Contain(buildingIdentity));

                int firstRevision = registry.BindingRevision;
                GeneratedRuntimeBinder.Bind(sceneRoot, data, profile);

                Assert.That(houseObject.GetComponents<GeneratedBuildingRuntimeRegistry>(), Has.Length.EqualTo(1));
                Assert.That(registry.BindingRevision, Is.GreaterThan(firstRevision));
                Assert.That(buildingIdentity.BindingRevision, Is.EqualTo(registry.BindingRevision));
                Assert.That(registry.Owns(buildingIdentity), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(generatorObject);
            }
        }

        [Test]
        public void RuntimeBinder_AssignsPositiveFloorOwnershipToEveryStairColliderZone()
        {
            GeneratedBuildingProfile profile = LoadFirstStairProfile();
            GeneratedBuildingData data = GeneratedBuildingDataGenerator.GenerateFull(profile, FixedSeeds[0]);
            GeneratedStairData stair = data.Floors[0].Stairs[0];
            GameObject generatorObject = new GameObject("Building Generator");
            GameObject houseObject = new GameObject("Generated House Root");
            GameObject stairRootObject = new GameObject("Stair");
            houseObject.transform.SetParent(generatorObject.transform);
            stairRootObject.transform.SetParent(houseObject.transform);

            try
            {
                GeneratedBuildingSceneRoot sceneRoot =
                    generatorObject.AddComponent<GeneratedBuildingSceneRoot>();
                SerializedObject serializedRoot = new SerializedObject(sceneRoot);
                serializedRoot.FindProperty("generatedHouseRoot").objectReferenceValue = houseObject.transform;
                serializedRoot.FindProperty("stairRoot").objectReferenceValue = stairRootObject.transform;
                serializedRoot.ApplyModifiedPropertiesWithoutUndo();

                GeneratedStairZoneKind[] zoneKinds =
                {
                    GeneratedStairZoneKind.Walkable,
                    GeneratedStairZoneKind.LeftRail,
                    GeneratedStairZoneKind.RightRail,
                    GeneratedStairZoneKind.Entry,
                    GeneratedStairZoneKind.Exit
                };

                for (int i = 0; i < zoneKinds.Length; i++)
                {
                    GameObject zoneObject = new GameObject(zoneKinds[i].ToString());
                    zoneObject.transform.SetParent(stairRootObject.transform);
                    BoxCollider2D collider = zoneObject.AddComponent<BoxCollider2D>();
                    collider.isTrigger = zoneKinds[i] == GeneratedStairZoneKind.Walkable ||
                                         zoneKinds[i] == GeneratedStairZoneKind.Entry ||
                                         zoneKinds[i] == GeneratedStairZoneKind.Exit;
                    zoneObject.AddComponent<GeneratedStairZone>().Configure(stair, zoneKinds[i]);
                }

                GeneratedRuntimeBinder.Bind(sceneRoot, data, profile);

                GeneratedStairZone[] zones =
                    stairRootObject.GetComponentsInChildren<GeneratedStairZone>(true);

                for (int i = 0; i < zones.Length; i++)
                {
                    GeneratedRuntimeIdentity identity =
                        zones[i].GetComponent<GeneratedRuntimeIdentity>();
                    int expectedFloor = zones[i].ZoneKind == GeneratedStairZoneKind.Exit
                        ? stair.UpperFloorIndex
                        : stair.LowerFloorIndex;

                    Assert.That(identity, Is.Not.Null, zones[i].ZoneKind.ToString());
                    Assert.That(identity.FloorIndex, Is.EqualTo(expectedFloor), zones[i].ZoneKind.ToString());
                    Assert.That(identity.Registry.Owns(identity), Is.True, zones[i].ZoneKind.ToString());
                }
            }
            finally
            {
                Object.DestroyImmediate(generatorObject);
            }
        }

        [Test]
        public void UnitBuildingContext_IsolatesCollisionAndVisibilityByRegistry()
        {
            GameObject buildingA = new GameObject("Building A");
            GameObject buildingB = new GameObject("Building B");
            GameObject unit = new GameObject("Unit");

            try
            {
                GeneratedBuildingRuntimeRegistry registryA = CreateTestRegistry(buildingA, "instance_a");
                GeneratedBuildingRuntimeRegistry registryB = CreateTestRegistry(buildingB, "instance_b");
                Collider2D floorA1 = CreateOwnedFloorCollider(buildingA, registryA, "instance_a", 1);
                Collider2D floorA2 = CreateOwnedFloorCollider(buildingA, registryA, "instance_a", 2);
                Collider2D floorB2 = CreateOwnedFloorCollider(buildingB, registryB, "instance_b", 2);
                GeneratedRuntimeIdentity triggerA = CreateOwnedFloorTrigger(
                    buildingA, registryA, "instance_a", 1
                );
                GeneratedRuntimeIdentity triggerB = CreateOwnedFloorTrigger(
                    buildingB, registryB, "instance_b", 1
                );
                registryA.CompleteRebuild(buildingA.transform);
                registryB.CompleteRebuild(buildingB.transform);

                CircleCollider2D unitCollider = unit.AddComponent<CircleCollider2D>();
                GeneratedFloorIsolationAgent floorAgent =
                    unit.AddComponent<GeneratedFloorIsolationAgent>();
                GeneratedVisibilityAgent visibilityAgent =
                    unit.AddComponent<GeneratedVisibilityAgent>();
                GeneratedBuildingContextAgent contextAgent =
                    unit.AddComponent<GeneratedBuildingContextAgent>();

                floorAgent.SetCurrentFloorIndex(1);
                contextAgent.SetBuildingContext(registryA);

                Assert.That(contextAgent.CurrentRegistry, Is.SameAs(registryA));
                Assert.That(floorAgent.BuildingRegistry, Is.SameAs(registryA));
                Assert.That(visibilityAgent.BuildingRegistry, Is.SameAs(registryA));
                Assert.That(Physics2D.GetIgnoreCollision(unitCollider, floorA1), Is.False);
                Assert.That(Physics2D.GetIgnoreCollision(unitCollider, floorA2), Is.True);
                Assert.That(Physics2D.GetIgnoreCollision(unitCollider, floorB2), Is.False);

                contextAgent.SetBuildingContext(registryB);

                Assert.That(Physics2D.GetIgnoreCollision(unitCollider, floorA2), Is.False);
                Assert.That(Physics2D.GetIgnoreCollision(unitCollider, floorB2), Is.True);
                Assert.That(visibilityAgent.BuildingRegistry, Is.SameAs(registryB));

                contextAgent.ClearBuildingContext();

                Assert.That(contextAgent.CurrentRegistry, Is.Null);
                Assert.That(floorAgent.BuildingRegistry, Is.Null);
                Assert.That(visibilityAgent.BuildingRegistry, Is.Null);
                Assert.That(Physics2D.GetIgnoreCollision(unitCollider, floorB2), Is.False);

                Assert.That(contextAgent.NotifyTriggerEnter(triggerA), Is.True);
                Assert.That(contextAgent.CurrentRegistry, Is.SameAs(registryA));
                Assert.That(contextAgent.NotifyTriggerEnter(triggerB), Is.False);
                Assert.That(contextAgent.CurrentRegistry, Is.SameAs(registryA));
                Assert.That(contextAgent.NotifyTriggerExit(triggerA), Is.True);
                Assert.That(contextAgent.CurrentRegistry, Is.Null);
                Assert.That(contextAgent.NotifyTriggerEnter(triggerB), Is.True);
                Assert.That(contextAgent.CurrentRegistry, Is.SameAs(registryB));
            }
            finally
            {
                Object.DestroyImmediate(unit);
                Object.DestroyImmediate(buildingA);
                Object.DestroyImmediate(buildingB);
            }
        }

        private static void ConfigureTestIdentity(
            GeneratedRuntimeIdentity identity,
            GeneratedBuildingRuntimeRegistry registry,
            string instanceId,
            int revision
        )
        {
            identity.Configure(
                instanceId,
                "building_a",
                "profile_a",
                "collider_a",
                "building_a",
                GeneratedRuntimeObjectKind.FloorGroundTrigger,
                1,
                0,
                new RectInt(0, 0, 2, 2),
                true,
                true,
                revision,
                registry
            );
        }

        private static GeneratedBuildingRuntimeRegistry CreateTestRegistry(
            GameObject building,
            string instanceId
        )
        {
            building.AddComponent<GeneratedVisibilityController>();
            GeneratedBuildingRuntimeRegistry registry =
                building.AddComponent<GeneratedBuildingRuntimeRegistry>();
            registry.BeginRebuild(instanceId, "building", "profile");
            return registry;
        }

        private static Collider2D CreateOwnedFloorCollider(
            GameObject building,
            GeneratedBuildingRuntimeRegistry registry,
            string instanceId,
            int floorIndex
        )
        {
            GameObject colliderObject = new GameObject($"Floor {floorIndex} Collider");
            colliderObject.transform.SetParent(building.transform);
            BoxCollider2D collider = colliderObject.AddComponent<BoxCollider2D>();
            GeneratedRuntimeIdentity identity =
                colliderObject.AddComponent<GeneratedRuntimeIdentity>();
            identity.Configure(
                instanceId,
                "building",
                "profile",
                $"floor_{floorIndex}_collider",
                $"floor_{floorIndex}",
                GeneratedRuntimeObjectKind.FloorUpperWallCollider,
                floorIndex,
                0,
                new RectInt(0, 0, 2, 2),
                true,
                false,
                registry.BindingRevision,
                registry
            );
            return collider;
        }

        private static GeneratedRuntimeIdentity CreateOwnedFloorTrigger(
            GameObject building,
            GeneratedBuildingRuntimeRegistry registry,
            string instanceId,
            int floorIndex
        )
        {
            GameObject triggerObject = new GameObject($"Floor {floorIndex} Trigger");
            triggerObject.transform.SetParent(building.transform);
            BoxCollider2D collider = triggerObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            GeneratedRuntimeIdentity identity =
                triggerObject.AddComponent<GeneratedRuntimeIdentity>();
            identity.Configure(
                instanceId,
                "building",
                "profile",
                $"floor_{floorIndex}_trigger",
                $"floor_{floorIndex}",
                GeneratedRuntimeObjectKind.FloorGroundTrigger,
                floorIndex,
                0,
                new RectInt(0, 0, 2, 2),
                true,
                true,
                registry.BindingRevision,
                registry
            );
            return identity;
        }

        private static void AssertOuterCornerCoverage(
            GeneratedFloorData floor,
            GeneratedBuildingProfile profile,
            int seed
        )
        {
            Assert.That(floor, Is.Not.Null, BuildContext(profile, seed, "floor is null"));

            Dictionary<GeneratedWallSide, Vector2Int> expectedCorners =
                new Dictionary<GeneratedWallSide, Vector2Int>
                {
                    { GeneratedWallSide.North, new Vector2Int(floor.Bounds.xMax - 1, floor.Bounds.yMax - 1) },
                    { GeneratedWallSide.West, new Vector2Int(floor.Bounds.xMin, floor.Bounds.yMax - 1) },
                    { GeneratedWallSide.East, new Vector2Int(floor.Bounds.xMax - 1, floor.Bounds.yMin) },
                    { GeneratedWallSide.South, new Vector2Int(floor.Bounds.xMin, floor.Bounds.yMin) }
                };
            int matchedCorners = 0;

            for (int wallIndex = 0; wallIndex < floor.OuterWalls.Count; wallIndex++)
            {
                GeneratedWallData wall = floor.OuterWalls[wallIndex];

                if (wall == null ||
                    string.IsNullOrEmpty(wall.WallId) ||
                    !wall.WallId.Contains("_corner_"))
                {
                    continue;
                }

                Assert.That(wall.CellCount, Is.EqualTo(1), BuildContext(profile, seed, wall.WallId));
                Assert.That(expectedCorners.ContainsKey(wall.Side), Is.True, BuildContext(profile, seed, wall.WallId));
                Assert.That(
                    wall.Cells[0].Position,
                    Is.EqualTo(expectedCorners[wall.Side]),
                    BuildContext(profile, seed, wall.WallId)
                );
                matchedCorners++;
            }

            Assert.That(
                matchedCorners,
                Is.EqualTo(4),
                BuildContext(profile, seed, $"{floor.FloorId} outer corner count")
            );
        }

        private static GeneratedBuildingProfile LoadProfile(string profilePath)
        {
            GeneratedBuildingProfile profile = AssetDatabase.LoadAssetAtPath<GeneratedBuildingProfile>(profilePath);
            Assert.That(profile, Is.Not.Null, $"Profile asset is missing: {profilePath}");
            return profile;
        }

        private static GeneratedBuildingProfile LoadFirstStairProfile()
        {
            List<string> profilePaths = GetProfilePaths();

            for (int i = 0; i < profilePaths.Count; i++)
            {
                GeneratedBuildingProfile profile = LoadProfile(profilePaths[i]);

                if (profile.GenerateStairs && profile.FloorCount > 1)
                {
                    return profile;
                }
            }

            Assert.Fail("A multi-floor stair profile is required for the stair ownership regression test.");
            return null;
        }

        private static List<string> GetProfilePaths()
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:GeneratedBuildingProfile",
                new[] { ProfileRoot }
            );
            List<string> paths = new List<string>(guids.Length);

            for (int i = 0; i < guids.Length; i++)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
            }

            paths.Sort(System.StringComparer.Ordinal);
            return paths;
        }

        private static string BuildContext(
            GeneratedBuildingProfile profile,
            int seed,
            string message
        )
        {
            string profileId = profile != null ? profile.BuildingId : "null";
            return $"profile={profileId}, seed={seed}: {message}";
        }
    }
}
