using ExamSystem.Core.Interfaces;
using ExamSystem.Web.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExamSystem.Web.Pages.Student.Exam;

[RequireRole("Student")]
public class IndexModel : PageModel
{
    private readonly IExamService _examService;

    public IndexModel(IExamService examService)
    {
        _examService = examService;
    }

    public IReadOnlyList<ExamSystem.Core.Models.Exam> Exams { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Exams = await _examService.GetActiveExamsAsync(cancellationToken);
        }
        catch
        {
            ErrorMessage = "Unable to load exam list. Please try again later.";
            Exams = [];
        }
    }
}
