namespace FydelisTech_Attack.Core.Models;

public class DnsInfo
{
    public string Domain { get; set; } = string.Empty;
    public List<string> ARecords { get; set; } = new();
    public List<string> AaaaRecords { get; set; } = new();
    public List<string> MxRecords { get; set; } = new();
    public List<string> NsRecords { get; set; } = new();
    public List<string> TxtRecords { get; set; } = new();
    public List<string> CnameRecords { get; set; } = new();
    public List<string> SoaRecords { get; set; } = new();
}