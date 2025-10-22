using System;
using System.IO;
using System.Text.Json;
using ButtonLauncherApp.Models;

namespace ButtonLauncherApp.Services;

public sealed class ConfigurationLoader
{
    private readonly ILogService _logger;

    public ConfigurationLoader(ILogService logger)
    {
        _logger = logger;
    }

    public ButtonLauncherConfiguration Load(string configPath)
    {
        try
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Konfigurationsdatei '{configPath}' wurde nicht gefunden.");
            }

            var json = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var configuration = JsonSerializer.Deserialize<ButtonLauncherConfiguration>(json, options);
            if (configuration is null)
            {
                throw new InvalidOperationException("Konfiguration konnte nicht geladen werden.");
            }

            return configuration;
        }
        catch (Exception ex)
        {
            _logger.Error("Fehler beim Laden der Konfiguration.", ex);
            throw;
        }
    }
}
