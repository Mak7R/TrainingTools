using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Identity;
using static Domain.Rules.DataSizes.TrainingPlanDataSizes;


namespace Infrastructure.Entities.TrainingPlanEntities;

public class TrainingPlanEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [StringLength(MaxNameSize)]
    public string Name { get; set; }
    
    [ForeignKey(nameof(Author))]
    public Guid AuthorId { get; set; }
    public ApplicationUser Author { get; set; }
    
    public bool IsPublic { get; set; }
    
    public IEnumerable<TrainingPlanBlockEntity> TrainingPlanBlocks { get; set; }
}