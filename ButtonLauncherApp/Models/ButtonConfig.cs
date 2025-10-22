using System;
using System.Text.Json.Serialization;

namespace ButtonLauncherApp.Models;

public sealed class ButtonConfig
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("action_type")]
    public string ActionType { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("confirm")]
    public string Confirm { get; set; } = "none";

    [JsonPropertyName("run_as_admin")]
    public bool RunAsAdmin { get; set; }
        = false;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    public bool TryGetActionType(out ButtonActionType actionType)
    {
        actionType = ActionType.ToLowerInvariant() switch
        {
            "open_explorer" => ButtonActionType.OpenExplorer,
            "run_exe_bat" => ButtonActionType.RunExeBat,
            "run_ps1" => ButtonActionType.RunPs1,
            "copy_clipboard" => ButtonActionType.CopyClipboard,
            "open_url_firefox" => ButtonActionType.OpenUrlFirefox,
            _ => ButtonActionType.OpenExplorer
        };

        return ActionType.Equals("open_explorer", StringComparison.OrdinalIgnoreCase)
            || ActionType.Equals("run_exe_bat", StringComparison.OrdinalIgnoreCase)
            || ActionType.Equals("run_ps1", StringComparison.OrdinalIgnoreCase)
            || ActionType.Equals("copy_clipboard", StringComparison.OrdinalIgnoreCase)
            || ActionType.Equals("open_url_firefox", StringComparison.OrdinalIgnoreCase);
    }

    public bool RequiresConfirmation => Confirm.Equals("yes_no_dialog", StringComparison.OrdinalIgnoreCase);
}
