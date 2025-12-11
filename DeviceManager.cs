using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IoTDeviceMonitor;

public class DeviceManager
{
    private readonly List<Device> _devices = new();
    private readonly string _storePath;
    private readonly Logger _logger;

    public DeviceManager(string storePath = "devices.json", Logger? logger = null)
    {
        _storePath = storePath;
        _logger = logger ?? new Logger();
        Load();
    }

    public IReadOnlyList<Device> Devices => _devices;

    public bool AddDevice(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Id) || string.IsNullOrWhiteSpace(device.Name))
        {
            return false;
        }

        if (!IsValidIp(device.IpAddress))
        {
            return false;
        }

        if (_devices.Any(d => string.Equals(d.Id, device.Id, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        _devices.Add(device);
        Save();
        _logger.Log($"Device added: {device.Name} ({device.Id})");
        return true;
    }

    public Device? FindById(string id)
    {
        return _devices.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public List<Device> SearchByName(string namePart)
    {
        namePart = namePart ?? string.Empty;
        return _devices
            .Where(d => d.Name.Contains(namePart, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public bool UpdateStatus(string id, DeviceStatus newStatus)
    {
        var device = FindById(id);
        if (device == null)
        {
            return false;
        }

        var previous = device.Status;
        device.UpdateStatus(newStatus);
        Save();
        _logger.Log($"Device status updated: {device.Name} ({device.Id}) {previous} -> {newStatus}");
        return true;
    }

    public bool RemoveDevice(string id)
    {
        var device = FindById(id);
        if (device == null)
        {
            return false;
        }

        _devices.Remove(device);
        Save();
        _logger.Log($"Device removed: {device.Name} ({device.Id})");
        return true;
    }

    public void SortByName()
    {
        _devices.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
    }

    public void SortByStatusThenName()
    {
        _devices.Sort((a, b) =>
        {
            var byStatus = CompareStatus(a.Status, b.Status);
            return byStatus != 0 ? byStatus : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        });
    }

    // Optional bubble sort implementations to illustrate manual sorting.
    public void BubbleSortByName()
    {
        for (var i = 0; i < _devices.Count - 1; i++)
        {
            for (var j = 0; j < _devices.Count - i - 1; j++)
            {
                if (string.Compare(_devices[j].Name, _devices[j + 1].Name, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    (_devices[j], _devices[j + 1]) = (_devices[j + 1], _devices[j]);
                }
            }
        }
    }

    public void BubbleSortByStatusThenName()
    {
        for (var i = 0; i < _devices.Count - 1; i++)
        {
            for (var j = 0; j < _devices.Count - i - 1; j++)
            {
                var current = _devices[j];
                var next = _devices[j + 1];
                var byStatus = CompareStatus(current.Status, next.Status);
                var shouldSwap = byStatus > 0 ||
                                 (byStatus == 0 &&
                                  string.Compare(current.Name, next.Name, StringComparison.OrdinalIgnoreCase) > 0);
                if (shouldSwap)
                {
                    (_devices[j], _devices[j + 1]) = (_devices[j + 1], _devices[j]);
                }
            }
        }
    }

    public void Load()
    {
        if (!File.Exists(_storePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_storePath);
            var devices = JsonConvert.DeserializeObject<List<Device>>(json, SerializerSettings());
            if (devices != null)
            {
                _devices.Clear();
                _devices.AddRange(devices);
                _logger.Log($"Loaded {_devices.Count} devices from {_storePath}");
            }
        }
        catch (Exception ex)
        {
            _logger.Log($"Failed to load devices: {ex.Message}");
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_devices, Formatting.Indented, SerializerSettings());
            File.WriteAllText(_storePath, json);
        }
        catch (Exception ex)
        {
            _logger.Log($"Failed to save devices: {ex.Message}");
        }
    }

    private static JsonSerializerSettings SerializerSettings()
    {
        return new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() },
            Formatting = Formatting.Indented
        };
    }

    /// <summary>
    /// Linear search by ID (exact) or name (contains, case-insensitive).
    /// </summary>
    public List<Device> SearchDevice(string deviceIdOrName)
    {
        var results = new List<Device>();
        if (string.IsNullOrWhiteSpace(deviceIdOrName))
        {
            return results;
        }

        var query = deviceIdOrName.Trim();
        foreach (var device in _devices)
        {
            if (string.Equals(device.Id, query, StringComparison.OrdinalIgnoreCase) ||
                device.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(device);
            }
        }

        return results;
    }

    public bool SortDevices(string criteria)
    {
        if (string.Equals(criteria, "name", StringComparison.OrdinalIgnoreCase))
        {
            SortByName();
            return true;
        }

        if (string.Equals(criteria, "status", StringComparison.OrdinalIgnoreCase))
        {
            SortByStatusThenName();
            return true;
        }

        return false;
    }

    public static bool IsValidIp(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        // Basic IPv4 pattern.
        var regex = new System.Text.RegularExpressions.Regex(@"^(25[0-5]|2[0-4]\d|[01]?\d\d?)\.(25[0-5]|2[0-4]\d|[01]?\d\d?)\.(25[0-5]|2[0-4]\d|[01]?\d\d?)\.(25[0-5]|2[0-4]\d|[01]?\d\d?)$");
        return regex.IsMatch(input);
    }

    private static int CompareStatus(DeviceStatus a, DeviceStatus b)
    {
        static int Rank(DeviceStatus status) =>
            status switch
            {
                DeviceStatus.Offline => 0,
                DeviceStatus.Maintenance => 1,
                DeviceStatus.Online => 2,
                _ => 3
            };

        return Rank(a).CompareTo(Rank(b));
    }
}

