using System.Text.Json.Serialization;

namespace ButtonLauncherApp.Models;

public sealed class MetaConfiguration
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("grid_columns")]
    public int GridColumns { get; set; } = 4;
}
