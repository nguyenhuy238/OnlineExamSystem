using ExamSystem.Web.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace ExamSystem.Web.Hubs;

public class ExamHub : Hub
{
    public async Task JoinExam(int examId)
    {
        if (examId <= 0)
        {
            return;
        }

        var groupName = BuildExamGroup(examId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var studentName = Context.GetHttpContext()?.Session.GetString(SessionKeys.FullName) ?? "Unknown Student";
        await Clients.Group(groupName).SendAsync("StudentJoined", studentName, examId);
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
            ? Context.GetHttpContext()?.Session.GetString(SessionKeys.FullName) ?? "Unknown Student"
            : studentName.Trim();

        await Clients.Group("admin").SendAsync("StudentSubmitted", safeName, examId, DateTime.UtcNow);
    }

    public async Task JoinAdmin()
    {
        var role = Context.GetHttpContext()?.Session.GetString(SessionKeys.Role);
        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
    }

    private static string BuildExamGroup(int examId)
    {
        return $"exam-{examId}";
    }
}
