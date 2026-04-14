using ExamSystem.Core.Models;

namespace ExamSystem.Core.Interfaces;

public interface IStudentService
{
    Task<Student?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default);
    Task<Student?> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Student> CreateStudentAsync(Student student, CancellationToken cancellationToken = default);
    Task<Student> UpdateStudentAsync(Student student, CancellationToken cancellationToken = default);
    Task DeleteStudentAsync(int id, CancellationToken cancellationToken = default);
}
