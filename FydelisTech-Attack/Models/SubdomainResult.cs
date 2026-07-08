namespace FydelisTech_Attack.Core.Models;

public class SubdomainResult
{
    public string Hostname { get; set; } = string.Empty;
    public string? Ip { get; set; }
    public bool IsAlive { get; set; }
    public int HttpStatus { get; set; }
    public string? Title { get; set; }
    public string? Technology { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? ScreenshotPath { get; set; } // Futuro
}