# PortXOR - A Lightweight Port Scanner

PortXOR is a lightweight, cross-platform CLI-based port scanner built with C# and .NET 8. It combines fast TCP/UDP/SYN scanning capabilities with an integrated defensive module (BlueXOR) for real-time scan detection.

## Features

- **Multi-Protocol Scanning**: TCP, UDP, and SYN scan support
- **Banner Grabbing**: Service identification on open ports
- **OS Detection**: Operating system fingerprinting capabilities
- **BlueXOR Module**: Real-time detection of incoming scans
- **Report Generation**: Timestamped scan reports
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Project Structure

```
/PortXOR
├── /src/                  # Source code (C# files, projects)
├── /docs/                 # Documentation (design docs, diagrams)
├── /tests/                # Unit and integration tests
├── /reports/              # Generated scan reports
├── /configs/              # Configuration files
├── /scripts/              # Utility scripts
├── README.md              # This file
├── .gitignore             # Git ignore file
└── LICENSE                # Licensing info
```

## Requirements

- .NET 8 SDK or later
- Windows/Linux/macOS
- Administrator/root privileges (for BlueXOR defensive module)

## Building the Project

```bash
cd src
dotnet build
```

## Running PortXOR

```bash
cd src
dotnet run -- [options]
```

## Usage Examples

### Basic TCP Port Scan
```bash
dotnet run -- --target 192.168.1.1 --ports 20-80
```

### Full Port Range Scan
```bash
dotnet run -- --target 192.168.1.1
```

### Enable BlueXOR Detection
```bash
dotnet run -- --bluexor --monitor
```

## Development Status

**Phase 1**: Project design and architecture (Current)
**Phase 2**: Core scanner implementation (Planned)
**Phase 3**: BlueXOR defensive module (Planned)
**Phase 4**: Testing and optimization (Planned)

## License

See LICENSE file for details.

## Contributing

This is an academic project. Contributions and suggestions are welcome!

## Disclaimer

This tool is intended for educational purposes and authorized security testing only. Unauthorized port scanning may be illegal in your jurisdiction. Use responsibly and ethically.

