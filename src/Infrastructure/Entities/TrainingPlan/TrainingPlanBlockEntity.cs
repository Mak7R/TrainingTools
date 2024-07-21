using System.ComponentModel.DataAnnotations;
using static Domain.Rules.DataSizes.TrainingPlanDataSizes;

namespace Infrastructure.Entities.TrainingPlan;

public class TrainingPlanBlockEntity
{
    [Key]
    public Guid Id { get; set; }
    public int Position { get; set; }

    [StringLength(MaxBlockNameSize)] public string Title { get; set; } = string.Empty;

    public IList<TrainingPlanBlockEntryEntity> TrainingPlanBlockEntries { get; set; } = new List<TrainingPlanBlockEntryEntity>();
}