namespace ExamSystem.Core.Models;

public class DashboardStats
{
    public int TotalStudents { get; set; }
    public int TotalExams { get; set; }
    public int TotalAttempts { get; set; }
    public double PassRate { get; set; }
    public double AverageScore { get; set; }
    public IReadOnlyList<TopStudentDto> TopStudents { get; set; } = [];
}
