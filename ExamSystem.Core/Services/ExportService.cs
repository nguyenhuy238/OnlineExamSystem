using System.Text.Json;
using System.Text.Json.Serialization;
using ExamSystem.Core.Interfaces;

namespace ExamSystem.Core.Services;

public class ExportService : IExportService
{
    private readonly IResultRepository _resultRepository;

    public ExportService(IResultRepository resultRepository)
    {
        _resultRepository = resultRepository;
    }

    public async Task<string> ExportResultsToJsonAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var results = await _resultRepository.GetAllWithRelationsAsync(cancellationToken);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });

        await File.WriteAllTextAsync(filePath, json, cancellationToken);
        return filePath;
    }
}
