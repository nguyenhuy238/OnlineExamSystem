using System.ComponentModel.DataAnnotations;

namespace ExamSystem.Web.Models.Admin;

public class ExamUpsertViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Range(1, 300)]
    [Display(Name = "Duration (minutes)")]
    public int Duration { get; set; } = 30;

    [Range(0, 10)]
    [Display(Name = "Pass Score")]
    public double PassScore { get; set; } = 5.0;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
