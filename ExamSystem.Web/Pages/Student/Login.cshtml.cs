using System.ComponentModel.DataAnnotations;
using ExamSystem.Core.Interfaces;
using ExamSystem.Web.Configuration;
using ExamSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ExamSystem.Web.Pages.Student;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IStudentService _studentService;
    private readonly AdminAccountOptions _adminAccount;

    public LoginModel(IStudentService studentService, IOptions<AdminAccountOptions> adminAccountOptions)
    {
        _studentService = studentService;
        _adminAccount = adminAccountOptions.Value;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (HttpContext.Session.GetInt32(SessionKeys.UserId).HasValue)
        {
            var role = HttpContext.Session.GetString(SessionKeys.Role);
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(Url.Action("Index", "Admin") ?? "/Admin/Index");
            }

            return Redirect(Url.Page("/Student/Exam/Index") ?? "/Student/Exam");
        }

        Input.ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (TryAuthenticateAdmin(Input.Email, Input.Password))
        {
            SignIn(0, _adminAccount.FullName, "Admin");
            return RedirectToLocal(Input.ReturnUrl, Url.Action("Index", "Admin") ?? "/Admin/Index");
        }

        var student = await _studentService.AuthenticateAsync(Input.Email, Input.Password, cancellationToken);
        if (student is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        SignIn(student.Id, student.FullName, "Student");
        return RedirectToLocal(Input.ReturnUrl, Url.Page("/Student/Exam/Index") ?? "/Student/Exam");
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

    private IActionResult RedirectToLocal(string? returnUrl, string fallbackUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Redirect(fallbackUrl);
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

    public class LoginInput
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
