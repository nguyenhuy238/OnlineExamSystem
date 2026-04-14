using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;
using ExamSystem.Web.Attributes;
using ExamSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExamSystem.Web.Pages.Student.Exam;

[RequireRole("Student")]
public class ResultModel : PageModel
{
    private readonly IResultService _resultService;

    public ResultModel(IResultService resultService)
    {
        _resultService = resultService;
    }

    public ExamResult? Result { get; private set; }

    public async Task<IActionResult> OnGetAsync(int resultId, CancellationToken cancellationToken)
    {
        var studentId = HttpContext.Session.GetInt32(SessionKeys.UserId);
        if (studentId is null)
        {
            return RedirectToPage("/Student/Login", new { returnUrl = Url.Page("/Student/Exam/Result", new { resultId }) });
        }

        var result = await _resultService.GetResultWithDetailsAsync(resultId, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        if (result.StudentId != studentId.Value)
        {
            return RedirectToPage("/Student/AccessDenied");
        }

        Result = result;
        return Page();
    }
}
