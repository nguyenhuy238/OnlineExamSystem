using ExamSystem.Core.Interfaces;
using ExamSystem.Web.Attributes;
using ExamSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExamSystem.Web.Pages.Student.Exam;

[RequireRole("Student")]
public class TakeModel : PageModel
{
    private readonly IExamService _examService;
    private readonly IResultService _resultService;

    public TakeModel(IExamService examService, IResultService resultService)
    {
        _examService = examService;
        _resultService = resultService;
    }

    public ExamSystem.Core.Models.Exam? Exam { get; private set; }
    public string? ErrorMessage { get; private set; }

    [BindProperty]
    public Dictionary<int, string> Answers { get; set; } = [];

    [BindProperty]
    public int DurationSeconds { get; set; }

    public async Task<IActionResult> OnGetAsync(int examId, CancellationToken cancellationToken)
    {
        Exam = await _examService.GetExamWithQuestionsAsync(examId, cancellationToken);
        if (Exam is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int examId, CancellationToken cancellationToken)
    {
        var studentId = HttpContext.Session.GetInt32(SessionKeys.UserId);
        if (studentId is null)
        {
            return RedirectToPage("/Student/Login", new { returnUrl = Url.Page("/Student/Exam/Take", new { examId }) });
        }

        try
        {
            var mappedAnswers = Answers
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => new { x.Key, Answer = char.ToUpperInvariant(x.Value.Trim()[0]) })
                .Where(x => x.Answer is 'A' or 'B' or 'C' or 'D')
                .ToDictionary(x => x.Key, x => x.Answer);

            var result = await _resultService.SubmitExamAsync(
                studentId.Value,
                examId,
                mappedAnswers,
                DurationSeconds,
                cancellationToken);

            return RedirectToPage("/Student/Exam/Result", new { resultId = result.Id });
        }
        catch (Exception ex)
        {
            Exam = await _examService.GetExamWithQuestionsAsync(examId, cancellationToken);
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
