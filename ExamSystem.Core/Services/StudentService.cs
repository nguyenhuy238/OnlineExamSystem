using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;

namespace ExamSystem.Core.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Student?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByEmailAsync(email, cancellationToken);
        if (student is null)
        {
            return null;
        }

        if (!VerifyPassword(password, student.Password))
        {
            return null;
        }

        return student;
    }

    public async Task<IReadOnlyList<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default)
    {
        return await _studentRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Student?> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _studentRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Student> CreateStudentAsync(Student student, CancellationToken cancellationToken = default)
    {
        if (await _studentRepository.AnyAsync(x => x.Email == student.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        if (await _studentRepository.AnyAsync(x => x.StudentCode == student.StudentCode, cancellationToken))
        {
            throw new InvalidOperationException("Student code already exists.");
        }

        student.Password = HashIfRequired(student.Password);
        student.CreatedAt = DateTime.UtcNow;

        await _studentRepository.AddAsync(student, cancellationToken);
        return student;
    }

    public async Task<Student> UpdateStudentAsync(Student student, CancellationToken cancellationToken = default)
    {
        var existingStudent = await _studentRepository.GetByIdAsync(student.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Student with id {student.Id} was not found.");

        existingStudent.FullName = student.FullName;
        existingStudent.StudentCode = student.StudentCode;
        existingStudent.Email = student.Email;

        if (!string.IsNullOrWhiteSpace(student.Password))
        {
            existingStudent.Password = HashIfRequired(student.Password);
        }

        await _studentRepository.UpdateAsync(existingStudent, cancellationToken);
        return existingStudent;
    }

    public async Task DeleteStudentAsync(int id, CancellationToken cancellationToken = default)
    {
        await _studentRepository.DeleteAsync(id, cancellationToken);
    }

    private static string HashIfRequired(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        return IsBcryptHash(password) ? password : BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string plainTextPassword, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        if (!IsBcryptHash(storedHash))
        {
            return plainTextPassword == storedHash;
        }

        return BCrypt.Net.BCrypt.Verify(plainTextPassword, storedHash);
    }

    private static bool IsBcryptHash(string value)
    {
        return value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$");
    }
}
