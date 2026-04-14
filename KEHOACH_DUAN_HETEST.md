# KẾ HOẠCH PHÁT TRIỂN HỆ THỐNG THI TRẮC NGHIỆM TRỰC TUYẾN
> **Dành cho AI Developer** — Tài liệu handoff đầy đủ để phát triển hoàn thiện
> **Thời gian:** 2 tuần | **Stack:** ASP.NET Core 8, EF Core, SignalR, SQL Server

---

## 1. TỔNG QUAN DỰ ÁN

### Mô tả
Hệ thống thi trắc nghiệm trực tuyến cho sinh viên. Admin quản lý đề thi, câu hỏi, sinh viên. Sinh viên đăng nhập, chọn bài thi, làm bài trong thời gian giới hạn, nộp bài và xem kết quả. Hệ thống có đồng hồ đếm ngược realtime và thông báo realtime qua SignalR.

### Yêu cầu môn học cần đáp ứng

| Tiêu chí | Tính năng |
|----------|-----------|
| EF Core + Scaffold | DB context, migrations, scaffold CRUD |
| CRUD đầy đủ | Sinh viên, Bài test, Câu hỏi, Kết quả |
| Dashboard thống kê | Điểm TB, tỉ lệ đạt/trượt, top sinh viên |
| MVC | Admin panel (quản lý hệ thống) |
| Razor Pages | Giao diện làm bài thi sinh viên |
| Routing + Model Binding | /exam/{id}, form trả lời |
| Thread / Task / Async-Await | Chấm điểm async, load đề async |
| Console Client | Gửi kết quả lên server qua socket |
| Console Server | Nhận và lưu kết quả từ client |
| JSON Serialize | Export kết quả thi ra JSON |
| Filter (tái sử dụng) | AuthFilter kiểm tra đăng nhập |
| Export JSON | ExportService xuất dữ liệu |
| Dependency Injection | IExamService, IResultService, IStudentService |
| Design Pattern | Repository Pattern |
| SignalR | Đồng hồ đếm ngược + thông báo nộp bài |

---

## 2. SOLUTION STRUCTURE

```
ExamSystem/
├── ExamSystem.Web/                  # ASP.NET Core 8 Web App (MVC + Razor Pages)
│   ├── Controllers/                 # MVC Controllers (Admin)
│   │   ├── AdminController.cs
│   │   ├── ExamController.cs
│   │   └── StudentController.cs
│   ├── Pages/                       # Razor Pages (Sinh viên làm bài)
│   │   ├── Exam/
│   │   │   ├── Index.cshtml         # Danh sách bài thi
│   │   │   ├── Take.cshtml          # Giao diện làm bài
│   │   │   └── Result.cshtml        # Xem kết quả
│   │   └── Auth/
│   │       ├── Login.cshtml
│   │       └── Logout.cshtml
│   ├── Hubs/
│   │   └── ExamHub.cs               # SignalR Hub
│   ├── Filters/
│   │   └── AuthFilter.cs            # Action Filter tái sử dụng
│   ├── wwwroot/                     # Static files
│   └── Program.cs
│
├── ExamSystem.Core/                 # Class Library - Business Logic
│   ├── Interfaces/
│   │   ├── IExamService.cs
│   │   ├── IStudentService.cs
│   │   ├── IResultService.cs
│   │   └── IRepository.cs           # Generic Repository interface
│   ├── Services/
│   │   ├── ExamService.cs
│   │   ├── StudentService.cs
│   │   ├── ResultService.cs         # Chấm điểm async/await
│   │   └── ExportService.cs         # Export JSON
│   └── Models/
│       ├── Student.cs
│       ├── Exam.cs
│       ├── Question.cs
│       ├── ExamResult.cs
│       └── ResultDetail.cs
│
├── ExamSystem.Data/                 # Class Library - Data Access
│   ├── AppDbContext.cs              # EF Core DbContext
│   ├── Repositories/
│   │   ├── GenericRepository.cs     # Repository Pattern
│   │   ├── ExamRepository.cs
│   │   └── ResultRepository.cs
│   └── Migrations/                  # EF Core Migrations
│
├── ExamSystem.Console.Server/       # Console App - TCP Server
│   └── Program.cs                   # Lắng nghe kết nối, nhận JSON
│
└── ExamSystem.Console.Client/       # Console App - TCP Client
    └── Program.cs                   # Gửi kết quả thi lên server
```

