using ExamSystem.Core.Models;

namespace ExamSystem.Data.Configurations;

internal static class SeedData
{
    // BCrypt hash for plaintext: 123456
    public const string DefaultStudentPasswordHash = "$2a$11$SXXhIP8IvHp3rGX9DDNd.e5uLt5Glsx/v2DeRVI1yg9mdU2yj.7CC";

    public static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Student[] Students =
    [
        new Student
        {
            Id = 1,
            FullName = "Nguyen Van An",
            StudentCode = "SV001",
            Email = "an@student.edu",
            Password = DefaultStudentPasswordHash,
            CreatedAt = SeedDate
        },
        new Student
        {
            Id = 2,
            FullName = "Tran Thi Binh",
            StudentCode = "SV002",
            Email = "binh@student.edu",
            Password = DefaultStudentPasswordHash,
            CreatedAt = SeedDate
        },
        new Student
        {
            Id = 3,
            FullName = "Le Quoc Cuong",
            StudentCode = "SV003",
            Email = "cuong@student.edu",
            Password = DefaultStudentPasswordHash,
            CreatedAt = SeedDate
        },
        new Student
        {
            Id = 4,
            FullName = "Pham Gia Dung",
            StudentCode = "SV004",
            Email = "dung@student.edu",
            Password = DefaultStudentPasswordHash,
            CreatedAt = SeedDate
        },
        new Student
        {
            Id = 5,
            FullName = "Vo Ngoc Em",
            StudentCode = "SV005",
            Email = "em@student.edu",
            Password = DefaultStudentPasswordHash,
            CreatedAt = SeedDate
        }
    ];

    public static readonly Exam[] Exams =
    [
        new Exam
        {
            Id = 1,
            Title = "Bai kiem tra OOP",
            Subject = "Lap trinh OOP",
            Duration = 30,
            PassScore = 5.0,
            IsActive = true,
            CreatedAt = SeedDate
        },
        new Exam
        {
            Id = 2,
            Title = "Bai kiem tra C# Co Ban",
            Subject = "C#",
            Duration = 20,
            PassScore = 5.0,
            IsActive = true,
            CreatedAt = SeedDate
        }
    ];

    public static readonly Question[] Questions =
    [
        new Question
        {
            Id = 1,
            ExamId = 1,
            Content = "Tinh chat nao KHONG thuoc OOP?",
            OptionA = "Ke thua",
            OptionB = "Da hinh",
            OptionC = "De quy",
            OptionD = "Dong goi",
            CorrectAnswer = 'C'
        },
        new Question
        {
            Id = 2,
            ExamId = 1,
            Content = "Interface trong C# co the chua?",
            OptionA = "Field",
            OptionB = "Constructor",
            OptionC = "Method signature",
            OptionD = "Static constructor",
            CorrectAnswer = 'C'
        },
        new Question
        {
            Id = 3,
            ExamId = 2,
            Content = "Tu khoa tao doi tuong trong C# la?",
            OptionA = "class",
            OptionB = "new",
            OptionC = "this",
            OptionD = "base",
            CorrectAnswer = 'B'
        },
        new Question
        {
            Id = 4,
            ExamId = 2,
            Content = "Kieu du lieu nao la so nguyen 32-bit?",
            OptionA = "long",
            OptionB = "double",
            OptionC = "int",
            OptionD = "decimal",
            CorrectAnswer = 'C'
        }
    ];
}
