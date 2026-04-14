namespace ExamSystem.Core.Models;

public class ResultDetail
{
    public int Id { get; set; }
    public int ResultId { get; set; }
    public int QuestionId { get; set; }
    public char? ChosenAnswer { get; set; }
    public bool IsCorrect { get; set; }

    public ExamResult? Result { get; set; }
    public Question? Question { get; set; }
}
