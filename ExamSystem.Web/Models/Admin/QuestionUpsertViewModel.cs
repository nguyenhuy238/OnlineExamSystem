using System.ComponentModel.DataAnnotations;

namespace ExamSystem.Web.Models.Admin;

public class QuestionUpsertViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam")]
    public int ExamId { get; set; }

    [Required]
    [StringLength(500)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Option A")]
    public string OptionA { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Option B")]
    public string OptionB { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Option C")]
    public string OptionC { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Option D")]
    public string OptionD { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[ABCDabcd]$", ErrorMessage = "Correct answer must be A, B, C, or D.")]
    [Display(Name = "Correct Answer")]
    public string CorrectAnswer { get; set; } = "A";
}
