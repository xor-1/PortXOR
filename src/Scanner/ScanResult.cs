namespace PortXOR.Scanner;

/// <summary>
/// Represents the results of a port scan
/// </summary>
public class ScanResult
{
    public string Target { get; set; } = string.Empty;
    public int StartPort { get; set; }
    public int EndPort { get; set; }
    public string ScanType { get; set; } = "tcp";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<PortStatus> OpenPorts { get; set; } = new();
    public string DetectedOS { get; set; } = "Unknown";

    public TimeSpan Duration => EndTime - StartTime;
    public int TotalPortsScanned => EndPort - StartPort + 1;
}

/// <summary>
/// Represents the status of a single port
/// </summary>
public class PortStatus
{
    public int Port { get; set; }
    public string Protocol { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public string Banner { get; set; } = string.Empty;
}

