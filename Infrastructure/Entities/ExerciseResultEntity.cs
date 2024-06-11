using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Identity;
using Domain.Rules;

namespace Infrastructure.Entities;

public class ExerciseResultEntity
{
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    [ForeignKey(nameof(Exercise))]
    public Guid ExerciseId { get; set; }
    
    public ExerciseEntity Exercise { get; set; }
    
    [StringLength(DataSizes.ExerciseResults.MaxResultsSize)]
    public string? Results { get; set; }
}