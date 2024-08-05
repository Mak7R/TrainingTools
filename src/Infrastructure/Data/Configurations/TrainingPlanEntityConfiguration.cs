using Infrastructure.Entities.TrainingPlan;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class TrainingPlanEntityConfiguration : IEntityTypeConfiguration<TrainingPlanEntity>
{
    public void Configure(EntityTypeBuilder<TrainingPlanEntity> builder)
    {
        builder
            .ToTable("TrainingPlan")
            .HasKey(plan => plan.Id);

        builder
            .HasIndex(plan => new { plan.AuthorId, Name = plan.Title })
            .IsUnique();

        builder
            .HasIndex(plan => plan.Title);
        
        builder
            .HasMany(p => p.TrainingPlanBlocks)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}