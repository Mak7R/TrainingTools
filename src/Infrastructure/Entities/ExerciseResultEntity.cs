using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Identity;
using Domain.Rules;

namespace Infrastructure.Entities;

public class ExerciseResultEntity
{
    [ForeignKey(nameof(Owner))] public Guid OwnerId { get; set; }

    public ApplicationUser Owner { get; set; }

    [ForeignKey(nameof(Exercise))] public Guid ExerciseId { get; set; }

    public ExerciseEntity Exercise { get; set; }

    [StringLength(DataSizes.ExerciseResultsSizes.MaxWeightsSize)]
    public string? Weights { get; set; }

    [StringLength(DataSizes.ExerciseResultsSizes.MaxCountsSize)]
    public string? Counts { get; set; }

    [StringLength(DataSizes.ExerciseResultsSizes.MaxCommentsSize)]
    public string? Comments { get; set; }
}