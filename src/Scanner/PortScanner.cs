using System.Net;
using System.Net.Sockets;
using PortXOR.Utils;

namespace PortXOR.Scanner;

/// <summary>
/// Main port scanner engine supporting TCP, UDP, and SYN scans
/// </summary>
public class PortScanner
{
    private readonly BannerGrabber _bannerGrabber;
    private readonly OSDetector _osDetector;
    private const int DefaultTimeout = 1000; // milliseconds

    public PortScanner()
    {
        _bannerGrabber = new BannerGrabber();
        _osDetector = new OSDetector();
    }

    /// <summary>
    /// Performs an asynchronous port scan on the specified target
    /// </summary>
    public async Task<ScanResult> ScanAsync(string target, int startPort, int endPort, string scanType = "tcp")
    {
        var result = new ScanResult
        {
            Target = target,
            StartPort = startPort,
            EndPort = endPort,
            ScanType = scanType,
            StartTime = DateTime.Now
        };

        ColorConsole.WriteInfo($"Starting {scanType.ToUpper()} scan");
        ColorConsole.WriteLineDim($" on {target} (ports {startPort}-{endPort})...\n");

        var tasks = new List<Task<PortStatus>>();

        for (int port = startPort; port <= endPort; port++)
        {
            int currentPort = port;
            tasks.Add(ScanPortAsync(target, currentPort, scanType));
        }

        var results = await Task.WhenAll(tasks);
        result.OpenPorts = results.Where(r => r.IsOpen).ToList();
        result.EndTime = DateTime.Now;

        // Perform banner grabbing on open ports
        if (result.OpenPorts.Any())
        {
            ColorConsole.WriteInfo($"\nFound {result.OpenPorts.Count} open port(s). ");
            ColorConsole.WriteLineDim("Grabbing banners...\n");
            await GrabBannersAsync(result);
        }
        else
        {
            ColorConsole.WriteLineDim("\nNo open ports found.\n");
        }

        // Attempt OS detection
        result.DetectedOS = await _osDetector.DetectOSAsync(target);

        return result;
    }

    private async Task<PortStatus> ScanPortAsync(string target, int port, string scanType)
    {
        return scanType.ToLower() switch
        {
            "tcp" => await ScanTcpPortAsync(target, port),
            "udp" => await ScanUdpPortAsync(target, port),
            "syn" => await ScanSynPortAsync(target, port),
            _ => await ScanTcpPortAsync(target, port)
        };
    }

    private async Task<PortStatus> ScanTcpPortAsync(string target, int port)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(target, port);
            var timeoutTask = Task.Delay(DefaultTimeout);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == connectTask && client.Connected)
            {
                ColorConsole.WriteSuccess("  [+] ");
                ColorConsole.WritePort($"Port {port}/TCP");
                ColorConsole.WriteLineSuccess(" is OPEN");
                return new PortStatus { Port = port, Protocol = "TCP", IsOpen = true };
            }
        }
        catch
        {
            // Port is closed or filtered
        }

        return new PortStatus { Port = port, Protocol = "TCP", IsOpen = false };
    }

    private async Task<PortStatus> ScanUdpPortAsync(string target, int port)
    {
        // UDP scanning implementation
        // Note: UDP scanning is less reliable than TCP
        try
        {
            using var client = new UdpClient();
            client.Client.ReceiveTimeout = DefaultTimeout;

            var endpoint = new IPEndPoint(IPAddress.Parse(target), port);
            await client.SendAsync(new byte[] { 0 }, 1, endpoint);

            var result = await client.ReceiveAsync();
            ColorConsole.WriteSuccess("  [+] ");
            ColorConsole.WritePort($"Port {port}/UDP");
            ColorConsole.WriteLineSuccess(" is OPEN");
            return new PortStatus { Port = port, Protocol = "UDP", IsOpen = true };
        }
        catch
        {
            // Port may be closed, filtered, or open but not responding
        }

        return new PortStatus { Port = port, Protocol = "UDP", IsOpen = false };
    }

    private async Task<PortStatus> ScanSynPortAsync(string target, int port)
    {
        // SYN scan implementation using raw sockets
        // Note: Requires administrator/root privileges
        // This is a placeholder - full implementation requires raw socket access
        ColorConsole.WriteWarning($"  [*] SYN scan on port {port}");
        ColorConsole.WriteLineDim(" (requires raw sockets)");
        return new PortStatus { Port = port, Protocol = "TCP", IsOpen = false };
    }

    private async Task GrabBannersAsync(ScanResult result)
    {
        foreach (var port in result.OpenPorts)
        {
            ColorConsole.WriteInfo($"  [*] Port {port.Port}: ");
            var banner = await _bannerGrabber.GrabBannerAsync(result.Target, port.Port, port.Protocol);
            if (!string.IsNullOrEmpty(banner))
            {
                port.Banner = banner;
                // Display banner with proper formatting
                var bannerLines = banner.Split('\n');
                if (bannerLines.Length > 0)
                {
                    ColorConsole.WriteLineDim(bannerLines[0]);
                    if (bannerLines.Length > 1)
                    {
                        foreach (var line in bannerLines.Skip(1).Take(2))
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                ColorConsole.WriteDim("      ");
                                ColorConsole.WriteLineDim(line.Trim());
                            }
                        }
                    }
                }
            }
            else
            {
                ColorConsole.WriteLineDim("No banner available");
            }
        }
    }
}

