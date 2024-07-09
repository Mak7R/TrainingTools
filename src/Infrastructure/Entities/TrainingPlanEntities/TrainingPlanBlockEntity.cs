using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static Domain.Rules.DataSizes.TrainingPlanDataSizes;

namespace Infrastructure.Entities.TrainingPlanEntities;

public class TrainingPlanBlockEntity
{
    [Key]
    public Guid Id { get; set; }
    public int Position { get; set; }
    
    [StringLength(MaxBlockNameSize)]
    public string Name { get; set; }
    
    public IEnumerable<TrainingPlanBlockEntryEntity> TrainingPlanBlockEntries { get; set; }
}