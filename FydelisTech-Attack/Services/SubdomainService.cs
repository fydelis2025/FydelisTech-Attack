using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using FydelisTech_Attack.Core.Interfaces;
using FydelisTech_Attack.Core.Models;

namespace FydelisTech_Attack.Core.Services;

public class SubdomainService
{
    private readonly HttpClient _httpClient;
    private readonly int _maxConcurrency;
    private readonly int _timeoutMs;

    public SubdomainService(int timeoutMs = 5000, int maxConcurrency = 50)
    {
        _timeoutMs = timeoutMs;
        _maxConcurrency = maxConcurrency;

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(_timeoutMs)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<List<SubdomainResult>> DiscoverAsync(string domain, List<string>? wordlist = null, 
        IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        wordlist ??= GetDefaultWordlist();
        var results = new List<SubdomainResult>();
        var semaphore = new SemaphoreSlim(_maxConcurrency);
        var tasks = new List<Task>();

        var scanProgress = new ScanProgress
        {
            CurrentPhase = 2,
            PhaseName = "Subdomain Discovery",
            TotalItems = wordlist.Count,
            CurrentItem = 0
        };

        foreach (var sub in wordlist)
        {
            if (ct.IsCancellationRequested) break;

            await semaphore.WaitAsync(ct);
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var hostname = $"{sub}.{domain}";
                    var result = new SubdomainResult { Hostname = hostname };

                    // DNS resolution
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        var addresses = await Dns.GetHostAddressesAsync(hostname);
                        result.Ip = addresses
                            .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                            .ToString() ?? addresses.FirstOrDefault()?.ToString();
                        result.IsAlive = result.Ip != null;
                    }
                    catch
                    {
                        result.IsAlive = false;
                    }
                    sw.Stop();
                    result.ResponseTimeMs = (int)sw.ElapsedMilliseconds;

                    // HTTP probe if alive
                    if (result.IsAlive)
                    {
                        await ProbeHttpAsync(result);
                    }

                    lock (results) results.Add(result);

                    scanProgress.CurrentItem++;
                    progress?.Report(scanProgress);
                }
                finally
                {
                    semaphore.Release();
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
        return results.OrderBy(r => r.Hostname).ToList();
    }

    private async Task ProbeHttpAsync(SubdomainResult result)
    {
        foreach (var scheme in new[] { "https", "http" })
        {
            try
            {
                var url = $"{scheme}://{result.Hostname}";
                var response = await _httpClient.GetAsync(url);
                result.HttpStatus = (int)response.StatusCode;

                var body = await response.Content.ReadAsStringAsync();

                // Extract title
                var titleMatch = Regex.Match(body, @"<title>\s*(.*?)\s*</title>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (titleMatch.Success && string.IsNullOrEmpty(result.Title))
                    result.Title = WebUtility.HtmlDecode(titleMatch.Groups[1].Value).Trim();

                // Detect technology
                if (response.Headers.Server?.Any() == true)
                {
                    var server = string.Join(", ", response.Headers.Server.Select(s => s.Product?.Name));
                    if (!string.IsNullOrEmpty(server))
                        result.Technology = server;
                }

                if (scheme == "https" && result.HttpStatus < 400)
                    break;
            }
            catch { }
        }
    }

    public static List<string> GetDefaultWordlist()
    {
        return new List<string>
        {
            "www", "mail", "remote", "blog", "webmail", "server", "ns1", "ns2",
            "smtp", "pop", "imap", "admin", "cpanel", "whm", "autodiscover",
            "ftp", "ssh", "vpn", "gitlab", "jenkins", "jira", "confluence",
            "portal", "app", "api", "dev", "test", "stage", "prod", "homolog",
            "dashboard", "painel", "manager", "management", "console", "panel",
            "owa", "exchange", "lyncdiscover", "sip", "meet", "teams",
            "docs", "wiki", "help", "support", "ticket", "suporte",
            "download", "uploads", "files", "storage", "cdn", "assets",
            "static", "img", "images", "css", "js", "fonts",
            "monitor", "zabbix", "nagios", "grafana", "prometheus",
            "rancher", "kubernetes", "k8s", "docker", "registry",
            "login", "signin", "auth", "sso", "logout", "register",
            "search", "sitemap", "robots", "crossdomain", "clientaccess",
            "redir", "redirect", "proxy", "relay", "mx",
            "labs", "beta", "alpha", "demo", "sandbox",
            "correio", "email", "webmail", "webmaill", "webemail",
            "firewall", "fortinet", "paloalto", "sonicwall",
            "radius", "nac", "8021x", "captive",
            "phone", "voip", "asterisk", "freepbx", "pbx",
            "erp", "sap", "oracle", "sql", "database", "db",
            "backup", "backups", "bkp", "restore",
            "status", "health", "healthcheck", "heartbeat",
            "vpn", "vpn1", "vpn2", "vpn3", "remoteaccess",
            "rdp", "terminal", "remoteapp", "tsweb",
            "lync", "skype", "teams", "zoom", "webex",
            "sharepoint", "sp", "sites", "teamsites",
            "crm", "dynamics", "customer",
            "intranet", "extranet", "partner",
            "training", "learn", "moodle", "canvas",
            "bugzilla", "mantis", "jira", "redmine",
            "svn", "git", "gitlab", "github", "bitbucket",
            "nexus", "nexus3", "artifactory", "maven",
            "sonar", "sonarqube", "codequality",
            "jenkins", "build", "ci", "cd", "pipeline",
            "docker", "registry", "k8s", "kubernetes",
            "rancher", "portainer", "swarm",
            "puppet", "chef", "ansible", "salt",
            "nagios", "zabbix", "cacti", "munin",
            "grafana", "prometheus", "kibana", "elastic",
            "kibana", "elasticsearch", "logstash",
            "splunk", "splunkweb", "splunkd",
            "stats", "statistics", "analytics", "piwik",
            "matomo", "googleanalytics",
            "ads", "adserver", "advertising",
            "track", "tracking", "tracker",
            "geo", "geoserver", "maps", "mapserver",
            "video", "videos", "stream", "streaming",
            "audio", "music", "radio", "podcast",
            "chat", "conference", "talk", "discuss",
            "forum", "board", "community", "groups",
            "wiki", "mediawiki", "confluence",
            "news", "press", "pr", "media",
            "shop", "store", "cart", "checkout",
            "payment", "pay", "pagamento", "billing",
            "secure", "security", "ssl", "certificates",
            "license", "licensing", "activation",
            "update", "updates", "upgrade", "patch",
            "download", "uploads", "files", "file",
            "img", "images", "cdn", "media",
            "static", "assets", "resources",
            "api", "api1", "api2", "api3",
            "rest", "soap", "graphql", "webhook",
            "mobile", "android", "ios", "app",
            "m", "mobileapp", "mobil",
            "labs", "lab", "beta", "alpha",
            "stage", "staging", "dev", "development",
            "test", "testing", "qa", "quality",
            "demo", "sandbox", "trial",
            "prod", "production", "live"
        };
    }
}