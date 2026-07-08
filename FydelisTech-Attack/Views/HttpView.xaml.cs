using FydelisTech_Attack.Core.Models;
using System.Windows.Controls;

namespace FydelisTech_Attack.WPF.Views;

public partial class HttpView : UserControl
{
    public HttpView()
    {
        InitializeComponent();
    }

    public void LoadResult(ReconResult result)
    {
        TotalCount.Text = result.TotalHttpServices.ToString();
        SuccessCount.Text = result.HttpServices.Count(s => s.StatusCode >= 200 && s.StatusCode < 300).ToString();
        ErrorCount.Text = result.HttpServices.Count(s => s.StatusCode >= 300).ToString();

        HttpGrid.ItemsSource = result.HttpServices;
    }

    public void Clear()
    {
        TotalCount.Text = "0";
        SuccessCount.Text = "0";
        ErrorCount.Text = "0";
        HttpGrid.ItemsSource = null;
    }
}