---

## 3. DATABASE SCHEMA

### 3.1 Danh sách bảng

```sql
-- Bảng sinh viên
CREATE TABLE Students (
    Id          INT PRIMARY KEY IDENTITY,
    FullName    NVARCHAR(100) NOT NULL,
    StudentCode VARCHAR(20)  NOT NULL UNIQUE,
    Email       VARCHAR(100) NOT NULL,
    Password    VARCHAR(256) NOT NULL,      -- BCrypt hash
    CreatedAt   DATETIME DEFAULT GETDATE()
);

-- Bảng bài thi
CREATE TABLE Exams (
    Id          INT PRIMARY KEY IDENTITY,
    Title       NVARCHAR(200) NOT NULL,
    Subject     NVARCHAR(100) NOT NULL,
    Duration    INT NOT NULL,               -- Phút
    PassScore   FLOAT DEFAULT 5.0,
    IsActive    BIT DEFAULT 1,
    CreatedAt   DATETIME DEFAULT GETDATE()
);

-- Bảng câu hỏi
CREATE TABLE Questions (
    Id          INT PRIMARY KEY IDENTITY,
    ExamId      INT NOT NULL FOREIGN KEY REFERENCES Exams(Id),
    Content     NVARCHAR(500) NOT NULL,
    OptionA     NVARCHAR(200) NOT NULL,
    OptionB     NVARCHAR(200) NOT NULL,
    OptionC     NVARCHAR(200) NOT NULL,
    OptionD     NVARCHAR(200) NOT NULL,
    CorrectAnswer CHAR(1) NOT NULL          -- A/B/C/D
);

-- Bảng kết quả thi
CREATE TABLE ExamResults (
    Id          INT PRIMARY KEY IDENTITY,
    StudentId   INT NOT NULL FOREIGN KEY REFERENCES Students(Id),
    ExamId      INT NOT NULL FOREIGN KEY REFERENCES Exams(Id),
    Score       FLOAT NOT NULL,
    IsPassed    BIT NOT NULL,
    SubmittedAt DATETIME DEFAULT GETDATE(),
    Duration    INT                         -- Thời gian làm (giây)
);

-- Bảng chi tiết kết quả
CREATE TABLE ResultDetails (
    Id          INT PRIMARY KEY IDENTITY,
    ResultId    INT NOT NULL FOREIGN KEY REFERENCES ExamResults(Id),
    QuestionId  INT NOT NULL FOREIGN KEY REFERENCES Questions(Id),
    ChosenAnswer CHAR(1),                   -- A/B/C/D hoặc NULL nếu bỏ qua
    IsCorrect   BIT NOT NULL
);
```

### 3.2 EF Core DbContext

```csharp
// ExamSystem.Data/AppDbContext.cs
public class AppDbContext : DbContext
{
    public DbSet<Student>     Students     { get; set; }
    public DbSet<Exam>        Exams        { get; set; }
    public DbSet<Question>    Questions    { get; set; }
    public DbSet<ExamResult>  ExamResults  { get; set; }
    public DbSet<ResultDetail> ResultDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Exam>().HasMany(e => e.Questions)
          .WithOne(q => q.Exam).HasForeignKey(q => q.ExamId);
        mb.Entity<ExamResult>().HasMany(r => r.Details)
          .WithOne(d => d.Result).HasForeignKey(d => d.ResultId);
    }
}
```

---

