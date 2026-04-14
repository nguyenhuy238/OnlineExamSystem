using ExamSystem.Core.Models;

namespace ExamSystem.Core.Interfaces;

public interface IQuestionService
{
    Task<IReadOnlyList<Question>> GetAllQuestionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetQuestionsByExamAsync(int examId, CancellationToken cancellationToken = default);
    Task<Question?> GetQuestionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Question> CreateQuestionAsync(Question question, CancellationToken cancellationToken = default);
    Task<Question> UpdateQuestionAsync(Question question, CancellationToken cancellationToken = default);
    Task DeleteQuestionAsync(int id, CancellationToken cancellationToken = default);
}
