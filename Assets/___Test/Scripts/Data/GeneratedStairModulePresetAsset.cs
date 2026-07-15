using UnityEngine;

[CreateAssetMenu(
    fileName = "NewGeneratedStairModulePreset",
    menuName = "Zombie Town/Test Building Generation/Stair Module Preset"
)]
public class GeneratedStairModulePresetAsset : ScriptableObject
{
    [SerializeField] private GeneratedStairModulePreset preset = new GeneratedStairModulePreset();

    public GeneratedStairModulePreset Preset => preset;

    public bool IsValid(out string errorMessage)
    {
        if (preset == null)
        {
            errorMessage = "Stair module preset data is missing.";
            return false;
        }

        return preset.IsValid(out errorMessage);
    }
}
