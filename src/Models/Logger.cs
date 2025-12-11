using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace IoTDeviceMonitor.Models;

public class Logger
{
    private readonly string _logPath;

    public Logger(IConfiguration configuration)
    {
        _logPath = configuration["LogFilePath"] ?? "logs/device_log.txt";
    }

    public void Log(string message)
    {
        try
        {
            var line = $"{DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)} | {message}";
            File.AppendAllLines(_logPath, new[] { line });
        }
        catch
        {
            // Swallow logging errors to avoid crashing the app.
        }
    }
}
