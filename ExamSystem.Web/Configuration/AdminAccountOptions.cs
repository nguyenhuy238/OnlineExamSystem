namespace ExamSystem.Web.Configuration;

public class AdminAccountOptions
{
    public const string SectionName = "Authentication:Admin";

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = "System Administrator";
}
