using FydelisTech_Attack.Core.Models;

namespace FydelisTech_Attack.Core.Interfaces;

public interface IScannerService
{
    Task<DnsInfo> EnumerateDnsAsync(string domain);
    Task<List<SubdomainResult>> DiscoverSubdomainsAsync(string domain, List<string>? wordlist = null, CancellationToken ct = default);
    Task<List<HttpService>> ScanHttpServicesAsync(string domain, List<SubdomainResult> subdomains, CancellationToken ct = default);
    Task<WafResult> DetectWafAsync(string domain, CancellationToken ct = default);
    Task<ReconResult> FullScanAsync(string domain, List<string>? wordlist = null, IProgress<ScanProgress>? progress = null, CancellationToken ct = default);
}

public class ScanProgress
{
    public int CurrentPhase { get; set; }
    public string PhaseName { get; set; } = string.Empty;
    public int CurrentItem { get; set; }
    public int TotalItems { get; set; }
    public string? CurrentTarget { get; set; }
    public double Percentage => TotalItems > 0 ? (double)CurrentItem / TotalItems * 100 : 0;
}