using FydelisTech_Attack.Core.Models;
using FydelisTech_Attack.Reports;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FydelisTech_Attack.WPF.Views;

public partial class ReportView : UserControl
{
    private ReconResult? _lastResult;
    private readonly JsonReportGenerator _jsonReport = new();
    private readonly HtmlReportGenerator _htmlReport = new();
    private string? _lastExportPath;

    public ReportView()
    {
        InitializeComponent();
    }

    public void LoadResult(ReconResult result)
    {
        _lastResult = result;
        DomainLabel.Text = result.Domain;
        ScanDateLabel.Text = $"Scan realizado em {result.ScanDate:yyyy-MM-dd HH:mm:ss UTC}";

        // Gera pré-visualização em texto
        var preview = GenerateTextPreview(result);
        ReportPreview.Text = preview;
    }

    public void Clear()
    {
        _lastResult = null;
        DomainLabel.Text = "Nenhum";
        ScanDateLabel.Text = "";
        ReportPreview.Text = "";
        LastExportPath.Text = "Nenhum";
    }

    private string GenerateTextPreview(ReconResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════╗");
        sb.AppendLine("║           ATTACKSURFACE - RELATÓRIO        ║");
        sb.AppendLine("╚══════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"  Domínio:    {result.Domain}");
        sb.AppendLine($"  Data:       {result.ScanDate:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"  Duração:    {result.ScanDuration.TotalSeconds:F1}s");
        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine("  RESUMO");
        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine($"  Subdomínios:     {result.TotalSubdomains}");
        sb.AppendLine($"  Hosts Vivos:     {result.LiveHosts}");
        sb.AppendLine($"  Serviços HTTP:   {result.TotalHttpServices}");
        sb.AppendLine($"  WAF:             {(result.HasWaf ? result.WafInfo.WafName + " (" + result.WafInfo.WafVendor + ")" : "Nenhum")}");
        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine("  DNS");
        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine($"  A:     {string.Join(", ", result.DnsInfo.ARecords.DefaultIfEmpty("Nenhum"))}");
        sb.AppendLine($"  MX:    {string.Join(", ", result.DnsInfo.MxRecords.DefaultIfEmpty("Nenhum"))}");
        sb.AppendLine($"  NS:    {string.Join(", ", result.DnsInfo.NsRecords.DefaultIfEmpty("Nenhum"))}");
        sb.AppendLine($"  TXT:   {string.Join(", ", result.DnsInfo.TxtRecords.DefaultIfEmpty("Nenhum"))}");
        sb.AppendLine();

        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine("  SUBDOMÍNIOS VIVOS");
        sb.AppendLine("═══════════════════════════════════════════════");
        foreach (var sub in result.Subdomains.Where(s => s.IsAlive).Take(20))
        {
            sb.AppendLine($"  ✅ {sub.Hostname,-35} {sub.Ip,-15} HTTP {sub.HttpStatus}");
        }
        if (result.Subdomains.Count(s => s.IsAlive) > 20)
            sb.AppendLine($"  ... e mais {result.Subdomains.Count(s => s.IsAlive) - 20} hosts vivos");
        sb.AppendLine();

        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine("  SERVIÇOS HTTP");
        sb.AppendLine("═══════════════════════════════════════════════");
        foreach (var svc in result.HttpServices.Take(15))
        {
            var statusColor = svc.StatusCode < 400 ? "OK" : "ERRO";
            sb.AppendLine($"  [{statusColor}] {svc.Url,-45} {svc.StatusCode}");
        }
        if (result.HttpServices.Count > 15)
            sb.AppendLine($"  ... e mais {result.HttpServices.Count - 15} serviços");
        sb.AppendLine();

        if (result.HasWaf)
        {
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine("  WAF DETECTADO!");
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine($"  WAF: {result.WafInfo.WafName} ({result.WafInfo.WafVendor})");
            foreach (var sig in result.WafInfo.DetectedSignatures)
                sb.AppendLine($"  ⚠️  {sig}");
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine("  Gerado por AttackSurface - by FydeliStech");
        sb.AppendLine("═══════════════════════════════════════════════");

        return sb.ToString();
    }

    private async void ExportJson_Click(object sender, RoutedEventArgs e)
    {
        if (_lastResult == null)
        {
            MessageBox.Show("Nenhum resultado para exportar.", "AttackSurface",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            DefaultExt = ".json",
            FileName = $"attacksurface_{_lastResult.Domain}_{DateTime.Now:yyyyMMdd_HHmmss}.json"
        };

        if (dialog.ShowDialog() == true)
        {
            await _jsonReport.SaveAsync(_lastResult, dialog.FileName);
            _lastExportPath = dialog.FileName;
            LastExportPath.Text = _lastExportPath;
            MessageBox.Show($"JSON exportado com sucesso!\n{_lastExportPath}", "AttackSurface",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void ExportHtml_Click(object sender, RoutedEventArgs e)
    {
        if (_lastResult == null)
        {
            MessageBox.Show("Nenhum resultado para exportar.", "AttackSurface",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "HTML Files (*.html)|*.html",
            DefaultExt = ".html",
            FileName = $"attacksurface_{_lastResult.Domain}_{DateTime.Now:yyyyMMdd_HHmmss}.html"
        };

        if (dialog.ShowDialog() == true)
        {
            await _htmlReport.SaveAsync(_lastResult, dialog.FileName);
            _lastExportPath = dialog.FileName;
            LastExportPath.Text = _lastExportPath;
            MessageBox.Show($"HTML exportado com sucesso!\n{_lastExportPath}", "AttackSurface",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CopyReport_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ReportPreview.Text))
        {
            Clipboard.SetText(ReportPreview.Text);
            MessageBox.Show("Relatório copiado para a área de transferência.", "AttackSurface",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_lastExportPath))
        {
            var folder = Path.GetDirectoryName(_lastExportPath);
            if (Directory.Exists(folder))
                Process.Start("explorer.exe", folder);
        }
        else
        {
            var reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
            Directory.CreateDirectory(reportsDir);
            Process.Start("explorer.exe", reportsDir);
        }
    }
}