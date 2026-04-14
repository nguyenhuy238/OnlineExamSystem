using System.ComponentModel.DataAnnotations;

namespace ExamSystem.Web.Models.Admin;

public class StudentUpsertViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Display(Name = "Student Code")]
    public string StudentCode { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }
}
