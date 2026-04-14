using ExamSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExamSystem.Data.Configurations;

internal class ResultDetailConfiguration : IEntityTypeConfiguration<ResultDetail>
{
    public void Configure(EntityTypeBuilder<ResultDetail> builder)
    {
        builder.ToTable("ResultDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChosenAnswer).HasMaxLength(1).IsFixedLength();
        builder.Property(x => x.IsCorrect).IsRequired();
    }
}
