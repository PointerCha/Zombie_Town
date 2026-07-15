using UnityEngine;

public class GeneratedBuildingDataView : MonoBehaviour
{
    [Header("Runtime Data Preview")]
    [SerializeField] private GeneratedBuildingData buildingData = new GeneratedBuildingData();
    [SerializeField] private bool retainInPlayModeForDebug;

    public GeneratedBuildingData BuildingData => buildingData;
    public bool HasTransientData => buildingData != null;

    public void SetData(GeneratedBuildingData newBuildingData)
    {
        buildingData = newBuildingData;
    }

    public bool ReleaseTransientData(bool force = false)
    {
        if (buildingData == null || (!force && retainInPlayModeForDebug))
        {
            return false;
        }

        buildingData = null;
        return true;
    }

    public void ReleaseTransientDataIfRuntime()
    {
        if (Application.isPlaying)
        {
            ReleaseTransientData();
        }
    }

    [ContextMenu("Release Transient Generation Data")]
    private void ReleaseTransientDataFromMenu()
    {
        bool released = ReleaseTransientData(true);
        Debug.Log($"{name}: transient generation data released={released}.", this);
    }

    [ContextMenu("Log Building Data Summary")]
    public void LogSummary()
    {
        if (buildingData == null)
        {
            Debug.Log($"{name}: Building data is null.");
            return;
        }

        Debug.Log(buildingData.BuildSummary(), this);
    }

    [ContextMenu("Validate Building Data")]
    public void ValidateCurrentData()
    {
        GeneratedBuildingSceneRoot sceneRoot = GetComponent<GeneratedBuildingSceneRoot>();
        GeneratedBuildingProfile profile = sceneRoot != null ? sceneRoot.Profile : null;
        GeneratedBuildingValidationResult result = GeneratedBuildingValidator.Validate(buildingData, profile);
        result.Log(this);
    }

    [ContextMenu("Rebuild Instance Data")]
    public void RebuildInstanceData()
    {
        GeneratedBuildingSceneRoot sceneRoot = GetComponent<GeneratedBuildingSceneRoot>();
        GeneratedBuildingProfile profile = sceneRoot != null ? sceneRoot.Profile : null;
        GeneratedBuildingInstanceDataView instanceView = GetComponent<GeneratedBuildingInstanceDataView>();

        if (instanceView == null)
        {
            instanceView = gameObject.AddComponent<GeneratedBuildingInstanceDataView>();
        }

        instanceView.RebuildFromGeneratedData(buildingData, profile);
        instanceView.LogSummary();
    }
}
