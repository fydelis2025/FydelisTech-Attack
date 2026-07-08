using FydelisTech_Attack.Core.Interfaces;
using FydelisTech_Attack.Core.Models;
using FydelisTech_Attack.Core.Services;
using FydelisTech_Attack.Reports;
using FydelisTech_Attack.WPF.Views;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace FydelisTech_Attack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly IScannerService _scanner;
        private readonly IReportGenerator _jsonReport;
        private readonly IReportGenerator _htmlReport;
        private readonly ExploitSuiteView _exploitView;
        private CancellationTokenSource? _cts;
        private ReconResult? _lastResult;
        private readonly Stopwatch _scanTimer = new();
        
        // Views
        private readonly DashboardView _dashboardView;
        private readonly DnsView _dnsView;
        private readonly SubdomainView _subdomainView;
        private readonly HttpView _httpView;
        private readonly WafView _wafView;
        private readonly ReportView _reportView;
        public MainWindow()
        {
            InitializeComponent();

            _scanner = new ReconEngine(timeoutMs: 5000, maxConcurrency: 50);
            _jsonReport = new JsonReportGenerator();
            _htmlReport = new HtmlReportGenerator();

            // Inicializa views
            _dashboardView = new DashboardView();
            _dnsView = new DnsView();
            _subdomainView = new SubdomainView();
            _httpView = new HttpView();
            _wafView = new WafView();
            _reportView = new ReportView();
            _exploitView = new ExploitSuiteView();


            // View inicial
            MainContentArea.Content = _dashboardView;

            // Eventos de navegação
            NavDashboard.Checked += (_, _) => MainContentArea.Content = _dashboardView;
            NavDns.Checked += (_, _) => MainContentArea.Content = _dnsView;
            NavSubdomain.Checked += (_, _) => MainContentArea.Content = _subdomainView;
            NavHttp.Checked += (_, _) => MainContentArea.Content = _httpView;
            NavWaf.Checked += (_, _) => MainContentArea.Content = _wafView;
            NavReports.Checked += (_, _) => MainContentArea.Content = _reportView;
            NavExploit.Checked += (_, _) => MainContentArea.Content = _exploitView;
            

            UpdateStatus("Pronto. Insira um domínio e clique em INICIAR SCAN.");
        }

        private async void StartScan_Click(object sender, RoutedEventArgs e)
        {
            var target = TargetTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(target))
            {
                MessageBox.Show("Informe um domínio alvo.", "AttackSurface", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Estado da UI
            SetScanState(true);
            _cts = new CancellationTokenSource();
            _scanTimer.Restart();
            _lastResult = null;

            ClearResults();
            UpdateStatus($"Iniciando scan em {target}...");
            LogMessage($"[+] Scan iniciado para {target}");
            LogMessage("[*] Fase 1: DNS Enumeration");

            try
            {
                var progress = new Progress<ScanProgress>(OnScanProgress);

                var result = await _scanner.FullScanAsync(target,
                    wordlist: null,
                    progress: progress,
                    ct: _cts.Token);

                _lastResult = result;
                _scanTimer.Stop();

                // Atualiza todas as views
                UpdateAllViews(result);
                UpdateStatusBar(result);

                LogMessage($"[✅] Scan concluído em {result.ScanDuration.TotalSeconds:F1}s");
                LogMessage($"[+] {result.TotalSubdomains} subdomínios encontrados, {result.LiveHosts} vivos");
                LogMessage($"[+] {result.TotalHttpServices} serviços HTTP detectados");
                LogMessage(result.HasWaf
                    ? $"[⚠️] WAF detectado: {result.WafInfo.WafName}"
                    : "[✅] Nenhum WAF detectado");

                // Muda para o dashboard automaticamente
                NavDashboard.IsChecked = true;
            }
            catch (OperationCanceledException)
            {
                LogMessage("[!] Scan cancelado pelo usuário");
                UpdateStatus("Scan cancelado.");
            }
            catch (Exception ex)
            {
                LogMessage($"[-] Erro: {ex.Message}");
                UpdateStatus($"Erro: {ex.Message}");
            }
            finally
            {
                SetScanState(false);
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void StopScan_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            UpdateStatus("Parando scan...");
        }

        private void ClearResults_Click(object sender, RoutedEventArgs e)
        {
            ClearResults();
            _lastResult = null;
            UpdateStatus("Resultados limpos.");
            UpdateStatusBar(null);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Futuro: janela de configurações (timeout, threads, wordlist)
            MessageBox.Show("Configurações disponíveis em breve.", "AttackSurface",
                MessageBoxButton.OK, MessageBoxImage.Information);
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
                LogMessage($"[+] JSON exportado: {dialog.FileName}");
                UpdateStatus($"JSON salvo em {dialog.FileName}");
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
                LogMessage($"[+] HTML exportado: {dialog.FileName}");
                UpdateStatus($"HTML salvo em {dialog.FileName}");
            }
        }

        private void OpenReportFolder_Click(object sender, RoutedEventArgs e)
        {
            var reportsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
            Directory.CreateDirectory(reportsDir);
            Process.Start("explorer.exe", reportsDir);
        }

        // ===== Métodos auxiliares =====

        private void SetScanState(bool isScanning)
        {
            ScanButton.IsEnabled = !isScanning;
            StopButton.IsEnabled = isScanning;
            TargetTextBox.IsEnabled = !isScanning;
        }

        private void ClearResults()
        {
            _dashboardView.Clear();
            _dnsView.Clear();
            _subdomainView.Clear();
            _httpView.Clear();
            _wafView.Clear();
            _reportView.Clear();

            ScanProgress.Value = 0;
            HostCountText.Text = "Hosts: 0";
            ServiceCountText.Text = "Serviços: 0";
            TimerText.Text = "00:00";
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _dashboardView.AddLog($"[{timestamp}] {message}");
        }

        private void OnScanProgress(ScanProgress progress)
        {
            var percentage = progress.Percentage;
            ScanProgress.Value = percentage > 0 ? percentage : 0;

            if (!string.IsNullOrEmpty(progress.PhaseName))
            {
                var msg = progress.TotalItems > 0
                    ? $"[{progress.PhaseName}] {progress.CurrentItem}/{progress.TotalItems}"
                    : $"[{progress.PhaseName}]";

                if (!string.IsNullOrEmpty(progress.CurrentTarget))
                    msg += $" - {progress.CurrentTarget}";

                UpdateStatus(msg);
            }

            TimerText.Text = _scanTimer.Elapsed.ToString(@"mm\:ss");
        }

        private void UpdateAllViews(ReconResult result)
        {
            _dashboardView.LoadResult(result);
            _dnsView.LoadResult(result);
            _subdomainView.LoadResult(result);
            _httpView.LoadResult(result);
            _wafView.LoadResult(result);
            _reportView.LoadResult(result);
        }

        private void UpdateStatusBar(ReconResult? result)
        {
            if (result == null)
            {
                HostCountText.Text = "Hosts: 0";
                ServiceCountText.Text = "Serviços: 0";
                TimerText.Text = "00:00";
                ScanProgress.Value = 0;
                return;
            }

            HostCountText.Text = $"Hosts: {result.LiveHosts}/{result.TotalSubdomains}";
            ServiceCountText.Text = $"Serviços: {result.TotalHttpServices}";
            TimerText.Text = result.ScanDuration.ToString(@"mm\：ss");
            ScanProgress.Value = 100;
        }
    }
}