## 4. MODELS (C# Classes)

```csharp
// Student.cs
public class Student {
    public int Id { get; set; }
    public string FullName { get; set; }
    public string StudentCode { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Exam.cs
public class Exam {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Subject { get; set; }
    public int Duration { get; set; }       // minutes
    public double PassScore { get; set; }
    public bool IsActive { get; set; }
    public List<Question> Questions { get; set; }
}

// Question.cs
public class Question {
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string Content { get; set; }
    public string OptionA { get; set; }
    public string OptionB { get; set; }
    public string OptionC { get; set; }
    public string OptionD { get; set; }
    public char CorrectAnswer { get; set; }
    public Exam Exam { get; set; }
}

// ExamResult.cs
public class ExamResult {
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ExamId { get; set; }
    public double Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int Duration { get; set; }
    public Student Student { get; set; }
    public Exam Exam { get; set; }
    public List<ResultDetail> Details { get; set; }
}

// ResultDetail.cs
public class ResultDetail {
    public int Id { get; set; }
    public int ResultId { get; set; }
    public int QuestionId { get; set; }
    public char? ChosenAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public ExamResult Result { get; set; }
    public Question Question { get; set; }
}
```

---

## 5. INTERFACES & SERVICES

### 5.1 Repository Pattern

```csharp
// IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T>           GetByIdAsync(int id);
    Task<List<T>>     GetAllAsync();
    Task              AddAsync(T entity);
    Task              UpdateAsync(T entity);
    Task              DeleteAsync(int id);
}

// GenericRepository.cs
public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _ctx;
    public GenericRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<T>       GetByIdAsync(int id) => await _ctx.Set<T>().FindAsync(id);
    public async Task<List<T>> GetAllAsync()         => await _ctx.Set<T>().ToListAsync();
    public async Task          AddAsync(T entity)    { await _ctx.Set<T>().AddAsync(entity); await _ctx.SaveChangesAsync(); }
    public async Task          UpdateAsync(T entity) { _ctx.Set<T>().Update(entity); await _ctx.SaveChangesAsync(); }
    public async Task          DeleteAsync(int id)   { var e = await GetByIdAsync(id); _ctx.Set<T>().Remove(e); await _ctx.SaveChangesAsync(); }
}
```

### 5.2 Service Interfaces

```csharp
// IExamService.cs
public interface IExamService
{
    Task<List<Exam>>  GetActiveExamsAsync();
    Task<Exam>        GetExamWithQuestionsAsync(int examId);
    Task              CreateExamAsync(Exam exam);
    Task              UpdateExamAsync(Exam exam);
    Task              DeleteExamAsync(int examId);
}

// IStudentService.cs
public interface IStudentService
{
    Task<Student>       AuthenticateAsync(string email, string password);
    Task<List<Student>> GetAllStudentsAsync();
    Task               CreateStudentAsync(Student student);
    Task               UpdateStudentAsync(Student student);
    Task               DeleteStudentAsync(int id);
}

// IResultService.cs
public interface IResultService
{
    Task<ExamResult>       SubmitExamAsync(int studentId, int examId, Dictionary<int, char> answers);
    Task<List<ExamResult>> GetResultsByStudentAsync(int studentId);
    Task<List<ExamResult>> GetResultsByExamAsync(int examId);
    Task<DashboardStats>   GetDashboardStatsAsync();
    Task                   ExportResultsToJsonAsync(string filePath);
}
```

### 5.3 ResultService — Async/Await + Task

