# IoT Device Monitor

A console-based application for managing IoT devices, built with C# and .NET 8. It allows users to add, update, search, sort, and remove devices, with data persistence to JSON and event logging.

## Features

- Add new devices with ID, name, and IP address
- Update device status (Online, Offline, Maintenance)
- Search devices by ID or name
- Sort devices by name or status
- Remove devices
- Persistent storage using JSON
- Event logging for auditing

## Project Structure

- `src/Models/`: Core models (Device, DeviceStatus, Logger)
- `src/Services/`: Business logic (DeviceService)
- `src/Utils/`: Utilities (FileManager)
- `src/Program.cs`: Main entry point
- `test/`: Unit tests
- `logs/`: Application logs
- `data/`: Persisted data
- `docs/`: Documentation
- `assets/`: Optional assets

## Setup and Running

1. Ensure .NET 8 SDK is installed.
2. Clone or navigate to the project directory.
3. Run `dotnet build` to build the project.
4. Run `dotnet run --project src/IoTDeviceMonitor.csproj` to start the application.
5. Follow the menu prompts to manage devices.

## Testing

Run unit tests with `dotnet test`.

## Dependencies

- .NET 8
- Newtonsoft.Json for JSON serialization
