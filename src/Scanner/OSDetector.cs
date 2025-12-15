using System.Net.NetworkInformation;

namespace PortXOR.Scanner;

/// <summary>
/// Detects operating system based on network responses
/// </summary>
public class OSDetector
{
    /// <summary>
    /// Attempts to detect the target's operating system
    /// </summary>
    public async Task<string> DetectOSAsync(string target)
    {
        try
        {
            // Basic OS detection using TTL and other TCP/IP stack characteristics
            // This is a simplified implementation
            var ping = new Ping();
            var reply = await ping.SendPingAsync(target, 1000);

            if (reply.Status == IPStatus.Success)
            {
                // TTL-based OS detection (simplified)
                var ttl = reply.Options?.Ttl ?? 64;
                return ttl switch
                {
                    <= 64 => "Linux/Unix",
                    <= 128 => "Windows",
                    _ => "Unknown"
                };
            }
        }
        catch
        {
            // Detection failed
        }

        return "Unknown";
    }
}

