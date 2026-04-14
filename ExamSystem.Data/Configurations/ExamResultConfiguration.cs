using ExamSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExamSystem.Data.Configurations;

internal class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
{
    public void Configure(EntityTypeBuilder<ExamResult> builder)
    {
        builder.ToTable("ExamResults");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Score).IsRequired();
        builder.Property(x => x.IsPassed).IsRequired();
        builder.Property(x => x.SubmittedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(x => x.Duration).IsRequired();

        builder.HasMany(x => x.Details)
            .WithOne(x => x.Result)
            .HasForeignKey(x => x.ResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
