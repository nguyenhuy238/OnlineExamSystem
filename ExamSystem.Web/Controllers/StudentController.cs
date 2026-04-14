using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;
using ExamSystem.Web.Attributes;
using ExamSystem.Web.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace ExamSystem.Web.Controllers;

[RequireRole("Admin")]
public class StudentController : Controller
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var students = await _studentService.GetAllStudentsAsync(cancellationToken);
            return View(students.OrderBy(x => x.StudentCode).ToList());
        }
        catch
        {
            TempData["Error"] = "Unable to load students. Please verify database connectivity.";
            return View(Array.Empty<Student>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new StudentUpsertViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentUpsertViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var student = new Student
            {
                FullName = model.FullName.Trim(),
                StudentCode = model.StudentCode.Trim(),
                Email = model.Email.Trim(),
                Password = model.Password!
            };

            await _studentService.CreateStudentAsync(student, cancellationToken);
            TempData["Success"] = "Student created successfully.";
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
        var student = await _studentService.GetStudentByIdAsync(id, cancellationToken);
        if (student is null)
        {
            return NotFound();
        }

        var model = new StudentUpsertViewModel
        {
            Id = student.Id,
            FullName = student.FullName,
            StudentCode = student.StudentCode,
            Email = student.Email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentUpsertViewModel model, CancellationToken cancellationToken)
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
            var student = new Student
            {
                Id = model.Id,
                FullName = model.FullName.Trim(),
                StudentCode = model.StudentCode.Trim(),
                Email = model.Email.Trim(),
                Password = string.IsNullOrWhiteSpace(model.Password) ? string.Empty : model.Password
            };

            await _studentService.UpdateStudentAsync(student, cancellationToken);
            TempData["Success"] = "Student updated successfully.";
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
            await _studentService.DeleteStudentAsync(id, cancellationToken);
            TempData["Success"] = "Student deleted successfully.";
        }
        catch
        {
            TempData["Error"] = "Unable to delete student.";
        }

        return RedirectToAction(nameof(Index));
    }
}
