#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class GeneratedBuildingFamilyValidationRunner
{
    private const string ProfileRoot = "Assets/___Test/Data/Profiles";

    public static void Run(
        MonoBehaviour owner,
        GeneratedBuildingSceneRoot sceneRoot,
        IReadOnlyList<GeneratedBuildingProfile> explicitProfiles,
        int seed,
        int differentSeedOffset,
        Func<GeneratedBuildingProfile, GeneratedBuildingData> generateScene
    )
    {
        List<GeneratedBuildingProfile> profiles = CollectProfiles(sceneRoot, explicitProfiles);

        if (profiles.Count <= 0)
        {
            Debug.LogError($"{owner.name}: no building profiles found for family validation matrix.", owner);
            return;
        }

        StringBuilder builder = new StringBuilder();
        int passCount = 0;
        int failCount = 0;

        builder.AppendLine("Generated Building Family Validation Matrix");
        builder.AppendLine($"- Seed: {seed}");
        builder.AppendLine($"- Different Seed Offset: {differentSeedOffset}");
        builder.AppendLine($"- Profiles: {profiles.Count}");

        for (int i = 0; i < profiles.Count; i++)
        {
            GeneratedBuildingProfile profile = profiles[i];
            GeneratedBuildingData buildingData = generateScene(profile);

            if (buildingData == null)
            {
                failCount++;
                builder.AppendLine($"  [{i}] {profile.name}: generation failed");
                continue;
            }

            GeneratedBuildingValidationResult dataResult = GeneratedBuildingValidator.Validate(buildingData, profile);
            GeneratedBuildingValidationResult runtimeResult = GeneratedRuntimeSceneValidator.Validate(sceneRoot, profile);
            bool seedValid = ValidateSeedReproducibility(
                profile,
                seed,
                differentSeedOffset,
                out string seedMessage
            );
            bool instanceValid = ValidateInstanceData(buildingData, profile, out string instanceMessage);
            bool passed = dataResult.IsValid && runtimeResult.IsValid && seedValid && instanceValid;

            if (passed)
            {
                passCount++;
            }
            else
            {
                failCount++;
            }

            builder.AppendLine(
                $"  [{i}] {profile.DisplayName} ({profile.BuildingId}): " +
                $"{(passed ? "passed" : "failed")} " +
                $"dataErrors={dataResult.ErrorCount} runtimeErrors={runtimeResult.ErrorCount} " +
                $"seed={(seedValid ? "ok" : "failed")} instance={(instanceValid ? "ok" : "failed")} " +
                $"hash={buildingData.BuildDeterministicHash()}"
            );

            if (!seedValid)
            {
                builder.AppendLine($"       seed: {seedMessage}");
            }

            if (!instanceValid)
            {
                builder.AppendLine($"       instance: {instanceMessage}");
            }
        }

        builder.AppendLine($"- Passed: {passCount}");
        builder.AppendLine($"- Failed: {failCount}");

        if (failCount > 0)
        {
            Debug.LogError(builder.ToString(), owner);
        }
        else
        {
            Debug.Log(builder.ToString(), owner);
        }
    }

    private static List<GeneratedBuildingProfile> CollectProfiles(
        GeneratedBuildingSceneRoot sceneRoot,
        IReadOnlyList<GeneratedBuildingProfile> explicitProfiles
    )
    {
        List<GeneratedBuildingProfile> profiles = new List<GeneratedBuildingProfile>();

        if (explicitProfiles != null)
        {
            for (int i = 0; i < explicitProfiles.Count; i++)
            {
                AddUnique(profiles, explicitProfiles[i]);
            }
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:GeneratedBuildingProfile",
            new[] { ProfileRoot }
        );

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            AddUnique(profiles, AssetDatabase.LoadAssetAtPath<GeneratedBuildingProfile>(path));
        }
#endif

        AddUnique(profiles, sceneRoot != null ? sceneRoot.Profile : null);
        profiles.Sort((left, right) => string.Compare(
            left.BuildingId,
            right.BuildingId,
            StringComparison.Ordinal
        ));
        return profiles;
    }

    private static void AddUnique(
        List<GeneratedBuildingProfile> profiles,
        GeneratedBuildingProfile profile
    )
    {
        if (profile != null && !profiles.Contains(profile))
        {
            profiles.Add(profile);
        }
    }

    private static bool ValidateSeedReproducibility(
        GeneratedBuildingProfile profile,
        int seed,
        int differentSeedOffset,
        out string message
    )
    {
        int differentSeed = seed + Mathf.Max(1, differentSeedOffset);
        GeneratedBuildingData first = GeneratedBuildingDataGenerator.GenerateFull(profile, seed);
        GeneratedBuildingData second = GeneratedBuildingDataGenerator.GenerateFull(profile, seed);
        GeneratedBuildingData different = GeneratedBuildingDataGenerator.GenerateFull(profile, differentSeed);

        GeneratedBuildingValidationResult firstResult = GeneratedBuildingValidator.Validate(first, profile);
        GeneratedBuildingValidationResult secondResult = GeneratedBuildingValidator.Validate(second, profile);
        GeneratedBuildingValidationResult differentResult = GeneratedBuildingValidator.Validate(different, profile);

        bool sameSeedMatches = first.BuildDeterministicHash() == second.BuildDeterministicHash() &&
                               first.BuildDeterministicFingerprint() == second.BuildDeterministicFingerprint();
        bool allValid = firstResult.IsValid && secondResult.IsValid && differentResult.IsValid;

        if (!sameSeedMatches)
        {
            message = $"same seed mismatch. firstHash={first.BuildDeterministicHash()}, secondHash={second.BuildDeterministicHash()}";
            return false;
        }

        if (!allValid)
        {
            message =
                $"validation failed. first={firstResult.ErrorCount}, second={secondResult.ErrorCount}, different={differentResult.ErrorCount}";
            return false;
        }

        message = $"same={first.BuildDeterministicHash()}, different={different.BuildDeterministicHash()}";
        return true;
    }

    private static bool ValidateInstanceData(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile,
        out string message
    )
    {
        GeneratedBuildingInstanceData instanceData = new GeneratedBuildingInstanceData();
        instanceData.Initialize(buildingData, profile);

        if (!instanceData.Footprint.Equals(buildingData.Footprint))
        {
            message = $"footprint mismatch. instance={instanceData.Footprint}, data={buildingData.Footprint}";
            return false;
        }

        if (buildingData.Entrances.Count > 0 && instanceData.PrimaryRoadConnectionCell == Vector2Int.zero)
        {
            message = "primary road connection cell is zero while entrance exists.";
            return false;
        }

        message = "ok";
        return true;
    }
}