```csharp
// ResultService.cs — Đây là nơi demo Async/Await và Task
public class ResultService : IResultService
{
    private readonly IRepository<ExamResult> _resultRepo;
    private readonly IRepository<ResultDetail> _detailRepo;
    private readonly AppDbContext _ctx;

    // Chấm điểm ASYNC — demo Task/Async-Await
    public async Task<ExamResult> SubmitExamAsync(
        int studentId, int examId, Dictionary<int, char> answers)
    {
        // Load câu hỏi song song (Task.WhenAll)
        var examTask     = _ctx.Exams.FindAsync(examId).AsTask();
        var questionsTask = _ctx.Questions
                               .Where(q => q.ExamId == examId)
                               .ToListAsync();

        await Task.WhenAll(examTask, questionsTask);

        var exam      = examTask.Result;
        var questions = questionsTask.Result;

        // Chấm điểm trên Thread Pool
        var details = await Task.Run(() => questions.Select(q => new ResultDetail
        {
            QuestionId   = q.Id,
            ChosenAnswer = answers.TryGetValue(q.Id, out var ans) ? ans : (char?)null,
            IsCorrect    = answers.TryGetValue(q.Id, out var a) && a == q.CorrectAnswer
        }).ToList());

        int  correct = details.Count(d => d.IsCorrect);
        double score = Math.Round((double)correct / questions.Count * 10, 2);

        var result = new ExamResult
        {
            StudentId   = studentId,
            ExamId      = examId,
            Score       = score,
            IsPassed    = score >= exam.PassScore,
            SubmittedAt = DateTime.Now,
            Details     = details
        };

        await _resultRepo.AddAsync(result);
        return result;
    }

    // Export JSON — demo JSON Serialize
    public async Task ExportResultsToJsonAsync(string filePath)
    {
        var results = await _ctx.ExamResults
            .Include(r => r.Student)
            .Include(r => r.Exam)
            .ToListAsync();

        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions
        {
            WriteIndented        = true,
            ReferenceHandler     = ReferenceHandler.IgnoreCycles
        });

        await File.WriteAllTextAsync(filePath, json);
    }

    // Dashboard thống kê
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        return new DashboardStats
        {
            TotalStudents = await _ctx.Students.CountAsync(),
            TotalExams    = await _ctx.Exams.CountAsync(),
            TotalAttempts = await _ctx.ExamResults.CountAsync(),
            PassRate      = await _ctx.ExamResults.AverageAsync(r => r.IsPassed ? 1.0 : 0.0) * 100,
            AverageScore  = await _ctx.ExamResults.AverageAsync(r => r.Score),
            TopStudents   = await _ctx.ExamResults
                .GroupBy(r => r.StudentId)
                .Select(g => new TopStudentDto {
                    StudentId  = g.Key,
                    FullName   = g.First().Student.FullName,
                    AvgScore   = g.Average(r => r.Score),
                    TotalExams = g.Count()
                })
                .OrderByDescending(s => s.AvgScore)
                .Take(5)
                .ToListAsync()
        };
    }
}
```

---

## 6. SIGNALR HUB

```csharp
// ExamHub.cs — Demo SignalR Realtime
[Authorize]
public class ExamHub : Hub
{
    // Sinh viên tham gia phòng thi
    public async Task JoinExam(int examId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"exam-{examId}");
        await Clients.Group($"exam-{examId}")
              .SendAsync("StudentJoined", Context.User.Identity.Name);
    }

    // Client gửi tick mỗi giây — server broadcast lại
    public async Task TimerTick(int examId, int secondsLeft)
    {
        await Clients.Group($"exam-{examId}")
              .SendAsync("TimerUpdate", secondsLeft);

        // Cảnh báo khi còn 5 phút
        if (secondsLeft == 300)
            await Clients.Group($"exam-{examId}")
                  .SendAsync("Warning", "Còn 5 phút!");
    }

    // Nộp bài — thông báo cho admin
    public async Task NotifySubmit(int examId, string studentName)
    {
        await Clients.Group("admin")
              .SendAsync("StudentSubmitted", studentName, DateTime.Now);
    }

    // Admin tham gia để nhận thông báo
    public async Task JoinAdmin()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
    }
}
```

### JavaScript phía client (Take.cshtml)

