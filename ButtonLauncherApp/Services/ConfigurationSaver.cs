using System;
using System.IO;
using System.Text.Json;
using ButtonLauncherApp.Models;

namespace ButtonLauncherApp.Services;

public sealed class ConfigurationSaver
{
    private readonly ILogService _logger;

    public ConfigurationSaver(ILogService logger)
    {
        _logger = logger;
    }

    public void Save(string configPath, ButtonLauncherConfiguration configuration)
    {
        try
        {
            var directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(configuration, options);
            File.WriteAllText(configPath, json);
        }
        catch (Exception ex)
        {
            _logger.Error("Fehler beim Speichern der Konfiguration.", ex);
            throw;
        }
    }
}
