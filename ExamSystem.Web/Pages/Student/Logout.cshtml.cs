using ExamSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExamSystem.Web.Pages.Student;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        HttpContext.Session.Remove(SessionKeys.UserId);
        HttpContext.Session.Remove(SessionKeys.Role);
        HttpContext.Session.Remove(SessionKeys.FullName);
        return RedirectToPage("/Student/Login");
    }
}
