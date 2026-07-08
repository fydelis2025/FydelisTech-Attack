using System.Diagnostics;
using System.Net;
using FydelisTech_Attack.Core.Models;

namespace FydelisTech_Attack.Core.Services;

public class DnsService
{
    public async Task<DnsInfo> EnumerateAsync(string domain)
    {
        var info = new DnsInfo { Domain = domain };

        // A Records
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(domain);
            info.ARecords = addresses
                .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(a => a.ToString())
                .Distinct()
                .ToList();
            
            info.AaaaRecords = addresses
                .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                .Select(a => a.ToString())
                .Distinct()
                .ToList();
        }
        catch { }

        // Consultas via nslookup para MX, NS, TXT, SOA
        info.MxRecords = await RunNslookupAsync(domain, "MX");
        info.NsRecords = await RunNslookupAsync(domain, "NS");
        info.TxtRecords = await RunNslookupAsync(domain, "TXT");
        info.SoaRecords = await RunNslookupAsync(domain, "SOA");

        // CNAME
        try
        {
            var entry = await Dns.GetHostEntryAsync(domain);
            if (!entry.HostName.Equals(domain, StringComparison.OrdinalIgnoreCase))
                info.CnameRecords.Add(entry.HostName);
        }
        catch { }

        return info;
    }

    private static async Task<List<string>> RunNslookupAsync(string domain, string type)
    {
        var results = new List<string>();
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "nslookup",
                Arguments = $"-type={type} {domain}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) ||
                    trimmed.StartsWith("Servidor:") ||
                    trimmed.StartsWith("Address:") ||
                    trimmed.StartsWith("Name:") ||
                    trimmed.StartsWith("---") ||
                    trimmed.Contains("can't find") ||
                    trimmed.Contains("Non-authoritative answer:"))
                    continue;

                results.Add(trimmed);
            }
        }
        catch { }

        return results.Distinct().ToList();
    }
}