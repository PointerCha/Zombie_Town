using UnityEngine;
using System;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine.Tilemaps;

public class GeneratedGroundGenerator : MonoBehaviour
{
    private const int PerBuildingTilemapBudget = 64;
    private const int PerBuildingColliderBudget = 64;
    private const int PerBuildingActiveObjectBudget = 256;
    private const double VisibilityUpdateBudgetMilliseconds = 0.25d;
    private const int ActiveCityChunkBuildingBudget = 16;

    [Header("Scene Binding")]
    [SerializeField] private GeneratedBuildingSceneRoot sceneRoot;
    [SerializeField] private GeneratedBuildingDataView dataView;
    [SerializeField] private GeneratedBuildingInstanceDataView instanceDataView;

    [Header("Generation")]
    [SerializeField] private int seed = 1001;
    [SerializeField] private bool clearBeforeGenerate = true;
    [SerializeField] private bool generateOnStart;
    [SerializeField] private int seedTestDifferentSeedOffset = 1;
    [SerializeField] private GeneratedBuildingProfile[] validationMatrixProfiles;

    [Header("27E-B Visual Acceptance")]
    [SerializeField] private int[] visualAcceptanceSeeds = { 1001, 2027 };
    [SerializeField] private int visualAcceptanceProfileIndex;
    [SerializeField] private int visualAcceptanceSeedIndex;
    [SerializeField] private List<string> acceptedVisualSamples = new List<string>();

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateGroundOnly();
        }
    }

    [ContextMenu("Generate Ground Only")]
    public void GenerateGroundOnly()
    {
        if (!TryResolveReferences())
        {
            return;
        }

        if (!GeneratedGenerationRunSupport.TryValidateSceneRoot(this, sceneRoot))
        {
            return;
        }

        GeneratedBuildingProfile profile = sceneRoot.Profile;
        ApplyFloorVisualOffsets(profile);
        GeneratedBuildingData buildingData = BuildGroundData(profile);
        dataView.SetData(buildingData);
        GeneratedBuildingSceneRenderer.RenderFloors(
            sceneRoot,
            buildingData,
            profile,
            clearBeforeGenerate,
            false,
            false
        );

        dataView.LogSummary();
    }

    [ContextMenu("Generate Building Shell")]
    public void GenerateBuildingShell()
    {
        if (!TryResolveReferences())
        {
            return;
        }

        if (!TryResolveWallReferences())
        {
            return;
        }

        if (!GeneratedGenerationRunSupport.TryValidateSceneRoot(this, sceneRoot))
        {
            return;
        }

        GeneratedBuildingProfile profile = sceneRoot.Profile;
        ApplyFloorVisualOffsets(profile);
        GeneratedBuildingData buildingData = BuildGroundData(profile);
        GeneratedBuildingDataGenerator.AddOuterWalls(buildingData);
        GeneratedEntrancePlanner.AddMainEntrance(buildingData, profile);
        dataView.SetData(buildingData);

        GeneratedBuildingSceneRenderer.RenderFloors(
            sceneRoot,
            buildingData,
            profile,
            clearBeforeGenerate,
            true,
            false
        );

        dataView.LogSummary();
    }

    [ContextMenu("Generate Building With Rooms")]
    public void GenerateBuildingWithRooms()
    {
        if (!TryResolveReferences())
        {
            return;
        }

        if (!TryResolveWallReferences())
        {
            return;
        }

        if (!GeneratedGenerationRunSupport.TryValidateSceneRoot(this, sceneRoot))
        {
            return;
        }

        GeneratedBuildingProfile profile = sceneRoot.Profile;
        ApplyFloorVisualOffsets(profile);
        GeneratedBuildingData buildingData = BuildGroundData(profile);
        GeneratedBuildingDataGenerator.AddOuterWalls(buildingData);
        GeneratedEntrancePlanner.AddMainEntrance(buildingData, profile);
        GeneratedRoomPlanner.AddRooms(buildingData, profile);
        dataView.SetData(buildingData);

        GeneratedBuildingSceneRenderer.RenderFloors(
            sceneRoot,
            buildingData,
            profile,
            clearBeforeGenerate,
            true,
            true
        );

        dataView.LogSummary();
    }

    [ContextMenu("Generate Building With Colliders")]
    public void GenerateBuildingWithColliders()
    {
        if (!TryResolveReferences())
        {
            return;
        }

        if (!TryResolveWallReferences())
        {
            return;
        }

        if (!GeneratedGenerationRunSupport.TryValidateSceneRoot(this, sceneRoot))
        {
            return;
        }

        GeneratedBuildingProfile profile = sceneRoot.Profile;
        ApplyFloorVisualOffsets(profile);
        GeneratedBuildingData buildingData = BuildGroundData(profile);
        GeneratedBuildingDataGenerator.AddOuterWalls(buildingData);
        GeneratedEntrancePlanner.AddMainEntrance(buildingData, profile);
        GeneratedRoomPlanner.AddRooms(buildingData, profile);
        dataView.SetData(buildingData);

        GeneratedBuildingSceneRenderer.RenderFloors(
            sceneRoot,
            buildingData,
            profile,
            clearBeforeGenerate,
            true,
            true
        );

        GeneratedColliderRenderer.PaintBuildingColliders(
            sceneRoot,
            buildingData,
            profile,
            clearBeforeGenerate
        );

        GeneratedCeilingRenderer.PaintCeiling(
            sceneRoot,
            buildingData,
            profile,
            clearBeforeGenerate
        );

        GeneratedVisibilityBinder.Bind(sceneRoot, buildingData);
        GeneratedRuntimeBinder.Bind(sceneRoot, buildingData, profile);

        GeneratedGenerationRunSupport.RebuildInstanceAndValidate(
            this,
            sceneRoot,
            instanceDataView,
            buildingData,
            profile
        );
        dataView.LogSummary();
        dataView.ReleaseTransientDataIfRuntime();
    }

    [ContextMenu("Generate Building With Stairs")]
    public void GenerateBuildingWithStairs()
    {
        if (!TryResolveReferences())
        {
            return;
        }

        if (!TryResolveWallReferences())
        {
            return;
        }

        if (!GeneratedGenerationRunSupport.TryValidateSceneRoot(this, sceneRoot))
        {
            return;
        }

        GeneratedBuildingProfile profile = sceneRoot.Profile;
        GeneratedBuildingData buildingData = GenerateFullBuildingForProfile(profile);

        if (buildingData == null)
        {
            return;
        }

        GeneratedGenerationRunSupport.RebuildInstanceAndValidate(
            this,
            sceneRoot,
            instanceDataView,
            buildingData,
            profile
        );
        dataView.LogSummary();
        dataView.ReleaseTransientDataIfRuntime();
    }

    [ContextMenu("Validate Building Family Matrix")]
    public void ValidateBuildingFamilyMatrix()
    {
        if (!TryResolveReferences())
        {
            return;
        }

        if (!TryResolveWallReferences())
        {
            return;
        }

        if (!GeneratedGenerationRunSupport.TryValidateSceneRoot(this, sceneRoot))
        {
            return;
        }

        GeneratedBuildingFamilyValidationRunner.Run(
            this,
            sceneRoot,
            validationMatrixProfiles,
            seed,
            seedTestDifferentSeedOffset,
            GenerateFullBuildingForProfile
        );
    }

    private GeneratedBuildingData GenerateFullBuildingForProfile(GeneratedBuildingProfile profile)
    {
        if (profile == null)
        {
            Debug.LogError($"{name}: profile is missing.", this);
            return null;
        }

        if (!profile.IsValid(out string profileError))
        {
            Debug.LogError($"{name}: profile is invalid. {profileError}", this);
            return null;
        }

        ApplyFloorVisualOffsets(profile);
        GeneratedBuildingData buildingData = BuildGroundData(profile);
        GeneratedBuildingDataGenerator.AddOuterWalls(buildingData);
        GeneratedStairPlanner.AddStairs(buildingData, profile);
        GeneratedEntrancePlanner.AddMainEntrance(buildingData, profile);
        GeneratedRoomPlanner.AddRooms(buildingData, profile);
        dataView.SetData(buildingData);

        GeneratedBuildingSceneRenderer.RenderCompleteBuilding(
            sceneRoot,
            buildingData,
            profile,
            clearBeforeGenerate
        );
        return buildingData;
    }

    [ContextMenu("Profile 27D-A Rendering")]
    public void ProfileStage27DARendering()
    {
        if (!TryResolveReferences() || validationMatrixProfiles == null || validationMatrixProfiles.Length == 0)
        {
            UnityEngine.Debug.LogError(
                $"{name}: assign Small, Large, and other profiles to Validation Matrix Profiles first.",
                this
            );
            return;
        }

        GeneratedBuildingProfile originalProfile = sceneRoot.Profile;

        try
        {
            ProfileRenderingPass("Legacy cell-by-cell", false);
            ProfileRenderingPass("27D-A batched", true);
        }
        finally
        {
            GeneratedTilemapUtility.UseBatchedPainting = true;

            if (originalProfile != null)
            {
                GenerateFullBuildingForProfile(originalProfile);
            }
        }
    }

    private void ProfileRenderingPass(string label, bool useBatchedPainting)
    {
        GeneratedTilemapUtility.UseBatchedPainting = useBatchedPainting;
        Stopwatch totalWatch = Stopwatch.StartNew();
        long totalAllocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int completed = 0;

        for (int i = 0; i < validationMatrixProfiles.Length; i++)
        {
            GeneratedBuildingProfile profile = validationMatrixProfiles[i];

            if (profile == null)
            {
                continue;
            }

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            Stopwatch watch = Stopwatch.StartNew();
            GenerateFullBuildingForProfile(profile);
            watch.Stop();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            completed++;

            UnityEngine.Debug.Log(
                $"27D-A {label}: profile={profile.BuildingId}, floors={profile.FloorCount}, " +
                $"footprint={profile.FootprintSize.x}x{profile.FootprintSize.y}, " +
                $"elapsedMs={watch.Elapsed.TotalMilliseconds:F3}, allocatedBytes={allocated:N0}",
                this
            );
        }

        totalWatch.Stop();
        long totalAllocated = GC.GetAllocatedBytesForCurrentThread() - totalAllocatedBefore;
        UnityEngine.Debug.Log(
            $"27D-A {label} multi-building batch: buildings={completed}, " +
            $"elapsedMs={totalWatch.Elapsed.TotalMilliseconds:F3}, allocatedBytes={totalAllocated:N0}",
            this
        );
    }

    [ContextMenu("Profile 27D-B Runtime Budgets")]
    public void ProfileStage27DBRuntimeBudgets()
    {
        if (!TryResolveReferences() || validationMatrixProfiles == null || validationMatrixProfiles.Length == 0)
        {
            Debug.LogError($"{name}: Validation Matrix Profiles are required for 27D-B profiling.", this);
            return;
        }

        GeneratedBuildingProfile originalProfile = sceneRoot.Profile;
        long batchReleasedBytes = 0L;
        int batchTilemaps = 0;
        int batchColliders = 0;
        int batchActiveObjects = 0;

        try
        {
            for (int i = 0; i < validationMatrixProfiles.Length; i++)
            {
                GeneratedBuildingProfile profile = validationMatrixProfiles[i];

                if (profile == null)
                {
                    continue;
                }

                GeneratedBuildingData generatedData = GenerateFullBuildingForProfile(profile);
                instanceDataView.RebuildFromGeneratedData(generatedData, profile);
                long retainedBefore = GC.GetTotalMemory(true);
                dataView.ReleaseTransientData(true);
                generatedData = null;
                long retainedAfter = GC.GetTotalMemory(true);
                long releasedBytes = Math.Max(0L, retainedBefore - retainedAfter);
                RuntimeBudgetCounts counts = CountRuntimeBudgetObjects();
                double visibilityMilliseconds = MeasureVisibilityUpdateCost(200);

                batchReleasedBytes += releasedBytes;
                batchTilemaps += counts.Tilemaps;
                batchColliders += counts.Colliders;
                batchActiveObjects += counts.ActiveObjects;

                bool withinBudget = counts.Tilemaps <= PerBuildingTilemapBudget &&
                                    counts.Colliders <= PerBuildingColliderBudget &&
                                    counts.ActiveObjects <= PerBuildingActiveObjectBudget &&
                                    visibilityMilliseconds <= VisibilityUpdateBudgetMilliseconds;
                string budgetMessage =
                    $"27D-B runtime budget: profile={profile.BuildingId}, releasedTransientBytes={releasedBytes:N0}, " +
                    $"retainedManagedSnapshotBytes={retainedAfter:N0}, tilemaps={counts.Tilemaps}, " +
                    $"colliders={counts.Colliders}, activeObjects={counts.ActiveObjects}, " +
                    $"visibilityUpdateAverageMs={visibilityMilliseconds:F4}, withinBudget={withinBudget}";

                if (withinBudget)
                {
                    Debug.Log(budgetMessage, this);
                }
                else
                {
                    Debug.LogError(budgetMessage, this);
                }
            }

            Debug.Log(
                $"27D-B measured matrix totals: buildings={validationMatrixProfiles.Length}, " +
                $"releasedTransientBytes={batchReleasedBytes:N0}, tilemaps={batchTilemaps}, " +
                $"colliders={batchColliders}, activeObjects={batchActiveObjects}. " +
                $"Active chunk budgets({ActiveCityChunkBuildingBudget} buildings): " +
                $"tilemaps<={PerBuildingTilemapBudget * ActiveCityChunkBuildingBudget}, " +
                $"colliders<={PerBuildingColliderBudget * ActiveCityChunkBuildingBudget}, " +
                $"activeObjects<={PerBuildingActiveObjectBudget * ActiveCityChunkBuildingBudget}",
                this
            );
        }
        finally
        {
            if (originalProfile != null)
            {
                GeneratedBuildingData restored = GenerateFullBuildingForProfile(originalProfile);
                instanceDataView.RebuildFromGeneratedData(restored, originalProfile);
            }
        }
    }

    private RuntimeBudgetCounts CountRuntimeBudgetObjects()
    {
        Transform root = sceneRoot.GeneratedHouseRoot;
        Tilemap[] tilemaps = root != null ? root.GetComponentsInChildren<Tilemap>(true) : Array.Empty<Tilemap>();
        Collider2D[] colliders = root != null ? root.GetComponentsInChildren<Collider2D>(true) : Array.Empty<Collider2D>();
        Transform[] transforms = root != null ? root.GetComponentsInChildren<Transform>(true) : Array.Empty<Transform>();
        int activeObjects = 0;

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && transforms[i].gameObject.activeInHierarchy)
            {
                activeObjects++;
            }
        }

        return new RuntimeBudgetCounts(tilemaps.Length, colliders.Length, activeObjects);
    }

    private double MeasureVisibilityUpdateCost(int iterations)
    {
        GeneratedVisibilityController controller = sceneRoot.GeneratedHouseRoot != null
            ? sceneRoot.GeneratedHouseRoot.GetComponent<GeneratedVisibilityController>()
            : null;

        if (controller == null || iterations <= 0)
        {
            return 0d;
        }

        Stopwatch watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            controller.SetCurrentFloorVisuals((i & 1) + 1);
        }

        watch.Stop();
        controller.ShowAllFloorVisuals();
        return watch.Elapsed.TotalMilliseconds / iterations;
    }

    private readonly struct RuntimeBudgetCounts
    {
        public int Tilemaps { get; }
        public int Colliders { get; }
        public int ActiveObjects { get; }

        public RuntimeBudgetCounts(int tilemaps, int colliders, int activeObjects)
        {
            Tilemaps = tilemaps;
            Colliders = colliders;
            ActiveObjects = activeObjects;
        }
    }

    [ContextMenu("27E-B Prepare Current Visual Sample")]
    public void PrepareCurrentVisualAcceptanceSample()
    {
        if (!TryGetCurrentVisualSample(out GeneratedBuildingProfile profile, out int sampleSeed))
        {
            return;
        }

        seed = sampleSeed;
        sceneRoot.SetProfile(profile);
        MarkVisualAcceptanceDirty();
        GenerateBuildingWithStairs();
        sceneRoot.CreateOrResetTestUnit();
        GeneratedBuildingData data = dataView.BuildingData;
        string key = BuildVisualSampleKey(profile, sampleSeed);
        string hash = data != null ? data.BuildDeterministicHash().ToString() : "unavailable";

        Debug.Log(
            $"27E-B visual sample ready: {key}, hash={hash}\n" +
            "Inspect: (1) four outer corners use corner tiles, " +
            "(2) room/entrance openings have no wall overlap, " +
            "(3) stairs and rails align, (4) ceiling sits on the top floor, " +
            "(5) isometric sorting has no incorrect overlap, " +
            "(6) entering floor/room fades and restores walls/ceiling, " +
            "(7) camera keeps the Unit and building readable.\n" +
            "If every item passes, run '27E-B Accept Current And Prepare Next'.",
            this
        );
    }

    [ContextMenu("27E-B Accept Current And Prepare Next")]
    public void AcceptCurrentVisualSampleAndPrepareNext()
    {
        if (!TryGetCurrentVisualSample(out GeneratedBuildingProfile profile, out int sampleSeed))
        {
            return;
        }

        GeneratedBuildingData data = dataView.BuildingData;
        GeneratedBuildingValidationResult dataResult = GeneratedBuildingValidator.Validate(data, profile);
        GeneratedBuildingValidationResult runtimeResult = GeneratedRuntimeSceneValidator.Validate(sceneRoot, profile);

        if (!dataResult.IsValid || !runtimeResult.IsValid)
        {
            Debug.LogError(
                $"27E-B cannot accept {BuildVisualSampleKey(profile, sampleSeed)}: " +
                $"dataErrors={dataResult.ErrorCount}, runtimeErrors={runtimeResult.ErrorCount}.",
                this
            );
            return;
        }

        string acceptedKey = BuildVisualSampleKey(profile, sampleSeed);

        if (!acceptedVisualSamples.Contains(acceptedKey))
        {
            acceptedVisualSamples.Add(acceptedKey);
        }

        AdvanceVisualSample();
        MarkVisualAcceptanceDirty();

        if (HasCompletedVisualAcceptance())
        {
            LogVisualAcceptanceSummary();
            return;
        }

        PrepareCurrentVisualAcceptanceSample();
    }

    [ContextMenu("27E-B Log Visual Acceptance Summary")]
    public void LogVisualAcceptanceSummary()
    {
        int required = GetRequiredVisualSampleCount();
        bool complete = required > 0 && acceptedVisualSamples.Count >= required;
        Debug.Log(
            $"27E-B Visual Acceptance Summary\n" +
            $"- Accepted: {acceptedVisualSamples.Count}/{required}\n" +
            $"- Complete: {complete}\n" +
            $"- Samples: {(acceptedVisualSamples.Count > 0 ? string.Join(", ", acceptedVisualSamples) : "none")}",
            this
        );
    }

    [ContextMenu("27E-B Reset Visual Acceptance")]
    public void ResetVisualAcceptance()
    {
        acceptedVisualSamples.Clear();
        visualAcceptanceProfileIndex = 0;
        visualAcceptanceSeedIndex = 0;
        MarkVisualAcceptanceDirty();
        Debug.Log("27E-B visual acceptance reset.", this);
    }

    private bool TryGetCurrentVisualSample(out GeneratedBuildingProfile profile, out int sampleSeed)
    {
        profile = null;
        sampleSeed = 0;

        if (!TryResolveReferences() || validationMatrixProfiles == null || validationMatrixProfiles.Length == 0 ||
            visualAcceptanceSeeds == null || visualAcceptanceSeeds.Length == 0)
        {
            Debug.LogError($"{name}: 27E-B profiles and seeds are required.", this);
            return false;
        }

        visualAcceptanceProfileIndex = Mathf.Clamp(visualAcceptanceProfileIndex, 0, validationMatrixProfiles.Length - 1);
        visualAcceptanceSeedIndex = Mathf.Clamp(visualAcceptanceSeedIndex, 0, visualAcceptanceSeeds.Length - 1);
        profile = validationMatrixProfiles[visualAcceptanceProfileIndex];
        sampleSeed = visualAcceptanceSeeds[visualAcceptanceSeedIndex];

        if (profile == null)
        {
            Debug.LogError($"{name}: 27E-B profile at index {visualAcceptanceProfileIndex} is missing.", this);
            return false;
        }

        return true;
    }

    private void AdvanceVisualSample()
    {
        visualAcceptanceSeedIndex++;

        if (visualAcceptanceSeedIndex < visualAcceptanceSeeds.Length)
        {
            return;
        }

        visualAcceptanceSeedIndex = 0;
        visualAcceptanceProfileIndex = Mathf.Min(
            visualAcceptanceProfileIndex + 1,
            validationMatrixProfiles.Length - 1
        );
    }

    private bool HasCompletedVisualAcceptance()
    {
        return acceptedVisualSamples.Count >= GetRequiredVisualSampleCount();
    }

    private int GetRequiredVisualSampleCount()
    {
        return validationMatrixProfiles != null && visualAcceptanceSeeds != null
            ? validationMatrixProfiles.Length * visualAcceptanceSeeds.Length
            : 0;
    }

    private static string BuildVisualSampleKey(GeneratedBuildingProfile profile, int sampleSeed)
    {
        return $"{(profile != null ? profile.BuildingId : "missing_profile")}|seed={sampleSeed}";
    }

    private void MarkVisualAcceptanceDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    [ContextMenu("Validate Seed Reproducibility")]
    public void ValidateSeedReproducibility()
    {
        if (!TryResolveReferences())
        {
            return;
        }

        if (!GeneratedGenerationRunSupport.TryValidateSceneRoot(this, sceneRoot))
        {
            return;
        }

        GeneratedBuildingProfile profile = sceneRoot.Profile;
        GeneratedGenerationRunSupport.ValidateSeedReproducibility(
            this,
            profile,
            seed,
            seedTestDifferentSeedOffset,
            BuildFullBuildingData
        );
    }


    private bool TryResolveReferences()
    {
        return GeneratedGenerationRunSupport.TryResolveReferences(
            this,
            ref sceneRoot,
            ref dataView,
            ref instanceDataView
        );
    }

    private void ApplyFloorVisualOffsets(GeneratedBuildingProfile profile)
    {
        if (profile == null)
        {
            return;
        }

        sceneRoot.ApplyFloorLocalPositions(profile);

        if (clearBeforeGenerate)
        {
            sceneRoot.ClearUnusedFloorTilemaps(profile.FloorCount);
        }
    }

    private bool TryResolveWallReferences()
    {
        return true;
    }

    private GeneratedBuildingData BuildGroundData(GeneratedBuildingProfile profile)
    {
        return BuildGroundData(profile, seed);
    }

    private GeneratedBuildingData BuildGroundData(GeneratedBuildingProfile profile, int generationSeed)
    {
        return GeneratedBuildingDataGenerator.GenerateGround(profile, generationSeed);
    }

    private GeneratedBuildingData BuildFullBuildingData(
        GeneratedBuildingProfile profile,
        int generationSeed
    )
    {
        return GeneratedBuildingDataGenerator.GenerateFull(profile, generationSeed);
    }

}
