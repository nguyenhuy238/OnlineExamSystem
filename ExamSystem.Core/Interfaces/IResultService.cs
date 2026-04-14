using ExamSystem.Core.Models;

namespace ExamSystem.Core.Interfaces;

public interface IResultService
{
    Task<ExamResult> SubmitExamAsync(
        int studentId,
        int examId,
        Dictionary<int, char> answers,
        int durationSeconds = 0,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExamResult>> GetResultsByStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResult>> GetResultsByExamAsync(int examId, CancellationToken cancellationToken = default);
    Task<ExamResult?> GetResultWithDetailsAsync(int resultId, CancellationToken cancellationToken = default);
    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}
