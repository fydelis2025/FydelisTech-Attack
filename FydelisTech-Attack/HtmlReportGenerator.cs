using FydelisTech_Attack.Core.Models;
using FydelisTech_Attack.Reports;
using System.IO;
using System.Text;

namespace FydelisTech_Attack.Reports;

public class HtmlReportGenerator : IReportGenerator
{
    public string FormatName => "HTML";
    public string FileExtension => ".html";

    public async Task<string> GenerateAsync(ReconResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine($"<title>AttackSurface — Relatório: {result.Domain}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(await GetStylesAsync());
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");

        // Header
        sb.AppendLine("<div class=\"header\">");
        sb.AppendLine("<h1>🔐 AttackSurface — Relatório OSINT</h1>");
        sb.AppendLine($"<p>Alvo: <strong>{result.Domain}</strong></p>");
        sb.AppendLine($"<p>Data: {result.ScanDate:yyyy-MM-dd HH:mm:ss UTC}</p>");
        sb.AppendLine($"<p>Duração: {result.ScanDuration.TotalSeconds:F1}s</p>");
        sb.AppendLine("</div>");

        // Summary cards
        sb.AppendLine("<div class=\"cards\">");
        sb.AppendLine(Card("Subdomínios", result.TotalSubdomains.ToString(), "#00FF41"));
        sb.AppendLine(Card("Hosts Vivos", result.LiveHosts.ToString(), "#00D4FF"));
        sb.AppendLine(Card("Serviços HTTP", result.TotalHttpServices.ToString(), "#FFD700"));
        sb.AppendLine(Card("WAF", result.HasWaf ? result.WafInfo.WafName : "Nenhum", result.HasWaf ? "#FF3333" : "#00FF41"));
        sb.AppendLine("</div>");

        // DNS Info
        sb.AppendLine("<div class=\"section\">");
        sb.AppendLine("<h2>📡 DNS Enumeration</h2>");
        sb.AppendLine(Table("A Records", result.DnsInfo.ARecords));
        sb.AppendLine(Table("MX Records", result.DnsInfo.MxRecords));
        sb.AppendLine(Table("NS Records", result.DnsInfo.NsRecords));
        sb.AppendLine(Table("TXT Records", result.DnsInfo.TxtRecords));
        sb.AppendLine("</div>");

        // Subdomains
        sb.AppendLine("<div class=\"section\">");
        sb.AppendLine("<h2>🌐 Subdomínios</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Hostname</th><th>IP</th><th>Status</th><th>HTTP</th><th>Title</th><th>Tech</th></tr>");
        foreach (var sub in result.Subdomains.OrderByDescending(s => s.IsAlive))
        {
            var status = sub.IsAlive ? "✅ Alive" : "❌ Dead";
            var httpStatus = sub.HttpStatus > 0 ? sub.HttpStatus.ToString() : "-";
            sb.AppendLine($"<tr><td>{sub.Hostname}</td><td>{sub.Ip ?? "-"}</td><td>{status}</td><td>{httpStatus}</td><td>{sub.Title ?? "-"}</td><td>{sub.Technology ?? "-"}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");

        // HTTP Services
        sb.AppendLine("<div class=\"section\">");
        sb.AppendLine("<h2>🌍 Serviços HTTP</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>URL</th><th>Status</th><th>Server</th><th>Content-Type</th></tr>");
        foreach (var svc in result.HttpServices)
        {
            sb.AppendLine($"<tr><td>{svc.Url}</td><td>{svc.StatusCode}</td><td>{svc.ServerHeader ?? "-"}</td><td>{svc.ContentType}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");

        // WAF
        sb.AppendLine("<div class=\"section\">");
        sb.AppendLine("<h2>🛡️ WAF Detection</h2>");
        if (result.HasWaf)
        {
            sb.AppendLine($"<p class=\"waf-detected\">⚠️ WAF Detectado: <strong>{result.WafInfo.WafName}</strong> ({result.WafInfo.WafVendor})</p>");
            sb.AppendLine("<ul>");
            foreach (var sig in result.WafInfo.DetectedSignatures)
                sb.AppendLine($"<li>{sig}</li>");
            sb.AppendLine("</ul>");
        }
        else
        {
            sb.AppendLine("<p class=\"waf-clean\">✅ Nenhum WAF detectado</p>");
        }
        sb.AppendLine("</div>");

        // Footer
        sb.AppendLine("<div class=\"footer\">");
        sb.AppendLine("<p>Gerado por <strong>AttackSurface</strong> — by FydeliStech</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    public async Task SaveAsync(ReconResult result, string filePath)
    {
        var html = await GenerateAsync(result);
        await File.WriteAllTextAsync(filePath, html);
    }

    private static string Card(string label, string value, string color)
    {
        return $@"
        <div class=""card"" style=""border-left: 4px solid {color}"">
            <div class=""card-value"" style=""color: {color}"">{value}</div>
            <div class=""card-label"">{label}</div>
        </div>";
    }

    private static string Table(string title, List<string> items)
    {
        if (items.Count == 0) return "";
        var sb = new StringBuilder();
        sb.AppendLine($"<h3>{title}</h3>");
        sb.AppendLine("<ul>");
        foreach (var item in items)
            sb.AppendLine($"<li>{item}</li>");
        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    private static async Task<string> GetStylesAsync()
    {
        // Em produção, isso viria de um arquivo CSS embutido como recurso
        return @"
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: #0D1117; color: #E6EDF3; padding: 20px; }
            .container { max-width: 1200px; margin: 0 auto; }
            .header { text-align: center; padding: 30px; border-bottom: 1px solid #30363D; margin-bottom: 30px; }
            .header h1 { color: #00FF41; font-size: 28px; margin-bottom: 10px; }
            .header p { color: #8B949E; margin: 5px 0; }
            .cards { display: flex; gap: 15px; flex-wrap: wrap; margin-bottom: 30px; }
            .card { background: #161B22; border: 1px solid #30363D; border-radius: 8px; padding: 20px; flex: 1; min-width: 180px; }
            .card-value { font-size: 32px; font-weight: bold; }
            .card-label { color: #8B949E; font-size: 14px; margin-top: 5px; }
            .section { background: #161B22; border: 1px solid #30363D; border-radius: 8px; padding: 20px; margin-bottom: 20px; }
            .section h2 { color: #00D4FF; font-size: 20px; margin-bottom: 15px; padding-bottom: 10px; border-bottom: 1px solid #30363D; }
            .section h3 { color: #8B949E; font-size: 14px; margin: 10px 0 5px; text-transform: uppercase; letter-spacing: 1px; }
            table { width: 100%; border-collapse: collapse; margin-top: 10px; font-size: 13px; }
            th { text-align: left; padding: 10px 8px; border-bottom: 2px solid #30363D; color: #8B949E; text-transform: uppercase; font-size: 11px; letter-spacing: 1px; }
            td { padding: 8px; border-bottom: 1px solid #21262D; }
            tr:hover td { background: #21262D; }
            ul { list-style: none; padding: 0; }
            ul li { padding: 4px 0; color: #8B949E; font-size: 13px; }
            ul li::before { content: '› '; color: #00FF41; }
            .waf-detected { color: #FF3333; padding: 10px; background: rgba(255, 51, 51, 0.1); border-radius: 4px; }
            .waf-clean { color: #00FF41; padding: 10px; background: rgba(0, 255, 65, 0.1); border-radius: 4px; }
            .footer { text-align: center; padding: 20px; color: #30363D; font-size: 12px; margin-top: 30px; border-top: 1px solid #30363D; }
        ";
    }
}