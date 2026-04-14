using ExamSystem.Core.Models;

namespace ExamSystem.Core.Interfaces;

public interface IResultRepository : IRepository<ExamResult>
{
    Task<IReadOnlyList<ExamResult>> GetResultsByStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResult>> GetResultsByExamAsync(int examId, CancellationToken cancellationToken = default);
    Task<ExamResult?> GetResultWithDetailsAsync(int resultId, CancellationToken cancellationToken = default);
}
