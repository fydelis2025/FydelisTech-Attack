using System.Net;
using System.Net.Http;
using FydelisTech_Attack.Core.Interfaces;
using FydelisTech_Attack.Core.Models;

namespace FydelisTech_Attack.Core.Services;

public class HttpProbeService
{
    private readonly HttpClient _httpClient;

    public HttpProbeService(int timeoutMs = 5000)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(timeoutMs)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<List<HttpService>> ScanAsync(string domain, List<SubdomainResult> subdomains,
        IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        var services = new List<HttpService>();
        var hostsToScan = subdomains
            .Where(s => s.IsAlive)
            .Select(s => s.Hostname)
            .Distinct()
            .Take(50)
            .ToList();

        hostsToScan.Add(domain);

        var semaphore = new SemaphoreSlim(10);
        var tasks = new List<Task>();
        var scanProgress = new ScanProgress
        {
            CurrentPhase = 3,
            PhaseName = "HTTP Probing",
            TotalItems = hostsToScan.Count,
            CurrentItem = 0
        };

        foreach (var host in hostsToScan)
        {
            if (ct.IsCancellationRequested) break;

            await semaphore.WaitAsync(ct);
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    foreach (var scheme in new[] { "https", "http" })
                    {
                        try
                        {
                            var url = $"{scheme}://{host}";
                            var response = await _httpClient.GetAsync(url, ct);
                            var body = await response.Content.ReadAsStringAsync(ct);

                            var service = new HttpService
                            {
                                Url = url,
                                StatusCode = (int)response.StatusCode,
                                ContentType = response.Content.Headers.ContentType?.ToString() ?? "",
                                BodySnippet = body.Length > 200 ? body[..200] : body,
                                SupportsHttps = scheme == "https"
                            };

                            if (response.Headers.Server?.Any() == true)
                                service.ServerHeader = string.Join(", ", response.Headers.Server);

                            foreach (var h in response.Headers)
                                service.ResponseHeaders.Add($"{h.Key}: {string.Join(", ", h.Value)}");

                            lock (services) services.Add(service);

                            if (scheme == "https" && (int)response.StatusCode < 400)
                                break;
                        }
                        catch { }
                    }
                }
                finally
                {
                    semaphore.Release();
                    scanProgress.CurrentItem++;
                    progress?.Report(scanProgress);
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
        return services;
    }
}