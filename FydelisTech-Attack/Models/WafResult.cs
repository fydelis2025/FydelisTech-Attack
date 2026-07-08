namespace FydelisTech_Attack.Core.Models;

public class WafResult
{
    public bool HasWaf { get; set; }
    public string WafName { get; set; } = "Nenhum";
    public string WafVendor { get; set; } = string.Empty;
    public List<string> DetectedSignatures { get; set; } = new();
    public DetectionConfidence Confidence { get; set; } = DetectionConfidence.Low;
}

public enum DetectionConfidence { Low, Medium, High, Certain }