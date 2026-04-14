using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;

namespace ExamSystem.Core.Services;

public class QuestionService : IQuestionService
{
    private readonly IRepository<Question> _questionRepository;
    private readonly IExamRepository _examRepository;

    public QuestionService(IRepository<Question> questionRepository, IExamRepository examRepository)
    {
        _questionRepository = questionRepository;
        _examRepository = examRepository;
    }

    public async Task<IReadOnlyList<Question>> GetAllQuestionsAsync(CancellationToken cancellationToken = default)
    {
        var questions = await _questionRepository.GetAllAsync(cancellationToken);
        return questions.OrderBy(x => x.ExamId).ThenBy(x => x.Id).ToList();
    }

    public async Task<IReadOnlyList<Question>> GetQuestionsByExamAsync(int examId, CancellationToken cancellationToken = default)
    {
        var questions = await _questionRepository.GetAllAsync(cancellationToken);
        return questions.Where(x => x.ExamId == examId).OrderBy(x => x.Id).ToList();
    }

    public async Task<Question?> GetQuestionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _questionRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Question> CreateQuestionAsync(Question question, CancellationToken cancellationToken = default)
    {
        await EnsureExamExists(question.ExamId, cancellationToken);
        ValidateQuestion(question);

        await _questionRepository.AddAsync(question, cancellationToken);
        return question;
    }

    public async Task<Question> UpdateQuestionAsync(Question question, CancellationToken cancellationToken = default)
    {
        await EnsureExamExists(question.ExamId, cancellationToken);

        var existingQuestion = await _questionRepository.GetByIdAsync(question.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Question with id {question.Id} was not found.");

        ValidateQuestion(question);

        existingQuestion.ExamId = question.ExamId;
        existingQuestion.Content = question.Content;
        existingQuestion.OptionA = question.OptionA;
        existingQuestion.OptionB = question.OptionB;
        existingQuestion.OptionC = question.OptionC;
        existingQuestion.OptionD = question.OptionD;
        existingQuestion.CorrectAnswer = char.ToUpperInvariant(question.CorrectAnswer);

        await _questionRepository.UpdateAsync(existingQuestion, cancellationToken);
        return existingQuestion;
    }

    public async Task DeleteQuestionAsync(int id, CancellationToken cancellationToken = default)
    {
        await _questionRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task EnsureExamExists(int examId, CancellationToken cancellationToken)
    {
        var exam = await _examRepository.GetByIdAsync(examId, cancellationToken);
        if (exam is null)
        {
            throw new InvalidOperationException($"Exam with id {examId} does not exist.");
        }
    }

    private static void ValidateQuestion(Question question)
    {
        if (string.IsNullOrWhiteSpace(question.Content))
        {
            throw new ArgumentException("Question content is required.", nameof(question));
        }

        if (string.IsNullOrWhiteSpace(question.OptionA)
            || string.IsNullOrWhiteSpace(question.OptionB)
            || string.IsNullOrWhiteSpace(question.OptionC)
            || string.IsNullOrWhiteSpace(question.OptionD))
        {
            throw new ArgumentException("All options must be provided.", nameof(question));
        }

        var answer = char.ToUpperInvariant(question.CorrectAnswer);
        if (answer is not ('A' or 'B' or 'C' or 'D'))
        {
            throw new ArgumentException("Correct answer must be one of A, B, C, D.", nameof(question));
        }
    }
}
