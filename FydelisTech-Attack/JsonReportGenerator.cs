using FydelisTech_Attack.Core.Models;
using FydelisTech_Attack.Reports;
using System.IO;
using System.Text.Json;

namespace FydelisTech_Attack.Reports;

public class JsonReportGenerator : IReportGenerator
{
    public string FormatName => "JSON";
    public string FileExtension => ".json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task<string> GenerateAsync(ReconResult result)
    {
        var json = JsonSerializer.Serialize(result, Options);
        return Task.FromResult(json);
    }

    public async Task SaveAsync(ReconResult result, string filePath)
    {
        var json = await GenerateAsync(result);
        await File.WriteAllTextAsync(filePath, json);
    }
}