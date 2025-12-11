using IoTDeviceMonitor.Models;
using IoTDeviceMonitor.Services;
using IoTDeviceMonitor.Utils;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var logger = new Logger(configuration);
var fileManager = new FileManager(configuration);
var service = new DeviceService(logger, fileManager);

Console.WriteLine("IoT Device Monitor");
Console.WriteLine("------------------");

bool exit = false;
while (!exit)
{
    ShowMenu();
    Console.Write("Please enter your choice (1-8): ");
    var choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                AddDeviceInteractive(service);
                break;
            case "2":
                UpdateStatus(service);
                break;
            case "3":
                SearchDevice(service);
                break;
            case "4":
                SortDevices(service);
                break;
            case "5":
                RemoveDevice(service);
                break;
            case "6":
                ViewAllDevices(service);
                break;
            case "7":
                GenerateReport(service);
                break;
            case "8":
                service.Save();
                exit = true;
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}. Please try again.");
        logger.Log($"Error in main loop: {ex.Message}");
    }

    if (!exit)
    {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();
    }
}

static void ShowMenu()
{
    Console.WriteLine("[1] Add New Device");
    Console.WriteLine("[2] Update Device Status");
    Console.WriteLine("[3] Search for a Device");
    Console.WriteLine("[4] Sort Devices");
    Console.WriteLine("[5] Remove a Device");
    Console.WriteLine("[6] View All Devices");
    Console.WriteLine("[7] Generate Report");
    Console.WriteLine("[8] Save & Exit");
}

static void AddDeviceInteractive(DeviceService service)
{
    Console.Write("Enter Device ID: ");
    var id = (Console.ReadLine() ?? string.Empty).Trim();

    if (service.FindById(id) != null)
    {
        Console.WriteLine("Error: Device ID already exists.");
        return;
    }

    Console.Write("Enter Device Name: ");
    var name = (Console.ReadLine() ?? string.Empty).Trim();

    Console.Write("Enter Device IP Address: ");
    var ip = (Console.ReadLine() ?? string.Empty).Trim();

    if (!DeviceService.IsValidIp(ip))
    {
        Console.WriteLine("Error: Invalid IP address format.");
        return;
    }

    var device = new Device(id, name, ip);

    if (service.AddDevice(device))
    {
        service.Save();
        Console.WriteLine("Device added successfully.");
    }
    else
    {
        Console.WriteLine("Failed to add device. Check inputs.");
    }
}

static void UpdateStatus(DeviceService service)
{
    Console.Write("Enter Device ID: ");
    var id = Console.ReadLine() ?? string.Empty;

    var device = service.FindById(id);
    if (device == null)
    {
        Console.WriteLine("Device not found.");
        return;
    }

    Console.WriteLine($"Current status: {device.Status}");
    Console.Write("Enter new status (Online, Offline, Maintenance): ");
    var statusInput = Console.ReadLine();
    if (!Enum.TryParse<DeviceStatus>(statusInput, true, out var status))
    {
        Console.WriteLine("Invalid status.");
        return;
    }

    if (service.UpdateStatus(id, status))
    {
        service.Save();
        Console.WriteLine("Status updated.");
    }
    else
    {
        Console.WriteLine("Failed to update status.");
    }
}

static void SearchDevice(DeviceService service)
{
    Console.Write("Enter Device ID or Name: ");
    var query = Console.ReadLine() ?? string.Empty;
    var matches = service.SearchDevice(query);
    if (!matches.Any())
    {
        Console.WriteLine("No matching devices found.");
        return;
    }

    PrintDevices(matches);
}

static void SortDevices(DeviceService service)
{
    Console.Write("Sort by 'Name' or 'Status': ");
    var criteria = Console.ReadLine() ?? string.Empty;
    if (!service.SortDevices(criteria))
    {
        Console.WriteLine("Invalid sort criteria. Use Name or Status.");
        return;
    }

    service.Save();
    Console.WriteLine("Devices sorted.");
    PrintDevices(service.Devices);
}

static void RemoveDevice(DeviceService service)
{
    Console.Write("Enter Device ID to remove: ");
    var id = Console.ReadLine() ?? string.Empty;
    if (service.RemoveDevice(id))
    {
        service.Save();
        Console.WriteLine("Device removed.");
    }
    else
    {
        Console.WriteLine("Device not found.");
    }
}

static void PrintDevices(IEnumerable<Device> devices)
{
    var list = devices.ToList();
    if (!list.Any())
    {
        Console.WriteLine("No devices found.");
        return;
    }

    Console.WriteLine("ID           Name                 IP               Status");
    Console.WriteLine("--------------------------------------------------------------");
    foreach (var device in list)
    {
        Console.WriteLine(device.ToDisplayString());
    }
}

static void ViewAllDevices(DeviceService service)
{
    PrintDevices(service.Devices);
}

static void GenerateReport(DeviceService service)
{
    var devices = service.Devices.ToList();
    var total = devices.Count;
    var online = devices.Count(d => d.Status == DeviceStatus.Online);
    var offline = devices.Count(d => d.Status == DeviceStatus.Offline);
    var maintenance = devices.Count(d => d.Status == DeviceStatus.Maintenance);

    Console.WriteLine("Device Report");
    Console.WriteLine("-------------");
    Console.WriteLine($"Total Devices: {total}");
    Console.WriteLine($"Online: {online}");
    Console.WriteLine($"Offline: {offline}");
    Console.WriteLine($"Maintenance: {maintenance}");
}
