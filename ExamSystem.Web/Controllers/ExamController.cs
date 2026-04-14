using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;
using ExamSystem.Web.Attributes;
using ExamSystem.Web.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace ExamSystem.Web.Controllers;

[RequireRole("Admin")]
public class ExamController : Controller
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var exams = await _examService.GetAllExamsAsync(cancellationToken);
            return View(exams.OrderByDescending(x => x.CreatedAt).ToList());
        }
        catch
        {
            TempData["Error"] = "Unable to load exams. Please verify database connectivity.";
            return View(Array.Empty<Exam>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ExamUpsertViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExamUpsertViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var exam = new Exam
            {
                Title = model.Title.Trim(),
                Subject = model.Subject.Trim(),
                Duration = model.Duration,
                PassScore = model.PassScore,
                IsActive = model.IsActive
            };

            await _examService.CreateExamAsync(exam, cancellationToken);
            TempData["Success"] = "Exam created successfully.";
            return RedirectToAction(nameof(Index));
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
        var exam = await _examService.GetExamWithQuestionsAsync(id, cancellationToken);
        if (exam is null)
        {
            return NotFound();
        }

        var model = new ExamUpsertViewModel
        {
            Id = exam.Id,
            Title = exam.Title,
            Subject = exam.Subject,
            Duration = exam.Duration,
            PassScore = exam.PassScore,
            IsActive = exam.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExamUpsertViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var exam = new Exam
            {
                Id = model.Id,
                Title = model.Title.Trim(),
                Subject = model.Subject.Trim(),
                Duration = model.Duration,
                PassScore = model.PassScore,
                IsActive = model.IsActive
            };

            await _examService.UpdateExamAsync(exam, cancellationToken);
            TempData["Success"] = "Exam updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _examService.DeleteExamAsync(id, cancellationToken);
            TempData["Success"] = "Exam deleted successfully.";
        }
        catch
        {
            TempData["Error"] = "Unable to delete exam.";
        }

        return RedirectToAction(nameof(Index));
    }
}
