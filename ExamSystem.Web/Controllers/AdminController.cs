using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;
using ExamSystem.Web.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ExamSystem.Web.Controllers;

[RequireRole("Admin")]
public class AdminController : Controller
{
    private readonly IResultService _resultService;

    public AdminController(IResultService resultService)
    {
        _resultService = resultService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        DashboardStats stats;
        try
        {
            stats = await _resultService.GetDashboardStatsAsync(cancellationToken);
        }
        catch
        {
            stats = new DashboardStats();
            TempData["Error"] = "Unable to load dashboard data. Please verify database connectivity.";
        }

        return View(stats);
    }
}
