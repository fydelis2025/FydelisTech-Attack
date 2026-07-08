namespace FydelisTech_Attack.Core.Models;

public class ReconResult
{
    public string Domain { get; set; } = string.Empty;
    public DateTime ScanDate { get; set; } = DateTime.UtcNow;
    public DnsInfo DnsInfo { get; set; } = new();
    public List<SubdomainResult> Subdomains { get; set; } = new();
    public List<HttpService> HttpServices { get; set; } = new();
    public WafResult WafInfo { get; set; } = new();
    
    public int TotalSubdomains => Subdomains.Count;
    public int LiveHosts => Subdomains.Count(s => s.IsAlive);
    public int TotalHttpServices => HttpServices.Count;
    public bool HasWaf => WafInfo.HasWaf;
    public TimeSpan ScanDuration { get; set; }
}