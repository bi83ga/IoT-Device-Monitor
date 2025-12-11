using IoTDeviceMonitor.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace IoTDeviceMonitor.Utils;

public class FileManager
{
    private readonly string _dataPath;
    private readonly bool _backupEnabled;

    public FileManager(IConfiguration configuration)
    {
        _dataPath = configuration["DataFilePath"] ?? "data/devices.json";
        _backupEnabled = bool.TryParse(configuration["BackupEnabled"], out var backup) ? backup : true;
    }

    public List<Device> LoadDevices()
    {
        try
        {
            if (!File.Exists(_dataPath))
            {
                return new List<Device>();
            }

            var json = File.ReadAllText(_dataPath);
            var devices = JsonConvert.DeserializeObject<List<Device>>(json) ?? new List<Device>();
            return devices;
        }
        catch (Exception)
        {
            return new List<Device>();
        }
    }

    public void SaveDevices(List<Device> devices)
    {
        try
        {
            if (_backupEnabled)
            {
                var backupPath = _dataPath.Replace(".json", $"_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                if (File.Exists(_dataPath))
                {
                    File.Copy(_dataPath, backupPath, true);
                }
            }

            var json = JsonConvert.SerializeObject(devices, Formatting.Indented);
            File.WriteAllText(_dataPath, json);
        }
        catch (Exception)
        {
            // Swallow errors to avoid crashing
        }
    }
}
