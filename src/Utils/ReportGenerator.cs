using PortXOR.Scanner;
using System.Text;

namespace PortXOR.Utils;

/// <summary>
/// Generates scan reports in text format
/// </summary>
public class ReportGenerator
{
    private readonly string _reportsDirectory;

    public ReportGenerator()
    {
        // Use reports directory relative to project root
        var projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName 
            ?? Directory.GetCurrentDirectory();
        _reportsDirectory = Path.Combine(projectRoot, "reports");
        
        if (!Directory.Exists(_reportsDirectory))
        {
            Directory.CreateDirectory(_reportsDirectory);
        }
    }

    /// <summary>
    /// Generates a timestamped report file from scan results
    /// </summary>
    public string GenerateReport(ScanResult result, string target)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"PortXOR_Report_{target.Replace(".", "_")}_{timestamp}.txt";
        var filePath = Path.Combine(_reportsDirectory, fileName);

        var report = BuildReportContent(result);
        File.WriteAllText(filePath, report);

        return filePath;
    }

    private string BuildReportContent(ScanResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=".PadRight(70, '='));
        sb.AppendLine("PortXOR Scan Report");
        sb.AppendLine("=".PadRight(70, '='));
        sb.AppendLine();
        sb.AppendLine($"Target: {result.Target}");
        sb.AppendLine($"Scan Type: {result.ScanType.ToUpper()}");
        sb.AppendLine($"Port Range: {result.StartPort} - {result.EndPort}");
        sb.AppendLine($"Total Ports Scanned: {result.TotalPortsScanned}");
        sb.AppendLine($"Start Time: {result.StartTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"End Time: {result.EndTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Duration: {result.Duration.TotalSeconds:F2} seconds");
        sb.AppendLine($"Detected OS: {result.DetectedOS}");
        sb.AppendLine();
        sb.AppendLine("-".PadRight(70, '-'));

        if (result.OpenPorts.Any())
        {
            sb.AppendLine($"OPEN PORTS ({result.OpenPorts.Count}):");
            sb.AppendLine("-".PadRight(70, '-'));
            sb.AppendLine($"{"Port",-10} {"Protocol",-10} {"Banner",-50}");
            sb.AppendLine("-".PadRight(70, '-'));

            foreach (var port in result.OpenPorts.OrderBy(p => p.Port))
            {
                var banner = string.IsNullOrEmpty(port.Banner) ? "N/A" : port.Banner;
                sb.AppendLine($"{port.Port,-10} {port.Protocol,-10} {banner,-50}");
            }
        }
        else
        {
            sb.AppendLine("No open ports found.");
        }

        sb.AppendLine();
        sb.AppendLine("=".PadRight(70, '='));
        sb.AppendLine($"Report generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("=".PadRight(70, '='));

        return sb.ToString();
    }
}

