using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Rules;

namespace Infrastructure.Entities;

public class ExerciseEntity
{
    [Key] public Guid Id { get; set; }
    [StringLength(DataSizes.Exercise.MaxNameSize)] public string Name { get; set; }
    
    [ForeignKey(nameof(Group))]
    public Guid GroupId { get; set; }
    
    public GroupEntity Group { get; set; }
}