using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamSystem.Data.Repositories;

public class ResultRepository : GenericRepository<ExamResult>, IResultRepository
{
    public ResultRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ExamResult>> GetAllWithRelationsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Exam)
            .Include(x => x.Details)
                .ThenInclude(x => x.Question)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExamResult>> GetResultsByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Exam)
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExamResult>> GetResultsByExamAsync(int examId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Student)
            .Where(x => x.ExamId == examId)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExamResult?> GetResultWithDetailsAsync(int resultId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Exam)
            .Include(x => x.Details)
                .ThenInclude(x => x.Question)
            .FirstOrDefaultAsync(x => x.Id == resultId, cancellationToken);
    }
}
