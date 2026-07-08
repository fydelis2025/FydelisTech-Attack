using System.Diagnostics;
using FydelisTech_Attack.Core.Interfaces;
using FydelisTech_Attack.Core.Models;

namespace FydelisTech_Attack.Core.Services;

public class ReconEngine : IScannerService
{
    private readonly DnsService _dnsService;
    private readonly SubdomainService _subdomainService;
    private readonly HttpProbeService _httpProbeService;
    private readonly WafDetectionService _wafService;

    public ReconEngine(int timeoutMs = 5000, int maxConcurrency = 50)
    {
        _dnsService = new DnsService();
        _subdomainService = new SubdomainService(timeoutMs, maxConcurrency);
        _httpProbeService = new HttpProbeService(timeoutMs);
        _wafService = new WafDetectionService(timeoutMs);
    }

    public async Task<DnsInfo> EnumerateDnsAsync(string domain)
    {
        return await _dnsService.EnumerateAsync(domain);
    }

    public async Task<List<SubdomainResult>> DiscoverSubdomainsAsync(string domain, List<string>? wordlist = null,
        CancellationToken ct = default)
    {
        return await _subdomainService.DiscoverAsync(domain, wordlist, ct: ct);
    }

    public async Task<List<HttpService>> ScanHttpServicesAsync(string domain, List<SubdomainResult> subdomains,
        CancellationToken ct = default)
    {
        return await _httpProbeService.ScanAsync(domain, subdomains, ct: ct);
    }

    public async Task<WafResult> DetectWafAsync(string domain, CancellationToken ct = default)
    {
        return await _wafService.DetectAsync(domain, ct);
    }

    public async Task<ReconResult> FullScanAsync(string domain, List<string>? wordlist = null,
        IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new ReconResult { Domain = domain };

        // Fase 1: DNS
        progress?.Report(new ScanProgress { CurrentPhase = 1, PhaseName = "DNS Enumeration" });
        result.DnsInfo = await _dnsService.EnumerateAsync(domain);

        // Fase 2: Subdomínios
        progress?.Report(new ScanProgress { CurrentPhase = 2, PhaseName = "Subdomain Discovery" });
        result.Subdomains = await _subdomainService.DiscoverAsync(domain, wordlist, progress, ct);

        // Fase 3: HTTP
        progress?.Report(new ScanProgress { CurrentPhase = 3, PhaseName = "HTTP Probing" });
        result.HttpServices = await _httpProbeService.ScanAsync(domain, result.Subdomains, progress, ct);

        // Fase 4: WAF
        progress?.Report(new ScanProgress { CurrentPhase = 4, PhaseName = "WAF Detection" });
        result.WafInfo = await _wafService.DetectAsync(domain, ct);

        sw.Stop();
        result.ScanDuration = sw.Elapsed;
        return result;
    }
}