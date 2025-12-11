using IoTDeviceMonitor.Models;

namespace IoTDeviceMonitor.Models;

public class Device
{
    public Device()
    {
    }

    public Device(string id, string name, string ipAddress, DeviceStatus status = DeviceStatus.Offline)
    {
        Id = id;
        Name = name;
        IpAddress = ipAddress;
        Status = status;
    }

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;

    public void UpdateStatus(DeviceStatus newStatus)
    {
        Status = newStatus;
    }

    public void DisplayInfo()
    {
        Console.WriteLine(ToDisplayString());
    }

    public string ToDisplayString()
    {
        return $"{Id,-12} {Name,-20} {IpAddress,-16} {Status}";
    }
}
