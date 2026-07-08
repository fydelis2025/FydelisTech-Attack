using FydelisTech_Attack.Core.Models;

namespace FydelisTech_Attack.Reports;

public interface IReportGenerator
{
    Task<string> GenerateAsync(ReconResult result);
    Task SaveAsync(ReconResult result, string filePath);
    string FormatName { get; }
    string FileExtension { get; }
}