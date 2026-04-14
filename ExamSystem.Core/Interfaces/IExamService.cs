using ExamSystem.Core.Models;

namespace ExamSystem.Core.Interfaces;

public interface IExamService
{
    Task<IReadOnlyList<Exam>> GetAllExamsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Exam>> GetActiveExamsAsync(CancellationToken cancellationToken = default);
    Task<Exam?> GetExamWithQuestionsAsync(int examId, CancellationToken cancellationToken = default);
    Task<Exam> CreateExamAsync(Exam exam, CancellationToken cancellationToken = default);
    Task<Exam> UpdateExamAsync(Exam exam, CancellationToken cancellationToken = default);
    Task DeleteExamAsync(int examId, CancellationToken cancellationToken = default);
}
