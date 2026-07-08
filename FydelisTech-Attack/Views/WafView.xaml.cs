using FydelisTech_Attack.Core.Models;
using System.Windows.Controls;
using System.Windows.Media;

namespace FydelisTech_Attack.WPF.Views;

public partial class WafView : UserControl
{
    public WafView()
    {
        InitializeComponent();
    }

    public void LoadResult(ReconResult result)
    {
        if (result.HasWaf)
        {
            WafIconText.Text = "⚠️";
            WafIconBorder.Background = FindResource("ErrorBgBrush") as SolidColorBrush
                ?? new SolidColorBrush(Color.FromArgb(16, 255, 51, 51));
            WafTitle.Text = $"WAF Detectado: {result.WafInfo.WafName}";
            WafTitle.Foreground = FindResource("AccentRedBrush") as SolidColorBrush
                ?? new SolidColorBrush(Colors.Red);
            WafVendor.Text = $"Fornecedor: {result.WafInfo.WafVendor}";
            WafConfidence.Text = $"Confiança: {result.WafInfo.Confidence}";
        }
        else
        {
            WafIconText.Text = "✅";
            WafIconBorder.Background = FindResource("SuccessBgBrush") as SolidColorBrush
                ?? new SolidColorBrush(Color.FromArgb(16, 0, 255, 65));
            WafTitle.Text = "Nenhum WAF Detectado";
            WafTitle.Foreground = FindResource("AccentGreenBrush") as SolidColorBrush
                ?? new SolidColorBrush(Colors.Green);
            WafVendor.Text = "";
            WafConfidence.Text = "Confiança: Baixa";
        }

        SignatureList.ItemsSource = result.WafInfo.DetectedSignatures.Count > 0
            ? result.WafInfo.DetectedSignatures
            : new List<string> { "Nenhuma assinatura de WAF encontrada" };

        var payloads = new List<string>
        {
            $"https://{result.Domain}/?id=1' OR '1'='1",
            $"https://{result.Domain}/?id=<script>alert(1)</script>",
            $"https://{result.Domain}/?id=../../../etc/passwd",
            $"https://{result.Domain}/?id=1 UNION SELECT * FROM users",
            $"https://{result.Domain}/"
        };
        PayloadList.ItemsSource = payloads;
    }

    public void Clear()
    {
        WafIconText.Text = "✅";
        WafIconBorder.Background = FindResource("SuccessBgBrush") as SolidColorBrush
            ?? new SolidColorBrush(Color.FromArgb(16, 0, 255, 65));
        WafTitle.Text = "Nenhum WAF Detectado";
        WafTitle.Foreground = FindResource("AccentGreenBrush") as SolidColorBrush
            ?? new SolidColorBrush(Colors.Green);
        WafVendor.Text = "";
        WafConfidence.Text = "Confiança: Baixa";
        SignatureList.ItemsSource = null;
        PayloadList.ItemsSource = null;
    }
}