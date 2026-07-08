using FydelisTech_Attack.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FydelisTech_Attack.WPF.Views;

public partial class DashboardView : UserControl
{
    public ObservableCollection<string> LogEntries { get; } = new();

    public DashboardView()
    {
        InitializeComponent();
        LogListBox.ItemsSource = LogEntries;
    }

    public void LoadResult(ReconResult result)
    {
        DomainText.Text = result.Domain;
        SubdomainCount.Text = result.TotalSubdomains.ToString();
        LiveHostCount.Text = result.LiveHosts.ToString();
        HttpCount.Text = result.TotalHttpServices.ToString();
        DurationText.Text = result.ScanDuration.ToString(@"mm\:ss");

        if (result.HasWaf)
        {
            WafStatus.Text = result.WafInfo.WafName;
            WafStatus.Foreground = FindResource("AccentRedBrush") as System.Windows.Media.SolidColorBrush
                ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
        }
        else
        {
            WafStatus.Text = "Nenhum";
            WafStatus.Foreground = FindResource("AccentGreenBrush") as System.Windows.Media.SolidColorBrush
                ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
        }
    }

    public void Clear()
    {
        DomainText.Text = "-";
        SubdomainCount.Text = "0";
        LiveHostCount.Text = "0";
        HttpCount.Text = "0";
        WafStatus.Text = "Nenhum";
        DurationText.Text = "00:00";
        LogEntries.Clear();
    }

    public void AddLog(string message)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => AddLog(message));
            return;
        }

        LogEntries.Add(message);

        // Auto-scroll para o final
        if (LogListBox.Items.Count > 0)
        {
            var lastIndex = LogListBox.Items.Count - 1;
            LogListBox.ScrollIntoView(LogListBox.Items[lastIndex]);
        }
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e)
    {
        LogEntries.Clear();
    }
}