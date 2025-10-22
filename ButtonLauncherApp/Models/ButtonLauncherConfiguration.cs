using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ButtonLauncherApp.Models;

public sealed class ButtonLauncherConfiguration
{
    [JsonPropertyName("meta")]
    public MetaConfiguration Meta { get; set; } = new();

    [JsonPropertyName("buttons")]
    public List<ButtonConfig> Buttons { get; set; } = new();
}
