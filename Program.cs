using IoTDeviceMonitor;

var logger = new Logger();
var manager = new DeviceManager(logger: logger);

Console.WriteLine("IoT Device Monitor");
Console.WriteLine("------------------");

bool exit = false;
while (!exit)
{
    ShowMenu();
    Console.Write("Please enter your choice (1-6): ");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            AddDeviceInteractive(manager);
            break;
        case "2":
            UpdateStatus(manager);
            break;
        case "3":
            SearchDevice(manager);
            break;
        case "4":
            SortDevices(manager);
            break;
        case "5":
            RemoveDevice(manager);
            break;
        case "6":
            manager.Save();
            exit = true;
            break;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }

    if (!exit)
    {
        Console.WriteLine();
    }
}

static void ShowMenu()
{
    Console.WriteLine("[1] Add New Device");
    Console.WriteLine("[2] Update Device Status");
    Console.WriteLine("[3] Search for a Device");
    Console.WriteLine("[4] Sort Devices");
    Console.WriteLine("[5] Remove a Device");
    Console.WriteLine("[6] Save & Exit");
}

static void AddDeviceInteractive(DeviceManager manager)
{
    Console.Write("Enter Device ID: ");
    var id = (Console.ReadLine() ?? string.Empty).Trim();

    if (manager.FindById(id) != null)
    {
        Console.WriteLine("Error: Device ID already exists.");
        return;
    }

    Console.Write("Enter Device Name: ");
    var name = (Console.ReadLine() ?? string.Empty).Trim();

    Console.Write("Enter Device IP Address: ");
    var ip = (Console.ReadLine() ?? string.Empty).Trim();

    if (!DeviceManager.IsValidIp(ip))
    {
        Console.WriteLine("Error: Invalid IP address format.");
        return;
    }

    var device = new Device(id, name, ip);

    if (manager.AddDevice(device))
    {
        Console.WriteLine("Device added successfully.");
    }
    else
    {
        Console.WriteLine("Failed to add device. Check inputs.");
    }
}

static void UpdateStatus(DeviceManager manager)
{
    Console.Write("Enter Device ID: ");
    var id = Console.ReadLine() ?? string.Empty;

    Console.Write("Enter new status (Online, Offline, Maintenance): ");
    var statusInput = Console.ReadLine();
    if (!Enum.TryParse<DeviceStatus>(statusInput, true, out var status))
    {
        Console.WriteLine("Invalid status.");
        return;
    }

    if (manager.UpdateStatus(id, status))
    {
        Console.WriteLine("Status updated.");
    }
    else
    {
        Console.WriteLine("Device not found.");
    }
}

static void SearchDevice(DeviceManager manager)
{
    Console.Write("Enter Device ID or Name: ");
    var query = Console.ReadLine() ?? string.Empty;
    var matches = manager.SearchDevice(query);
    if (!matches.Any())
    {
        Console.WriteLine("No matching devices found.");
        return;
    }

    PrintDevices(matches);
}

static void SortDevices(DeviceManager manager)
{
    Console.Write("Sort by 'Name' or 'Status': ");
    var criteria = Console.ReadLine() ?? string.Empty;
    if (!manager.SortDevices(criteria))
    {
        Console.WriteLine("Invalid sort criteria. Use Name or Status.");
        return;
    }

    manager.Save();
    Console.WriteLine("Devices sorted.");
    PrintDevices(manager.Devices);
}

static void RemoveDevice(DeviceManager manager)
{
    Console.Write("Enter Device ID to remove: ");
    var id = Console.ReadLine() ?? string.Empty;
    if (manager.RemoveDevice(id))
    {
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