```javascript
// Kết nối SignalR và đồng hồ đếm ngược
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/examHub")
    .build();

let secondsLeft = examDuration * 60; // examDuration từ Razor Model

connection.on("TimerUpdate", (seconds) => {
    updateTimerDisplay(seconds);
    if (seconds <= 0) autoSubmit();
});

connection.on("Warning", (msg) => {
    showWarning(msg); // Hiển thị toast cảnh báo
});

connection.start().then(() => {
    connection.invoke("JoinExam", examId);
    // Tick mỗi giây
    setInterval(async () => {
        secondsLeft--;
        await connection.invoke("TimerTick", examId, secondsLeft);
    }, 1000);
});

function updateTimerDisplay(s) {
    const m = Math.floor(s / 60).toString().padStart(2, '0');
    const sec = (s % 60).toString().padStart(2, '0');
    document.getElementById('timer').textContent = `${m}:${sec}`;
    if (s <= 300) document.getElementById('timer').classList.add('text-danger');
}
```

---

## 7. AUTH FILTER (Tái sử dụng)

```csharp
// AuthFilter.cs — IActionFilter dùng lại cho mọi Controller/Action cần auth
public class AuthFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId  = session.GetInt32("UserId");
        var role    = session.GetString("Role");

        if (userId == null)
        {
            context.Result = new RedirectToPageResult("/Auth/Login");
            return;
        }

        // Kiểm tra role nếu controller có [RequireRole]
        var requiredRole = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireRoleAttribute>().FirstOrDefault();

        if (requiredRole != null && role != requiredRole.Role)
            context.Result = new ForbidResult();
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

// Cách dùng trong Controller:
[ServiceFilter(typeof(AuthFilter))]
[RequireRole("Admin")]
public class AdminController : Controller { ... }

// Cách dùng trong Razor Pages:
[ServiceFilter(typeof(AuthFilter))]
public class TakeModel : PageModel { ... }
```

---

## 8. DEPENDENCY INJECTION SETUP

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IExamRepository, ExamRepository>();

// Services (DI)
builder.Services.AddScoped<IExamService,    ExamService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IResultService,  ResultService>();
builder.Services.AddScoped<ExportService>();

// Filters
builder.Services.AddScoped<AuthFilter>();

// SignalR
builder.Services.AddSignalR();

// Session
builder.Services.AddSession(opt => {
    opt.IdleTimeout     = TimeSpan.FromMinutes(30);
    opt.Cookie.HttpOnly = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();
app.UseSession();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<ExamHub>("/examHub");
app.Run();
```

---

## 9. CONSOLE SERVER & CLIENT

### 9.1 Console Server

```csharp
// ExamSystem.Console.Server/Program.cs
// TCP Server — Lắng nghe kết quả thi từ Console Client

class Program
{
    static async Task Main(string[] args)
    {
        var listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("[SERVER] Đang lắng nghe cổng 5000...");

        while (true)
        {
            var client  = await listener.AcceptTcpClientAsync();
            // Mỗi client chạy trên Task riêng (demo Task)
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string json = await reader.ReadToEndAsync();
        Console.WriteLine($"[SERVER] Nhận dữ liệu:\n{json}");

        // Deserialize và lưu vào DB hoặc file
        var result = JsonSerializer.Deserialize<ExamResultDto>(json);
        string fileName = $"result_{result.StudentCode}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, json);

        Console.WriteLine($"[SERVER] Đã lưu: {fileName}");
        client.Close();
    }
}
```

### 9.2 Console Client

```csharp
// ExamSystem.Console.Client/Program.cs
// TCP Client — Gửi kết quả thi lên Server

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Nhập MSSV: ");
        string code = Console.ReadLine();

        // Tạo dữ liệu mẫu (demo JSON Serialize)
        var result = new ExamResultDto
        {
            StudentCode = code,
            ExamTitle   = "Lập trình OOP",
            Score       = 8.5,
            IsPassed    = true,
            SubmittedAt = DateTime.Now,
            Answers     = new() { { 1, 'A' }, { 2, 'C' }, { 3, 'B' } }
        };

        // Serialize JSON
        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.WriteLine($"[CLIENT] Gửi dữ liệu:\n{json}");

        // Kết nối TCP
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", 5000);

        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.AutoFlush = true;

        await writer.WriteAsync(json);
        Console.WriteLine("[CLIENT] Đã gửi thành công!");
    }
}
```

---

## 10. MVC CONTROLLERS (Admin)

```csharp
// AdminController.cs — Dashboard
[ServiceFilter(typeof(AuthFilter))]
[RequireRole("Admin")]
public class AdminController : Controller
{
    private readonly IResultService _resultService;
    public AdminController(IResultService resultService)
        => _resultService = resultService;

    public async Task<IActionResult> Index()
    {
        var stats = await _resultService.GetDashboardStatsAsync();
        return View(stats);
    }

    public async Task<IActionResult> ExportJson()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "exports",
                                   $"results_{DateTime.Now:yyyyMMdd}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await _resultService.ExportResultsToJsonAsync(path);
        return PhysicalFile(path, "application/json",
                            $"results_{DateTime.Now:yyyyMMdd}.json");
    }
}

