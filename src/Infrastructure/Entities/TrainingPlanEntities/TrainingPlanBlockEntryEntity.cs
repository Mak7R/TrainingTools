using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static Domain.Rules.DataSizes.TrainingPlanDataSizes;

namespace Infrastructure.Entities.TrainingPlanEntities;

public class TrainingPlanBlockEntryEntity
{
    [Key]
    public Guid Id { get; set; }
    public int Position { get; set; }
    
    [ForeignKey(nameof(Group))]
    public Guid GroupId { get; set; }
    public GroupEntity Group { get; set; }
    
    [StringLength(MaxBlockEntryDescriptionSize)]
    public string Desctiption { get; set; }
}