namespace ExamSystem.Core.Models;

public class Question
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public char CorrectAnswer { get; set; }

    public Exam? Exam { get; set; }
    public ICollection<ResultDetail> ResultDetails { get; set; } = new List<ResultDetail>();
}
