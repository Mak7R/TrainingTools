using Infrastructure.Entities.TrainingPlan;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class TrainingPlanBlockEntryEntityConfiguration : IEntityTypeConfiguration<TrainingPlanBlockEntryEntity>
{
    public void Configure(EntityTypeBuilder<TrainingPlanBlockEntryEntity> builder)
    {
        builder
            .ToTable("TrainingPlanBlockEntry");
    }
}