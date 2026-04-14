using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;
using ExamSystem.Web.Attributes;
using ExamSystem.Web.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExamSystem.Web.Controllers;

[RequireRole("Admin")]
public class QuestionController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly IExamService _examService;

    public QuestionController(IQuestionService questionService, IExamService examService)
    {
        _questionService = questionService;
        _examService = examService;
    }

    public async Task<IActionResult> Index(int? examId, CancellationToken cancellationToken)
    {
        try
        {
            var exams = await _examService.GetAllExamsAsync(cancellationToken);
            var questions = examId.HasValue
                ? await _questionService.GetQuestionsByExamAsync(examId.Value, cancellationToken)
                : await _questionService.GetAllQuestionsAsync(cancellationToken);

            var examTitleLookup = exams.ToDictionary(x => x.Id, x => x.Title);
            var model = new QuestionIndexViewModel
            {
                ExamId = examId,
                ExamOptions = exams
                    .OrderBy(x => x.Title)
                    .Select(x => new SelectListItem(x.Title, x.Id.ToString()))
                    .ToList(),
                Questions = questions.Select(q => new QuestionListItemViewModel
                {
                    Id = q.Id,
                    ExamId = q.ExamId,
                    ExamTitle = examTitleLookup.TryGetValue(q.ExamId, out var title) ? title : $"Exam #{q.ExamId}",
                    Content = q.Content,
                    CorrectAnswer = q.CorrectAnswer.ToString()
                }).ToList()
            };

            return View(model);
        }
        catch
        {
            TempData["Error"] = "Unable to load questions. Please verify database connectivity.";
            return View(new QuestionIndexViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? examId, CancellationToken cancellationToken)
    {
        var exams = await _examService.GetAllExamsAsync(cancellationToken);
        if (exams.Count == 0)
        {
            TempData["Error"] = "Please create at least one exam before adding questions.";
            return RedirectToAction("Index", "Exam");
        }

        await PopulateExamSelectListAsync(exams, examId);

        var defaultExamId = examId ?? exams.First().Id;
        return View(new QuestionUpsertViewModel { ExamId = defaultExamId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionUpsertViewModel model, CancellationToken cancellationToken)
    {
        await PopulateExamSelectListAsync(null, model.ExamId, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var question = ToEntity(model);
            await _questionService.CreateQuestionAsync(question, cancellationToken);
            TempData["Success"] = "Question created successfully.";
            return RedirectToAction(nameof(Index), new { examId = model.ExamId });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var question = await _questionService.GetQuestionByIdAsync(id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        await PopulateExamSelectListAsync(null, question.ExamId, cancellationToken);

        var model = new QuestionUpsertViewModel
        {
            Id = question.Id,
            ExamId = question.ExamId,
            Content = question.Content,
            OptionA = question.OptionA,
            OptionB = question.OptionB,
            OptionC = question.OptionC,
            OptionD = question.OptionD,
            CorrectAnswer = question.CorrectAnswer.ToString()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, QuestionUpsertViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        await PopulateExamSelectListAsync(null, model.ExamId, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var question = ToEntity(model);
            await _questionService.UpdateQuestionAsync(question, cancellationToken);
            TempData["Success"] = "Question updated successfully.";
            return RedirectToAction(nameof(Index), new { examId = model.ExamId });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? examId, CancellationToken cancellationToken)
    {
        try
        {
            await _questionService.DeleteQuestionAsync(id, cancellationToken);
            TempData["Success"] = "Question deleted successfully.";
        }
        catch
        {
            TempData["Error"] = "Unable to delete question.";
        }

        return RedirectToAction(nameof(Index), new { examId });
    }

    private async Task PopulateExamSelectListAsync(
        IReadOnlyList<Exam>? exams = null,
        int? selectedExamId = null,
        CancellationToken cancellationToken = default)
    {
        exams ??= await _examService.GetAllExamsAsync(cancellationToken);
        ViewBag.Exams = exams
            .OrderBy(x => x.Title)
            .Select(x => new SelectListItem(x.Title, x.Id.ToString(), x.Id == selectedExamId))
            .ToList();
    }

    private static Question ToEntity(QuestionUpsertViewModel model)
    {
        return new Question
        {
            Id = model.Id,
            ExamId = model.ExamId,
            Content = model.Content.Trim(),
            OptionA = model.OptionA.Trim(),
            OptionB = model.OptionB.Trim(),
            OptionC = model.OptionC.Trim(),
            OptionD = model.OptionD.Trim(),
            CorrectAnswer = char.ToUpperInvariant(model.CorrectAnswer.Trim()[0])
        };
    }
}
