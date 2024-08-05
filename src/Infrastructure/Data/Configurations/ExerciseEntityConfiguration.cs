using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ExerciseEntityConfiguration : IEntityTypeConfiguration<ExerciseEntity>
{
    public void Configure(EntityTypeBuilder<ExerciseEntity> builder)
    {
        builder
            .ToTable("Exercise")
            .HasIndex(e => e.Name);

        builder
            .HasIndex(e => new {e.Name, e.GroupId})
            .IsUnique();
    }
}