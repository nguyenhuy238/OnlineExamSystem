using ExamSystem.Core.Interfaces;
using ExamSystem.Web.Configuration;
using ExamSystem.Web.Infrastructure;
using ExamSystem.Web.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ExamSystem.Web.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly IStudentService _studentService;
    private readonly AdminAccountOptions _adminAccount;

    public AuthController(IStudentService studentService, IOptions<AdminAccountOptions> adminAccountOptions)
    {
        _studentService = studentService;
        _adminAccount = adminAccountOptions.Value;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.GetInt32(SessionKeys.UserId).HasValue)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (TryAuthenticateAdmin(model.Email, model.Password))
        {
            SignIn(0, _adminAccount.FullName, "Admin");
            return RedirectToLocal(model.ReturnUrl, "Admin", "Index");
        }

        var student = await _studentService.AuthenticateAsync(model.Email, model.Password);
        if (student is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        SignIn(student.Id, student.FullName, "Student");
        return RedirectToLocal(model.ReturnUrl, "Home", "Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private bool TryAuthenticateAdmin(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(_adminAccount.Email) || string.IsNullOrWhiteSpace(_adminAccount.PasswordHash))
        {
            return false;
        }

        if (!string.Equals(email, _adminAccount.Email, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return VerifyPassword(password, _adminAccount.PasswordHash);
    }

    private void SignIn(int userId, string fullName, string role)
    {
        HttpContext.Session.SetInt32(SessionKeys.UserId, userId);
        HttpContext.Session.SetString(SessionKeys.FullName, fullName);
        HttpContext.Session.SetString(SessionKeys.Role, role);
    }

    private IActionResult RedirectToLocal(string? returnUrl, string fallbackController, string fallbackAction)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(fallbackAction, fallbackController);
    }

    private static bool VerifyPassword(string plainTextPassword, string storedHash)
    {
        if (IsBcryptHash(storedHash))
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, storedHash);
        }

        return plainTextPassword == storedHash;
    }

    private static bool IsBcryptHash(string value)
    {
        return value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$");
    }
}
