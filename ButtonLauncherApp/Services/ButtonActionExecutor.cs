using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ButtonLauncherApp.Models;

namespace ButtonLauncherApp.Services;

public sealed class ButtonActionExecutor
{
    private readonly Window _owner;
    private readonly ILogService _logger;

    public ButtonActionExecutor(Window owner, ILogService logger)
    {
        _owner = owner;
        _logger = logger;
    }

    public void Execute(ButtonConfig config)
    {
        if (!config.TryGetActionType(out var actionType))
        {
            ShowError($"Unbekannter action_type '{config.ActionType}'.");
            return;
        }

        if (config.RequiresConfirmation)
        {
            var result = MessageBox.Show(_owner,
                $"Soll die Aktion '{config.Label}' ausgeführt werden?",
                "Bestätigung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                _logger.Info($"Aktion '{config.Id}' wurde vom Benutzer abgebrochen.");
                return;
            }
        }

        try
        {
            switch (actionType)
            {
                case ButtonActionType.OpenExplorer:
                    ExecuteExplorer(config.Target);
                    break;
                case ButtonActionType.RunExeBat:
                    ExecuteProcess(config.Target, null, config.RunAsAdmin);
                    break;
                case ButtonActionType.RunPs1:
                    ExecutePowerShell(config.Target, config.RunAsAdmin);
                    break;
                case ButtonActionType.CopyClipboard:
                    CopyToClipboard(config.Target);
                    break;
                case ButtonActionType.OpenUrlFirefox:
                    OpenFirefox(config.Target);
                    break;
            }

            _logger.Info($"Aktion '{config.Id}' erfolgreich ausgeführt.");
        }
        catch (Exception ex)
        {
            _logger.Error($"Fehler beim Ausführen der Aktion '{config.Id}'.", ex);
            ShowError(ex.Message);
        }
    }

    private void ExecuteExplorer(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("Es wurde kein Pfad angegeben.");
        }

        if (!Directory.Exists(path) && !File.Exists(path))
        {
            throw new FileNotFoundException($"Pfad '{path}' wurde nicht gefunden.");
        }

        var psi = new ProcessStartInfo("explorer.exe", $"\"{path}\"")
        {
            UseShellExecute = true
        };

        Process.Start(psi);
    }

    private void ExecuteProcess(string fileName, string? arguments, bool runAsAdmin)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("Es wurde keine Datei angegeben.");
        }

        if (IsPathCheckRequired(fileName) && !File.Exists(fileName))
        {
            throw new FileNotFoundException($"Datei '{fileName}' wurde nicht gefunden.");
        }

        var psi = new ProcessStartInfo(fileName, arguments ?? string.Empty)
        {
            UseShellExecute = true
        };

        if (runAsAdmin)
        {
            psi.Verb = "runas";
        }

        Process.Start(psi);
    }

    private static bool IsPathCheckRequired(string fileName)
    {
        // Allow commands such as "powershell.exe" or "notepad" to be resolved via the
        // operating system's PATH lookup. Only verify the file exists when an explicit
        // directory is provided to avoid false negatives for built-in executables.
        return Path.IsPathRooted(fileName)
               || fileName.Contains(Path.DirectorySeparatorChar)
               || fileName.Contains(Path.AltDirectorySeparatorChar);
    }

    private void ExecutePowerShell(string scriptPath, bool runAsAdmin)
    {
        if (string.IsNullOrWhiteSpace(scriptPath))
        {
            throw new InvalidOperationException("Es wurde kein Skript angegeben.");
        }

        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Skript '{scriptPath}' wurde nicht gefunden.");
        }

        var arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"";
        ExecuteProcess("powershell.exe", arguments, runAsAdmin);
    }

    private void CopyToClipboard(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new InvalidOperationException("Kein Text für die Zwischenablage vorhanden.");
        }

        Clipboard.SetText(text);
    }

    private void OpenFirefox(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException("Es wurde keine URL angegeben.");
        }

        var psi = new ProcessStartInfo("firefox.exe", url)
        {
            UseShellExecute = true
        };

        try
        {
            Process.Start(psi);
        }
        catch (Win32Exception)
        {
            var fallback = new ProcessStartInfo(url)
            {
                UseShellExecute = true
            };

            Process.Start(fallback);
        }
    }

    private void ShowError(string message)
    {
        MessageBox.Show(_owner, message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
