using UnityEngine;

public static class GeneratedGenerationRunSupport
{
    public static bool TryResolveReferences(
        MonoBehaviour owner,
        ref GeneratedBuildingSceneRoot sceneRoot,
        ref GeneratedBuildingDataView dataView,
        ref GeneratedBuildingInstanceDataView instanceDataView
    )
    {
        if (owner == null)
        {
            return false;
        }

        GameObject ownerObject = owner.gameObject;

        if (sceneRoot == null)
        {
            sceneRoot = owner.GetComponent<GeneratedBuildingSceneRoot>();
        }

        if (dataView == null)
        {
            dataView = owner.GetComponent<GeneratedBuildingDataView>();
        }

        if (dataView == null)
        {
            dataView = ownerObject.AddComponent<GeneratedBuildingDataView>();
        }

        if (instanceDataView == null)
        {
            instanceDataView = owner.GetComponent<GeneratedBuildingInstanceDataView>();
        }

        if (instanceDataView == null)
        {
            instanceDataView = ownerObject.AddComponent<GeneratedBuildingInstanceDataView>();
        }

        if (sceneRoot == null)
        {
            Debug.LogError($"{owner.name}: GeneratedBuildingSceneRoot is missing.", owner);
            return false;
        }

        if (sceneRoot.FloorsRoot == null)
        {
            Debug.LogError($"{owner.name}: Floors Root is missing.", owner);
            return false;
        }

        return true;
    }

    public static bool TryValidateSceneRoot(
        MonoBehaviour owner,
        GeneratedBuildingSceneRoot sceneRoot
    )
    {
        if (owner == null)
        {
            return false;
        }

        if (sceneRoot == null)
        {
            Debug.LogError($"{owner.name}: GeneratedBuildingSceneRoot is missing.", owner);
            return false;
        }

        if (!sceneRoot.IsValid(out string sceneError))
        {
            Debug.LogError($"{owner.name}: scene root is invalid. {sceneError}", owner);
            return false;
        }

        return true;
    }

    public static void RebuildInstanceAndValidate(
        MonoBehaviour owner,
        GeneratedBuildingSceneRoot sceneRoot,
        GeneratedBuildingInstanceDataView instanceDataView,
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile
    )
    {
        if (instanceDataView != null)
        {
            instanceDataView.RebuildFromGeneratedData(buildingData, profile);
            instanceDataView.LogSummary();
        }

        GeneratedBuildingValidationResult result = GeneratedBuildingValidator.Validate(buildingData, profile);
        result.Log(owner);

        GeneratedBuildingValidationResult runtimeResult = GeneratedRuntimeSceneValidator.Validate(sceneRoot, profile);
        runtimeResult.Log(owner);
    }

    public static void ValidateSeedReproducibility(
        MonoBehaviour owner,
        GeneratedBuildingProfile profile,
        int originalSeed,
        int seedTestDifferentSeedOffset,
        System.Func<GeneratedBuildingProfile, int, GeneratedBuildingData> buildFullBuildingData
    )
    {
        if (owner == null || profile == null || buildFullBuildingData == null)
        {
            return;
        }

        int differentSeed = originalSeed + Mathf.Max(1, seedTestDifferentSeedOffset);

        GeneratedBuildingData first = buildFullBuildingData(profile, originalSeed);
        GeneratedBuildingData second = buildFullBuildingData(profile, originalSeed);
        GeneratedBuildingData different = buildFullBuildingData(profile, differentSeed);

        GeneratedBuildingValidationResult firstResult = GeneratedBuildingValidator.Validate(first, profile);
        GeneratedBuildingValidationResult secondResult = GeneratedBuildingValidator.Validate(second, profile);
        GeneratedBuildingValidationResult differentResult = GeneratedBuildingValidator.Validate(different, profile);

        int firstHash = first.BuildDeterministicHash();
        int secondHash = second.BuildDeterministicHash();
        int differentHash = different.BuildDeterministicHash();
        bool sameSeedMatches = firstHash == secondHash &&
                               first.BuildDeterministicFingerprint() == second.BuildDeterministicFingerprint();
        bool differentSeedChanges = firstHash != differentHash ||
                                    first.BuildDeterministicFingerprint() != different.BuildDeterministicFingerprint();

        if (!sameSeedMatches)
        {
            Debug.LogError(
                $"{owner.name}: seed reproducibility failed. Same seed generated different data. " +
                $"seed={originalSeed}, firstHash={firstHash}, secondHash={secondHash}",
                owner
            );
            return;
        }

        if (!firstResult.IsValid || !secondResult.IsValid || !differentResult.IsValid)
        {
            Debug.LogError(
                $"{owner.name}: seed validation failed. " +
                $"firstValid={firstResult.IsValid}, secondValid={secondResult.IsValid}, differentValid={differentResult.IsValid}",
                owner
            );
            return;
        }

        Debug.Log(
            $"{owner.name}: seed reproducibility passed.\n" +
            $"- Same seed: {originalSeed}\n" +
            $"- Same seed hash: {firstHash}\n" +
            $"- Different seed: {differentSeed}\n" +
            $"- Different seed hash: {differentHash}\n" +
            $"- Different seed changed data: {differentSeedChanges}\n" +
            $"- Validation: passed",
            owner
        );
    }
}
