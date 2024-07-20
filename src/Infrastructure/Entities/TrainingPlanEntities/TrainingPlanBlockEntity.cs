using System.ComponentModel.DataAnnotations;

using static Domain.Rules.DataSizes.TrainingPlanDataSizes;

namespace Infrastructure.Entities.TrainingPlanEntities;

public class TrainingPlanBlockEntity
{
    [Key]
    public Guid Id { get; set; }
    public int Position { get; set; }
    
    [StringLength(MaxBlockNameSize)]
    public string Title { get; set; }
    
    public List<TrainingPlanBlockEntryEntity> TrainingPlanBlockEntries { get; set; }
}