using System;
using System.Globalization;
using System.IO;

namespace ButtonLauncherApp.Services;

public sealed class FileLogService : ILogService
{
    private readonly string _logFilePath;
    private readonly object _syncRoot = new();

    public FileLogService(string? logFilePath = null)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(directory);
        _logFilePath = logFilePath ?? Path.Combine(directory, "button-launcher.log");
    }

    public void Info(string message)
    {
        WriteEntry("INFO", message, null);
    }

    public void Error(string message, Exception? exception = null)
    {
        WriteEntry("ERROR", message, exception);
    }

    private void WriteEntry(string level, string message, Exception? exception)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var lines = exception is null
            ? $"{timestamp} [{level}] {message}{Environment.NewLine}"
            : $"{timestamp} [{level}] {message}{Environment.NewLine}{exception}{Environment.NewLine}";

        lock (_syncRoot)
        {
            File.AppendAllText(_logFilePath, lines);
        }
    }
}
