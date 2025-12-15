using System.Net.Sockets;
using System.Text;

namespace PortXOR.Scanner;

/// <summary>
/// Grabs service banners from open ports
/// </summary>
public class BannerGrabber
{
    private const int BannerTimeout = 3000; // milliseconds
    private const int ReadDelay = 100; // Small delay for localhost services

    /// <summary>
    /// Attempts to grab a banner from the specified port
    /// </summary>
    public async Task<string> GrabBannerAsync(string target, int port, string protocol)
    {
        if (protocol != "TCP")
            return string.Empty; // Banner grabbing primarily works with TCP

        try
        {
            using var client = new TcpClient();
            
            // Set socket options for better localhost support
            client.ReceiveTimeout = BannerTimeout;
            client.SendTimeout = BannerTimeout;
            client.NoDelay = true; // Disable Nagle's algorithm for faster localhost response

            var connectTask = client.ConnectAsync(target, port);
            var timeoutTask = Task.Delay(BannerTimeout);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == connectTask && client.Connected)
            {
                var stream = client.GetStream();
                stream.ReadTimeout = BannerTimeout;

                // For localhost, wait a bit for the service to send initial data
                await Task.Delay(ReadDelay);

                // Try to read any data sent immediately upon connection
                var initialBanner = await ReadAvailableDataAsync(stream);
                
                // If no initial data, send a probe
                if (string.IsNullOrEmpty(initialBanner))
                {
                    var probe = GetProbeForPort(port);
                    if (!string.IsNullOrEmpty(probe))
                    {
                        var probeBytes = Encoding.UTF8.GetBytes(probe);
                        await stream.WriteAsync(probeBytes);
                        await stream.FlushAsync();
                        
                        // Wait a bit for response
                        await Task.Delay(ReadDelay);
                    }
                }

                // Read response (combine initial banner if any)
                var responseBanner = await ReadAvailableDataAsync(stream);
                var banner = !string.IsNullOrEmpty(initialBanner) 
                    ? $"{initialBanner.Trim()}\n{responseBanner.Trim()}".Trim()
                    : responseBanner.Trim();

                if (!string.IsNullOrEmpty(banner))
                {
                    // Clean up the banner
                    banner = CleanBanner(banner);
                    return banner.Length > 150 ? banner.Substring(0, 150) + "..." : banner;
                }
            }
        }
        catch (SocketException)
        {
            // Connection issues - port might be filtered
        }
        catch (Exception)
        {
            // Other errors - failed to grab banner
        }

        return string.Empty;
    }

    /// <summary>
    /// Reads all available data from the stream without blocking
    /// </summary>
    private async Task<string> ReadAvailableDataAsync(NetworkStream stream)
    {
        var buffer = new byte[4096];
        var bannerBuilder = new StringBuilder();
        var totalBytesRead = 0;
        var maxBytes = 2048; // Limit banner size

        try
        {
            while (stream.DataAvailable && totalBytesRead < maxBytes)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, maxBytes - totalBytesRead));
                if (bytesRead == 0)
                    break;

                totalBytesRead += bytesRead;
                var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                bannerBuilder.Append(text);

                // Small delay to allow more data to arrive
                if (stream.DataAvailable)
                    await Task.Delay(50);
            }
        }
        catch
        {
            // Read timeout or other error - return what we have
        }

        return bannerBuilder.ToString();
    }

    /// <summary>
    /// Cleans up banner text by removing control characters and normalizing whitespace
    /// </summary>
    private string CleanBanner(string banner)
    {
        if (string.IsNullOrEmpty(banner))
            return string.Empty;

        // Replace common control characters but keep newlines
        var cleaned = banner
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        // Remove null bytes and other control chars except newlines and tabs
        var sb = new StringBuilder();
        foreach (var c in cleaned)
        {
            if (char.IsControl(c) && c != '\n' && c != '\t')
                continue;
            sb.Append(c);
        }

        return sb.ToString().Trim();
    }

    private string GetProbeForPort(int port)
    {
        // Common service probes - expanded list
        return port switch
        {
            21 => "QUIT\r\n",                                    // FTP
            22 => "\r\n",                                        // SSH
            23 => "\r\n",                                        // Telnet
            25 => "QUIT\r\n",                                    // SMTP
            53 => "\x00\x00\x10\x00\x00\x00\x00\x00\x00\x00\x00\x00", // DNS
            80 => "GET / HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n", // HTTP
            110 => "QUIT\r\n",                                   // POP3
            143 => "a001 LOGOUT\r\n",                            // IMAP
            443 => "GET / HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n", // HTTPS
            3306 => "\x00\x00\x00\x0a\x00\x00\x00\x00\x00\x00\x00\x00", // MySQL
            5432 => "\x00\x00\x00\x04\xd2\x16\x2f\x9b",         // PostgreSQL
            8080 => "GET / HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n", // HTTP Alt
            8443 => "GET / HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n", // HTTPS Alt
            _ => string.Empty
        };
    }
}

