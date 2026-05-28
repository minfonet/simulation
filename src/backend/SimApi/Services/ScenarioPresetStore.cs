namespace SimApi.Services;

public class ScenarioPresetStore
{
    private static readonly List<ScenarioPresetInfo> Presets = new()
    {
        new ScenarioPresetInfo("default", "Default", "Base driving course with straight road, curves, and obstacles", "res://Scenes/Main.tscn")
    };

    public IReadOnlyList<ScenarioPresetInfo> GetAll() => Presets.AsReadOnly();
    public bool IsValid(string presetId) => Presets.Any(p => p.Id == presetId);
    public ScenarioPresetInfo? GetById(string presetId) => Presets.FirstOrDefault(p => p.Id == presetId);
}

public record ScenarioPresetInfo(string Id, string Name, string Description, string GodotScenePath);
