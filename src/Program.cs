using PortXOR.Scanner;
using PortXOR.BlueXOR;
using PortXOR.Utils;

namespace PortXOR;

/// <summary>
/// PortXOR - A lightweight port scanner with integrated defensive detection
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Display colorful header
        ColorConsole.WriteHeader();
        Console.WriteLine();

        // Parse command-line arguments
        var options = ParseArguments(args);

        if (options == null)
        {
            ShowUsage();
            return;
        }

        try
        {
            // Initialize scanner
            var scanner = new PortScanner();

            // Perform scan if target specified
            if (!string.IsNullOrEmpty(options.Target))
            {
                var results = await scanner.ScanAsync(
                    options.Target,
                    options.StartPort,
                    options.EndPort,
                    options.ScanType
                );

                // Generate report
                var reportGenerator = new ReportGenerator();
                var reportPath = reportGenerator.GenerateReport(results, options.Target);

                ColorConsole.WriteSuccess("\n✓ Scan completed successfully!");
                ColorConsole.WriteLineDim($"  Report saved to: {reportPath}");
                
                // Summary
                Console.WriteLine();
                ColorConsole.WriteInfo("Summary: ");
                ColorConsole.WriteLineDim($"{results.OpenPorts.Count} open port(s) found out of {results.TotalPortsScanned} scanned");
                ColorConsole.WriteInfo("Duration: ");
                ColorConsole.WriteLineDim($"{results.Duration.TotalSeconds:F2} seconds");
                if (results.DetectedOS != "Unknown")
                {
                    ColorConsole.WriteInfo("Detected OS: ");
                    ColorConsole.WriteLineDim(results.DetectedOS);
                }
            }

            // Start BlueXOR monitoring if requested
            if (options.EnableBlueXOR)
            {
                Console.WriteLine();
                ColorConsole.WriteBanner("Starting BlueXOR defensive module...\n");
                var blueXOR = new BlueXORDetector();
                await blueXOR.StartMonitoringAsync();
            }
        }
        catch (Exception ex)
        {
            ColorConsole.WriteError($"\n✗ Error: ");
            ColorConsole.WriteLineError(ex.Message);
            if (ex.InnerException != null)
            {
                ColorConsole.WriteDim("  Details: ");
                ColorConsole.WriteLineDim(ex.InnerException.Message);
            }
            Environment.Exit(1);
        }
    }

    static ScanOptions? ParseArguments(string[] args)
    {
        var options = new ScanOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--target":
                case "-t":
                    if (i + 1 < args.Length)
                        options.Target = args[++i];
                    break;
                case "--ports":
                case "-p":
                    if (i + 1 < args.Length)
                    {
                        var portRange = args[++i].Split('-');
                        if (portRange.Length == 2)
                        {
                            if (int.TryParse(portRange[0], out int startPort))
                                options.StartPort = startPort;
                            if (int.TryParse(portRange[1], out int endPort))
                                options.EndPort = endPort;
                        }
                        else if (int.TryParse(portRange[0], out int singlePort))
                        {
                            options.StartPort = singlePort;
                            options.EndPort = singlePort;
                        }
                    }
                    break;
                case "--scan-type":
                case "-s":
                    if (i + 1 < args.Length)
                        options.ScanType = args[++i].ToLower();
                    break;
                case "--bluexor":
                case "-b":
                    options.EnableBlueXOR = true;
                    break;
                case "--help":
                case "-h":
                    return null;
            }
        }

        return string.IsNullOrEmpty(options.Target) && !options.EnableBlueXOR ? null : options;
    }

    static void ShowUsage()
    {
        ColorConsole.WriteInfo("Usage: ");
        Console.WriteLine("PortXOR [options]\n");
        
        ColorConsole.WriteBanner("Options:\n");
        ColorConsole.WriteSuccess("  -t, --target <IP>        ");
        Console.WriteLine("Target IP address to scan");
        ColorConsole.WriteSuccess("  -p, --ports <range>      ");
        Console.WriteLine("Port range (e.g., 20-80 or 80)");
        ColorConsole.WriteSuccess("  -s, --scan-type <type>   ");
        Console.WriteLine("Scan type: tcp, udp, syn (default: tcp)");
        ColorConsole.WriteSuccess("  -b, --bluexor            ");
        Console.WriteLine("Enable BlueXOR defensive monitoring");
        ColorConsole.WriteSuccess("  -h, --help               ");
        Console.WriteLine("Show this help message");
        
        Console.WriteLine();
        ColorConsole.WriteBanner("Examples:\n");
        ColorConsole.WriteDim("  PortXOR --target 192.168.1.1 --ports 20-80\n");
        ColorConsole.WriteDim("  PortXOR --target localhost --ports 80,443,8080\n");
        ColorConsole.WriteDim("  PortXOR --target 192.168.1.1\n");
        ColorConsole.WriteDim("  PortXOR --bluexor\n");
    }
}

/// <summary>
/// Command-line options for PortXOR
/// </summary>
class ScanOptions
{
    public string Target { get; set; } = string.Empty;
    public int StartPort { get; set; } = 1;
    public int EndPort { get; set; } = 65535;
    public string ScanType { get; set; } = "tcp";
    public bool EnableBlueXOR { get; set; } = false;
}

