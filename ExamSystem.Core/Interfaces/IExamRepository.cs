using ExamSystem.Core.Models;

namespace ExamSystem.Core.Interfaces;

public interface IExamRepository : IRepository<Exam>
{
    Task<IReadOnlyList<Exam>> GetActiveExamsAsync(CancellationToken cancellationToken = default);
    Task<Exam?> GetExamWithQuestionsAsync(int examId, CancellationToken cancellationToken = default);
}