// ExamController.cs — CRUD Bài thi
[ServiceFilter(typeof(AuthFilter))]
[RequireRole("Admin")]
public class ExamController : Controller
{
    private readonly IExamService _examService;
    public ExamController(IExamService examService) => _examService = examService;

    public async Task<IActionResult> Index()
        => View(await _examService.GetActiveExamsAsync());

    [HttpGet] public IActionResult Create() => View();
    [HttpPost] public async Task<IActionResult> Create(Exam exam)
    {
        if (!ModelState.IsValid) return View(exam);
        await _examService.CreateExamAsync(exam);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet] public async Task<IActionResult> Edit(int id)
        => View(await _examService.GetExamWithQuestionsAsync(id));

    [HttpPost] public async Task<IActionResult> Edit(Exam exam)
    {
        await _examService.UpdateExamAsync(exam);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost] public async Task<IActionResult> Delete(int id)
    {
        await _examService.DeleteExamAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
```

---

## 11. RAZOR PAGES (Sinh viên)

```csharp
// Pages/Exam/Take.cshtml.cs
[ServiceFilter(typeof(AuthFilter))]
public class TakeModel : PageModel
{
    private readonly IExamService   _examService;
    private readonly IResultService _resultService;

    public Exam    Exam      { get; set; }
    public int     StudentId { get; set; }

    [BindProperty]
    public Dictionary<int, char> Answers { get; set; } = new();

    public TakeModel(IExamService e, IResultService r)
    { _examService = e; _resultService = r; }

    // GET /Exam/Take?examId=1
    public async Task<IActionResult> OnGetAsync(int examId)
    {
        StudentId = HttpContext.Session.GetInt32("UserId") ?? 0;
        Exam      = await _examService.GetExamWithQuestionsAsync(examId);
        if (Exam == null) return NotFound();
        return Page();
    }

    // POST — Nộp bài
    public async Task<IActionResult> OnPostAsync(int examId)
    {
        int studentId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var result    = await _resultService.SubmitExamAsync(studentId, examId, Answers);
        return RedirectToPage("Result", new { resultId = result.Id });
    }
}
```

---

## 12. DEMO CONCURRENCY (Thread / Task / Async)

```csharp
// ConcurrencyDemoService.cs — File riêng để demo 3 phiên bản
public class ConcurrencyDemoService
{
    // ──────────────────────────────────────────────
    // PHIÊN BẢN 1: Thread (cũ, low-level)
    // ──────────────────────────────────────────────
    public void GradeWithThread(List<Question> questions, Dictionary<int, char> answers)
    {
        int correct = 0;
        var threads = new List<Thread>();

        foreach (var q in questions)
        {
            var thread = new Thread(() =>
            {
                if (answers.TryGetValue(q.Id, out var ans) && ans == q.CorrectAnswer)
                    Interlocked.Increment(ref correct);
            });
            threads.Add(thread);
            thread.Start();
        }

        threads.ForEach(t => t.Join());
        Console.WriteLine($"[Thread] Đúng: {correct}/{questions.Count}");
    }

    // ──────────────────────────────────────────────
    // PHIÊN BẢN 2: Task (hiện đại hơn Thread)
    // ──────────────────────────────────────────────
    public void GradeWithTask(List<Question> questions, Dictionary<int, char> answers)
    {
        int correct = 0;
        var tasks = questions.Select(q => Task.Run(() =>
        {
            if (answers.TryGetValue(q.Id, out var ans) && ans == q.CorrectAnswer)
                Interlocked.Increment(ref correct);
        })).ToArray();

        Task.WaitAll(tasks);
        Console.WriteLine($"[Task] Đúng: {correct}/{questions.Count}");
    }

    // ──────────────────────────────────────────────
    // PHIÊN BẢN 3: Async/Await (Best Practice)
    // ──────────────────────────────────────────────
    public async Task<int> GradeAsync(List<Question> questions, Dictionary<int, char> answers)
    {
        var tasks = questions.Select(async q =>
        {
            await Task.Yield(); // Nhả thread, không block
            return answers.TryGetValue(q.Id, out var ans) && ans == q.CorrectAnswer;
        });

        var results = await Task.WhenAll(tasks);
        int correct = results.Count(r => r);
        Console.WriteLine($"[Async] Đúng: {correct}/{questions.Count}");
        return correct;
    }
}
```

---

## 13. KẾ HOẠCH 2 TUẦN CHI TIẾT

### TUẦN 1 — Backend & Core

| Ngày | Mục tiêu | Việc cần làm | Checklist |
|------|----------|-------------|-----------|
| **1** | Setup project | Tạo solution, tất cả projects, cài NuGet packages | ☐ Solution structure ☐ NuGet |
| **2** | EF Core | Tạo Models, AppDbContext, Migration, Seed data | ☐ DB tạo được ☐ Seed 5 SV, 2 đề |
| **3** | Repository | GenericRepository, ExamRepository, StudentRepository | ☐ CRUD hoạt động |
| **4** | Services | ExamService, StudentService (CRUD + Auth) | ☐ Login/Register |
| **5** | Concurrency | ResultService với 3 phiên bản chấm điểm | ☐ Thread ☐ Task ☐ Async |
| **6** | Console App | TCP Server + TCP Client + JSON Serialize | ☐ Gửi/nhận được |
| **7** | Export | ExportService JSON, test toàn bộ backend | ☐ Export file JSON |

### TUẦN 2 — Web & Advanced

| Ngày | Mục tiêu | Việc cần làm | Checklist |
|------|----------|-------------|-----------|
| **8** | MVC Admin | AdminController (Dashboard), ExamController (CRUD) | ☐ Dashboard hiển thị |
| **9** | MVC Admin | StudentController, QuestionController (CRUD) | ☐ CRUD hoàn chỉnh |
| **10** | Razor Pages | Login, Danh sách đề thi, Giao diện làm bài | ☐ Load đề thi |
| **11** | SignalR | ExamHub, đồng hồ đếm ngược, thông báo nộp bài | ☐ Timer realtime |
| **12** | Filter + DI | AuthFilter, DI setup hoàn chỉnh, routing | ☐ Auth hoạt động |
| **13** | Test & Fix | Test toàn bộ luồng, fix bug | ☐ Tất cả feature pass |
| **14** | Hoàn thiện | UI polish, chuẩn bị slide thuyết trình | ☐ Demo được |

---

## 14. LUỒNG DEMO THUYẾT TRÌNH

```
1. [ADMIN] Login → Dashboard thống kê (điểm TB, tỉ lệ đạt)
2. [ADMIN] Tạo đề thi mới → Thêm câu hỏi (CRUD)
3. [ADMIN] Quản lý sinh viên (CRUD)
4. [SINH VIÊN] Login → Xem danh sách đề thi
5. [SINH VIÊN] Chọn đề → Làm bài (đồng hồ đếm ngược realtime ← SignalR)
6. [ADMIN] Nhìn thấy sinh viên nộp bài realtime ← SignalR
7. [SINH VIÊN] Xem kết quả chi tiết
8. [ADMIN] Export JSON → Download file
9. [CONSOLE] Chạy Server → Chạy Client → Gửi kết quả qua TCP
10. [CODE] Giải thích Thread/Task/Async, Repository, DI, Filter
```

---

## 15. NUGET PACKAGES CẦN CÀI

```xml
<!-- ExamSystem.Web -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.*" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.*" />

<!-- ExamSystem.Data -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />

<!-- ExamSystem.Console.Server & Client -->
<PackageReference Include="System.Text.Json" Version="8.0.*" />
```

---

## 16. SEED DATA MẪU

```csharp
// AppDbContext.cs — OnModelCreating
modelBuilder.Entity<Student>().HasData(
    new Student { Id=1, FullName="Nguyễn Văn An", StudentCode="SV001",
                  Email="an@student.edu", Password=BCrypt.HashPassword("123456") },
    new Student { Id=2, FullName="Trần Thị Bình", StudentCode="SV002",
                  Email="binh@student.edu", Password=BCrypt.HashPassword("123456") }
);

modelBuilder.Entity<Exam>().HasData(
    new Exam { Id=1, Title="Bài kiểm tra OOP", Subject="Lập trình OOP",
               Duration=30, PassScore=5.0, IsActive=true }
);

modelBuilder.Entity<Question>().HasData(
    new Question { Id=1, ExamId=1, Content="Tính chất nào KHÔNG thuộc OOP?",
                   OptionA="Kế thừa", OptionB="Đa hình",
                   OptionC="Đệ quy", OptionD="Đóng gói", CorrectAnswer='C' },
    new Question { Id=2, ExamId=1, Content="Interface trong C# có thể chứa?",
                   OptionA="Field", OptionB="Constructor",
                   OptionC="Method signature", OptionD="Static constructor", CorrectAnswer='C' }
);
```

---

## 17. CHECKLIST CUỐI CÙNG

Trước khi thuyết trình, kiểm tra đủ tất cả mục sau:

- [ ] EF Core Scaffold chạy được, migration thành công
- [ ] CRUD đầy đủ: Sinh viên, Bài thi, Câu hỏi, Kết quả
- [ ] Dashboard thống kê hiển thị đúng số liệu
- [ ] MVC Controllers có đủ Admin panel
- [ ] Razor Pages: Login, Danh sách đề, Làm bài, Kết quả
- [ ] Routing `/exam/{id}` hoạt động, Model Binding đúng
- [ ] 3 phiên bản Concurrency: Thread, Task, Async/Await
- [ ] Console Server chạy và lắng nghe cổng 5000
- [ ] Console Client kết nối và gửi JSON thành công
- [ ] JSON Export tạo file đúng định dạng
- [ ] AuthFilter ngăn truy cập khi chưa đăng nhập
- [ ] ExportService tái sử dụng được
- [ ] DI đăng ký đủ các Services và Repositories
- [ ] Repository Pattern rõ ràng qua GenericRepository
- [ ] SignalR đồng hồ đếm ngược hoạt động realtime
- [ ] SignalR thông báo nộp bài cho admin

---

*Tài liệu này đủ để handoff cho AI developer (Cursor, Copilot, Claude) phát triển hoàn thiện. Mỗi phần đã có code mẫu rõ ràng và mapping với yêu cầu môn học.*
