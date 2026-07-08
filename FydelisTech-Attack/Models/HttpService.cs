namespace FydelisTech_Attack.Core.Models;

public class HttpService
{
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? ServerHeader { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public List<string> ResponseHeaders { get; set; } = new();
    public string? BodySnippet { get; set; }
    public bool SupportsHttps { get; set; }
}