using System.Collections.Concurrent;

namespace ExamSystem.Web.Infrastructure;

public class ExamPresenceTracker
{
    private readonly ConcurrentDictionary<string, ActiveExamConnection> _connections = new(StringComparer.Ordinal);

    public void Upsert(string connectionId, ActiveExamConnection connection)
    {
        _connections[connectionId] = connection;
    }

    public bool TryRemove(string connectionId, out ActiveExamConnection? removed)
    {
        if (_connections.TryRemove(connectionId, out var active))
        {
            removed = active;
            return true;
        }

        removed = null;
        return false;
    }

    public bool TryGet(string connectionId, out ActiveExamConnection? connection)
    {
        if (_connections.TryGetValue(connectionId, out var active))
        {
            connection = active;
            return true;
        }

        connection = null;
        return false;
    }

    public IReadOnlyList<ActiveExamStudentDto> GetSnapshot()
    {
        return _connections
            .Values
            .GroupBy(x => new { x.StudentId, x.ExamId })
            .Select(group =>
            {
                var first = group.First();
                var startedAt = group.Min(x => x.StartedAtUtc);

                return new ActiveExamStudentDto
                {
                    StudentId = first.StudentId,
                    StudentName = first.StudentName,
                    ExamId = first.ExamId,
                    ExamTitle = first.ExamTitle,
                    StartedAtUtc = startedAt,
                    ConnectionCount = group.Count()
                };
            })
            .OrderBy(x => x.StartedAtUtc)
            .ThenBy(x => x.StudentName)
            .ToList();
    }
}

public sealed class ActiveExamConnection
{
    public int StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public int ExamId { get; init; }
    public string ExamTitle { get; init; } = string.Empty;
    public DateTime StartedAtUtc { get; init; }
}

public sealed class ActiveExamStudentDto
{
    public int StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public int ExamId { get; init; }
    public string ExamTitle { get; init; } = string.Empty;
    public DateTime StartedAtUtc { get; init; }
    public int ConnectionCount { get; init; }
}
