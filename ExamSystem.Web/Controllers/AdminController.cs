using ExamSystem.Web.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ExamSystem.Web.Controllers;

[RequireRole("Admin")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
