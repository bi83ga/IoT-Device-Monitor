using System.Globalization;

namespace IoTDeviceMonitor;

public class Logger
{
    private readonly string _logPath;

    public Logger(string logPath = "events.log")
    {
        _logPath = logPath;
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

