using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Models;

public class ExerciseResults
{
    public Guid Id { get; set; }
    
    [ForeignKey(nameof(Owner))]
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
    
    [ForeignKey(nameof(Exercise))]
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
    
    public List<ExerciseResultEntry> Results { get; set; }
}