namespace ExamSystem.Core.Models;

public class Exam
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int Duration { get; set; }
    public double PassScore { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
