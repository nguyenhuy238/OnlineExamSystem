namespace ExamSystem.Core.Models;

public class TopStudentDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public double AvgScore { get; set; }
    public int TotalExams { get; set; }
}
