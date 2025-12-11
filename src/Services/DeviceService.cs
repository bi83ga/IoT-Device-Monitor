using IoTDeviceMonitor.Models;
using IoTDeviceMonitor.Utils;

namespace IoTDeviceMonitor.Services;

public class DeviceService
{
    private readonly List<Device> _devices = new List<Device>();
    private readonly Logger _logger;
    private readonly FileManager _fileManager;

    public DeviceService(Logger logger, FileManager fileManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        LoadDevices();
    }

    public IEnumerable<Device> Devices => _devices.AsReadOnly();

    public bool AddDevice(Device device)
    {
        if (device == null || string.IsNullOrWhiteSpace(device.Id) || string.IsNullOrWhiteSpace(device.Name) || string.IsNullOrWhiteSpace(device.IpAddress) || !IsValidIp(device.IpAddress) || FindById(device.Id) != null)
        {
            _logger.Log($"Failed to add device: Invalid or duplicate device data. ID: {device?.Id}, Name: {device?.Name}, IP: {device?.IpAddress}");
            return false;
        }

        _devices.Add(device);
        _logger.Log($"Device added: {device.Id} - {device.Name} ({device.IpAddress})");
        return true;
    }

    public bool UpdateStatus(string id, DeviceStatus status)
    {
        var device = FindById(id);
        if (device == null)
        {
            _logger.Log($"Failed to update status: Device not found. ID: {id}");
            return false;
        }

        device.UpdateStatus(status);
        _logger.Log($"Device status updated: {id} to {status}");
        return true;
    }

    public bool RemoveDevice(string id)
    {
        var device = FindById(id);
        if (device == null)
        {
            _logger.Log($"Failed to remove device: Device not found. ID: {id}");
            return false;
        }

        _devices.Remove(device);
        _logger.Log($"Device removed: {id} - {device.Name} ({device.IpAddress})");
        return true;
    }

    public Device? FindById(string id) => _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Device> SearchDevice(string query) => _devices.Where(d => d.Id.Contains(query, StringComparison.OrdinalIgnoreCase) || d.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

    public bool SortDevices(string criteria)
    {
        switch (criteria.ToLowerInvariant())
        {
            case "name":
                _devices.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                _logger.Log("Devices sorted by name");
                return true;
            case "status":
                _devices.Sort((a, b) => a.Status.CompareTo(b.Status));
                _logger.Log("Devices sorted by status");
                return true;
            default:
                _logger.Log($"Failed to sort devices: Invalid criteria '{criteria}'");
                return false;
        }
    }

    public void Save() => _fileManager.SaveDevices(_devices);

    private void LoadDevices()
    {
        var devices = _fileManager.LoadDevices();
        _devices.AddRange(devices);
        _logger.Log($"Loaded {devices.Count} devices from file");
    }

    public static bool IsValidIp(string ipAddress) => System.Net.IPAddress.TryParse(ipAddress, out _);
}
