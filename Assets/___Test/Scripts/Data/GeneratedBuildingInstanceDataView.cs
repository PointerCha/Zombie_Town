using UnityEngine;

public class GeneratedBuildingInstanceDataView : MonoBehaviour
{
    [Header("City Placement Data Preview")]
    [SerializeField] private GeneratedBuildingInstanceData instanceData = new GeneratedBuildingInstanceData();

    public GeneratedBuildingInstanceData InstanceData => instanceData;

    public void SetData(GeneratedBuildingInstanceData newInstanceData)
    {
        instanceData = newInstanceData;
    }

    public void RebuildFromGeneratedData(
        GeneratedBuildingData buildingData,
        GeneratedBuildingProfile profile
    )
    {
        if (instanceData == null)
        {
            instanceData = new GeneratedBuildingInstanceData();
        }

        instanceData.Initialize(buildingData, profile);
    }

    [ContextMenu("Log Instance Data Summary")]
    public void LogSummary()
    {
        if (instanceData == null)
        {
            Debug.Log($"{name}: Instance data is null.", this);
            return;
        }

        Debug.Log(instanceData.BuildSummary(), this);
    }
}
