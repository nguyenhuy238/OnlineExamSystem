using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExamSystem.Web.Models.Admin;

public class QuestionIndexViewModel
{
    public int? ExamId { get; set; }
    public IReadOnlyList<SelectListItem> ExamOptions { get; set; } = [];
    public IReadOnlyList<QuestionListItemViewModel> Questions { get; set; } = [];
}

public class QuestionListItemViewModel
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
}
