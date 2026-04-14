namespace ExamSystem.Core.Models;

public class ExamResult
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ExamId { get; set; }
    public double Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int Duration { get; set; }

    public Student? Student { get; set; }
    public Exam? Exam { get; set; }
    public ICollection<ResultDetail> Details { get; set; } = new List<ResultDetail>();
}
