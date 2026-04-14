using ExamSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExamSystem.Data.Configurations;

internal class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(500);
        builder.Property(x => x.OptionA).IsRequired().HasMaxLength(200);
        builder.Property(x => x.OptionB).IsRequired().HasMaxLength(200);
        builder.Property(x => x.OptionC).IsRequired().HasMaxLength(200);
        builder.Property(x => x.OptionD).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CorrectAnswer).IsRequired().HasMaxLength(1).IsFixedLength();

        builder.HasMany(x => x.ResultDetails)
            .WithOne(x => x.Question)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.Questions);
    }
}
