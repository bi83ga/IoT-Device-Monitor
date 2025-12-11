using System;
using System.IO;
using System.Linq;
using IoTDeviceMonitor.Models;
using IoTDeviceMonitor.Services;
using IoTDeviceMonitor.Utils;
using Xunit;

namespace IoTDeviceMonitor.Tests;

public class DeviceManagerTests
{
    private DeviceService CreateService(out string dataPath)
    {
        dataPath = Path.Combine(Path.GetTempPath(), $"devices-{Guid.NewGuid()}.json");
        var logPath = Path.Combine(Path.GetTempPath(), $"log-{Guid.NewGuid()}.txt");
        var logger = new Logger(logPath);
        var fileManager = new FileManager(dataPath);
        return new DeviceService(logger, fileManager);
    }

    [Fact]
    public void AddDevice_RejectsDuplicateIds()
    {
        var service = CreateService(out _);
        var first = new Device { Id = "A1", Name = "Sensor 1", IpAddress = "10.0.0.1" };
        var dup = new Device { Id = "a1", Name = "Sensor 2", IpAddress = "10.0.0.2" };

        Assert.True(service.AddDevice(first));
        Assert.False(service.AddDevice(dup));
        Assert.Single(service.Devices);
    }

    [Fact]
    public void AddDevice_RequiresIdAndName()
    {
        var service = CreateService(out _);
        var missingId = new Device { Id = "", Name = "Sensor", IpAddress = "10.0.0.3" };
        var missingName = new Device { Id = "B1", Name = "", IpAddress = "10.0.0.4" };

        Assert.False(service.AddDevice(missingId));
        Assert.False(service.AddDevice(missingName));
        Assert.Empty(service.Devices);
    }

    [Fact]
    public void AddDevice_RejectsInvalidIp()
    {
        var service = CreateService(out _);
        var device = new Device { Id = "B2", Name = "Sensor", IpAddress = "999.1.1.1" };

        Assert.False(service.AddDevice(device));
        Assert.Empty(service.Devices);
    }

    [Fact]
    public void AddDevice_PersistsToJson()
    {
        var service = CreateService(out var path);
        var device = new Device { Id = "C1", Name = "Edge", IpAddress = "10.0.0.5" };

        Assert.True(service.AddDevice(device));

        var reloaded = new DeviceService(new Logger(Path.Combine(Path.GetTempPath(), $"log-{Guid.NewGuid()}.txt")), new FileManager(path));
        Assert.Single(reloaded.Devices);
        Assert.Equal("C1", reloaded.Devices.First().Id);
        Assert.Equal(DeviceStatus.Offline, reloaded.Devices.First().Status);
    }

    [Fact]
    public void UpdateStatus_PersistsChange()
    {
        var service = CreateService(out var path);
        var device = new Device { Id = "D1", Name = "Hub", IpAddress = "10.0.0.6" };
        service.AddDevice(device);

        Assert.True(service.UpdateStatus("D1", DeviceStatus.Maintenance));

        var reloaded = new DeviceService(new Logger(Path.Combine(Path.GetTempPath(), $"log-{Guid.NewGuid()}.txt")), new FileManager(path));
        Assert.Equal(DeviceStatus.Maintenance, reloaded.Devices.First().Status);
    }

    [Fact]
    public void Load_HandlesCorruptJsonGracefully()
    {
        var path = Path.Combine(Path.GetTempPath(), $"devices-{Guid.NewGuid()}.json");
        File.WriteAllText(path, "{ bad json");

        var service = new DeviceService(new Logger(Path.Combine(Path.GetTempPath(), $"log-{Guid.NewGuid()}.txt")), new FileManager(path));

        Assert.Empty(service.Devices);
    }

    [Fact]
    public void SearchDevice_FindsByIdOrName()
    {
        var service = CreateService(out _);
        service.AddDevice(new Device { Id = "E1", Name = "Gateway", IpAddress = "10.0.0.7" });
        service.AddDevice(new Device { Id = "F1", Name = "Temp Sensor", IpAddress = "10.0.0.8" });

        var byId = service.SearchDevice("E1");
        var byName = service.SearchDevice("temp");

        Assert.Single(byId);
        Assert.Equal("E1", byId.First().Id);

        Assert.Single(byName);
        Assert.Equal("F1", byName.First().Id);
    }

    [Fact]
    public void SortDevices_ByStatusThenName()
    {
        var service = CreateService(out _);
        service.AddDevice(new Device { Id = "E1", Name = "Gamma", IpAddress = "10.0.0.9" });
        service.AddDevice(new Device { Id = "F1", Name = "Alpha", IpAddress = "10.0.0.10" });
        service.AddDevice(new Device { Id = "G1", Name = "Beta", IpAddress = "10.0.0.11" });

        service.UpdateStatus("G1", DeviceStatus.Maintenance);
        service.UpdateStatus("E1", DeviceStatus.Online);

        service.SortDevices("status");
        var ordered = service.Devices.ToList();

        Assert.Equal("F1", ordered[0].Id); // Offline first (default)
        Assert.Equal("G1", ordered[1].Id); // Maintenance next
        Assert.Equal("E1", ordered[2].Id); // Online last
    }
}

