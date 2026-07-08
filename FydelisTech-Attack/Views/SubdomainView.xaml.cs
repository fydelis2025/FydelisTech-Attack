using FydelisTech_Attack.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FydelisTech_Attack.WPF.Views;

public partial class SubdomainView : UserControl
{
    private List<SubdomainResult>? _allSubdomains;

    public SubdomainView()
    {
        InitializeComponent();
    }

    public void LoadResult(ReconResult result)
    {
        _allSubdomains = result.Subdomains;
        UpdateCounts(result);
        ApplyFilter();
    }

    public void Clear()
    {
        _allSubdomains = null;
        SubdomainGrid.ItemsSource = null;
        TotalCount.Text = "0";
        AliveCount.Text = "0";
        DeadCount.Text = "0";
    }

    private void UpdateCounts(ReconResult result)
    {
        TotalCount.Text = result.TotalSubdomains.ToString();
        AliveCount.Text = result.LiveHosts.ToString();
        DeadCount.Text = (result.TotalSubdomains - result.LiveHosts).ToString();
    }

    private void ApplyFilter()
    {
        if (_allSubdomains == null) return;

        var filter = (FilterCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
        IEnumerable<SubdomainResult> filtered = filter switch
        {
            "Vivos" => _allSubdomains.Where(s => s.IsAlive),
            "Mortos" => _allSubdomains.Where(s => !s.IsAlive),
            "Com HTTP" => _allSubdomains.Where(s => s.HttpStatus > 0),
            _ => _allSubdomains
        };

        SubdomainGrid.ItemsSource = filtered.ToList();
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }
}