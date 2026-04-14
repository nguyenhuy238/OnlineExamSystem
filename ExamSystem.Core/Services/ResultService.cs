using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;

namespace ExamSystem.Core.Services;

public class ResultService : IResultService
{
    private readonly IExamRepository _examRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IResultRepository _resultRepository;

    public ResultService(
        IExamRepository examRepository,
        IStudentRepository studentRepository,
        IResultRepository resultRepository)
    {
        _examRepository = examRepository;
        _studentRepository = studentRepository;
        _resultRepository = resultRepository;
    }

    public async Task<ExamResult> SubmitExamAsync(
        int studentId,
        int examId,
        Dictionary<int, char> answers,
        int durationSeconds = 0,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Student with id {studentId} was not found.");
        var exam = await _examRepository.GetExamWithQuestionsAsync(examId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exam with id {examId} was not found.");

        var questions = exam.Questions.ToList();
        if (questions.Count == 0)
        {
            throw new InvalidOperationException("Exam has no questions.");
        }

        // Grade on background thread to showcase Task + async/await pattern.
        var details = await Task.Run(() =>
            questions.Select(q => new ResultDetail
            {
                QuestionId = q.Id,
                ChosenAnswer = answers.TryGetValue(q.Id, out var chosenAnswer) ? chosenAnswer : null,
                IsCorrect = answers.TryGetValue(q.Id, out var answer) && answer == q.CorrectAnswer
            }).ToList(), cancellationToken);

        var correctAnswers = details.Count(x => x.IsCorrect);
        var score = Math.Round((double)correctAnswers / questions.Count * 10, 2);

        var result = new ExamResult
        {
            StudentId = student.Id,
            ExamId = exam.Id,
            Score = score,
            IsPassed = score >= exam.PassScore,
            SubmittedAt = DateTime.UtcNow,
            Duration = durationSeconds,
            Details = details
        };

        await _resultRepository.AddAsync(result, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<ExamResult>> GetResultsByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _resultRepository.GetResultsByStudentAsync(studentId, cancellationToken);
    }

    public async Task<IReadOnlyList<ExamResult>> GetResultsByExamAsync(int examId, CancellationToken cancellationToken = default)
    {
        return await _resultRepository.GetResultsByExamAsync(examId, cancellationToken);
    }

    public async Task<ExamResult?> GetResultWithDetailsAsync(int resultId, CancellationToken cancellationToken = default)
    {
        return await _resultRepository.GetResultWithDetailsAsync(resultId, cancellationToken);
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetAllAsync(cancellationToken);
        var exams = await _examRepository.GetAllAsync(cancellationToken);
        var results = await _resultRepository.GetAllWithRelationsAsync(cancellationToken);

        var totalAttempts = results.Count;
        var passRate = totalAttempts == 0 ? 0 : results.Count(x => x.IsPassed) * 100.0 / totalAttempts;
        var averageScore = totalAttempts == 0 ? 0 : results.Average(x => x.Score);

        var topStudents = results
            .GroupBy(x => x.StudentId)
            .Select(group =>
            {
                var studentName = group.First().Student?.FullName
                    ?? students.FirstOrDefault(s => s.Id == group.Key)?.FullName
                    ?? "Unknown";

                return new TopStudentDto
                {
                    StudentId = group.Key,
                    FullName = studentName,
                    AvgScore = Math.Round(group.Average(x => x.Score), 2),
                    TotalExams = group.Count()
                };
            })
            .OrderByDescending(x => x.AvgScore)
            .Take(5)
            .ToList();

        return new DashboardStats
        {
            TotalStudents = students.Count,
            TotalExams = exams.Count,
            TotalAttempts = totalAttempts,
            PassRate = Math.Round(passRate, 2),
            AverageScore = Math.Round(averageScore, 2),
            TopStudents = topStudents
        };
    }
}
