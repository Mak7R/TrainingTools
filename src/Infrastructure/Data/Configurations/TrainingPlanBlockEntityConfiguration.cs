using Infrastructure.Entities.TrainingPlan;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class TrainingPlanBlockEntityConfiguration : IEntityTypeConfiguration<TrainingPlanBlockEntity>
{
    public void Configure(EntityTypeBuilder<TrainingPlanBlockEntity> builder)
    {
        builder
            .ToTable("TrainingPlanBlock")
            .Property("TrainingPlanEntityId")
            .IsRequired();


        builder
            .HasMany(block => block.TrainingPlanBlockEntries)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}