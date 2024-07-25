using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ExerciseResultEntityConfiguration : IEntityTypeConfiguration<ExerciseResultEntity>
{
    public void Configure(EntityTypeBuilder<ExerciseResultEntity> builder)
    {
        builder
            .ToTable("ExerciseResult")
            .HasKey(er => new { er.OwnerId, er.ExerciseId });
    }
}