using ExamSystem.Core.Interfaces;
using ExamSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamSystem.Data.Repositories;

public class ExamRepository : GenericRepository<Exam>, IExamRepository
{
    public ExamRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Exam>> GetActiveExamsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Exam?> GetExamWithQuestionsAsync(int examId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);
    }
}
