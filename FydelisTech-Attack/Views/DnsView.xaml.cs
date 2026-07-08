using FydelisTech_Attack.Core.Models;
using System.Windows.Controls;

namespace FydelisTech_Attack.WPF.Views;

public partial class DnsView : UserControl
{
    public DnsView()
    {
        InitializeComponent();
    }

    public void LoadResult(ReconResult result)
    {
        DomainText.Text = result.Domain;

        ARecordList.ItemsSource = result.DnsInfo.ARecords.Count > 0
            ? result.DnsInfo.ARecords
            : new List<string> { "Nenhum registro A encontrado" };

        MxRecordList.ItemsSource = result.DnsInfo.MxRecords.Count > 0
            ? result.DnsInfo.MxRecords
            : new List<string> { "Nenhum registro MX encontrado" };

        NsRecordList.ItemsSource = result.DnsInfo.NsRecords.Count > 0
            ? result.DnsInfo.NsRecords
            : new List<string> { "Nenhum registro NS encontrado" };

        TxtRecordList.ItemsSource = result.DnsInfo.TxtRecords.Count > 0
            ? result.DnsInfo.TxtRecords
            : new List<string> { "Nenhum registro TXT encontrado" };

        CnameRecordList.ItemsSource = result.DnsInfo.CnameRecords.Count > 0
            ? result.DnsInfo.CnameRecords
            : new List<string> { "Nenhum registro CNAME encontrado" };
    }

    public void Clear()
    {
        DomainText.Text = "-";
        ARecordList.ItemsSource = null;
        MxRecordList.ItemsSource = null;
        NsRecordList.ItemsSource = null;
        TxtRecordList.ItemsSource = null;
        CnameRecordList.ItemsSource = null;
    }
}