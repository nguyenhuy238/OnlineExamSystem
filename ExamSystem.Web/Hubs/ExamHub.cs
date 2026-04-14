using ExamSystem.Web.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace ExamSystem.Web.Hubs;

public class ExamHub : Hub
{
    private const string AdminGroupName = "admin";
    private readonly ExamPresenceTracker _presenceTracker;

    public ExamHub(ExamPresenceTracker presenceTracker)
    {
        _presenceTracker = presenceTracker;
    }

    public async Task JoinExam(int examId, string? examTitle = null)
    {
        if (examId <= 0)
        {
            return;
        }

        if (!IsRole("Student"))
        {
            return;
        }

        var studentId = GetSessionInt(SessionKeys.UserId);
        if (studentId is null || studentId <= 0)
        {
            return;
        }

        var groupName = BuildExamGroup(examId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var studentName = GetSessionValue(SessionKeys.FullName) ?? $"Student #{studentId.Value}";
        var safeExamTitle = string.IsNullOrWhiteSpace(examTitle) ? $"Exam #{examId}" : examTitle.Trim();

        var startedAtUtc = DateTime.UtcNow;
        if (_presenceTracker.TryGet(Context.ConnectionId, out var existingConnection)
            && existingConnection is not null
            && existingConnection.StudentId == studentId.Value
            && existingConnection.ExamId == examId)
        {
            startedAtUtc = existingConnection.StartedAtUtc;
        }

        _presenceTracker.Upsert(Context.ConnectionId, new ActiveExamConnection
        {
            StudentId = studentId.Value,
            StudentName = studentName,
            ExamId = examId,
            ExamTitle = safeExamTitle,
            StartedAtUtc = startedAtUtc
        });

        await Clients.Group(groupName).SendAsync("StudentJoined", studentName, examId);
        await Clients.Group(AdminGroupName).SendAsync("StudentJoined", studentName, examId);
        await BroadcastActiveStudentsAsync();
    }

    public async Task LeaveExam(int examId)
    {
        if (examId > 0)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildExamGroup(examId));
        }

        if (_presenceTracker.TryRemove(Context.ConnectionId, out _))
        {
            await BroadcastActiveStudentsAsync();
        }
    }

    public async Task TimerTick(int examId, int secondsLeft)
    {
        if (examId <= 0 || secondsLeft < 0)
        {
            return;
        }

        var groupName = BuildExamGroup(examId);
        await Clients.Group(groupName).SendAsync("TimerUpdate", secondsLeft, examId);

        if (secondsLeft == 300)
        {
            await Clients.Group(groupName).SendAsync("Warning", "Only 5 minutes left.", examId);
        }
    }

    public async Task NotifySubmit(int examId, string studentName)
    {
        var safeName = string.IsNullOrWhiteSpace(studentName)
            ? GetSessionValue(SessionKeys.FullName) ?? "Unknown Student"
            : studentName.Trim();

        await Clients.Group(AdminGroupName).SendAsync("StudentSubmitted", safeName, examId, DateTime.UtcNow);

        if (_presenceTracker.TryRemove(Context.ConnectionId, out _))
        {
            await BroadcastActiveStudentsAsync();
        }
    }

    public async Task JoinAdmin()
    {
        var role = GetSessionValue(SessionKeys.Role);
        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroupName);
        await Clients.Caller.SendAsync("ActiveExamStudentsChanged", _presenceTracker.GetSnapshot());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_presenceTracker.TryRemove(Context.ConnectionId, out _))
        {
            await BroadcastActiveStudentsAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string BuildExamGroup(int examId)
    {
        return $"exam-{examId}";
    }

    private Task BroadcastActiveStudentsAsync()
    {
        var snapshot = _presenceTracker.GetSnapshot();
        return Clients.Group(AdminGroupName).SendAsync("ActiveExamStudentsChanged", snapshot);
    }

    private bool IsRole(string role)
    {
        var currentRole = GetSessionValue(SessionKeys.Role);
        return string.Equals(currentRole, role, StringComparison.OrdinalIgnoreCase);
    }

    private string? GetSessionValue(string key)
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is null)
        {
            return null;
        }

        try
        {
            return httpContext.Session.GetString(key);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private int? GetSessionInt(string key)
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is null)
        {
            return null;
        }

        try
        {
            return httpContext.Session.GetInt32(key);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
