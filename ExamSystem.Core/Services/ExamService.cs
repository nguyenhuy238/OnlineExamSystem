using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;

namespace ExamSystem.Core.Services;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;

    public ExamService(IExamRepository examRepository)
    {
        _examRepository = examRepository;
    }

    public async Task<IReadOnlyList<Exam>> GetAllExamsAsync(CancellationToken cancellationToken = default)
    {
        return await _examRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Exam>> GetActiveExamsAsync(CancellationToken cancellationToken = default)
    {
        return await _examRepository.GetActiveExamsAsync(cancellationToken);
    }

    public async Task<Exam?> GetExamWithQuestionsAsync(int examId, CancellationToken cancellationToken = default)
    {
        return await _examRepository.GetExamWithQuestionsAsync(examId, cancellationToken);
    }

    public async Task<Exam> CreateExamAsync(Exam exam, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(exam.Title))
        {
            throw new ArgumentException("Exam title is required.", nameof(exam));
        }

        exam.CreatedAt = DateTime.UtcNow;
        await _examRepository.AddAsync(exam, cancellationToken);
        return exam;
    }

    public async Task<Exam> UpdateExamAsync(Exam exam, CancellationToken cancellationToken = default)
    {
        var existingExam = await _examRepository.GetByIdAsync(exam.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Exam with id {exam.Id} was not found.");

        existingExam.Title = exam.Title;
        existingExam.Subject = exam.Subject;
        existingExam.Duration = exam.Duration;
        existingExam.PassScore = exam.PassScore;
        existingExam.IsActive = exam.IsActive;

        await _examRepository.UpdateAsync(existingExam, cancellationToken);
        return existingExam;
    }

    public async Task DeleteExamAsync(int examId, CancellationToken cancellationToken = default)
    {
        await _examRepository.DeleteAsync(examId, cancellationToken);
    }
}
