using System;

namespace ButtonLauncherApp.Services;

public interface ILogService
{
    void Info(string message);
    void Error(string message, Exception? exception = null);
}
