namespace ExamSystem.Core.Interfaces;

public interface IExportService
{
    Task<string> ExportResultsToJsonAsync(string filePath, CancellationToken cancellationToken = default);
}
