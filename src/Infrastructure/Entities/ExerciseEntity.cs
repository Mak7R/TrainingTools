using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Rules;

namespace Infrastructure.Entities;

public class ExerciseEntity
{
    [Key] public Guid Id { get; set; }

    [StringLength(DataSizes.ExerciseDataSizes.MaxNameSize)]
    public string Name { get; set; } = string.Empty;

    [ForeignKey(nameof(Group))] public Guid GroupId { get; set; }

    public GroupEntity Group { get; set; }

    [StringLength(DataSizes.ExerciseDataSizes.MaxAboutSize)]
    public string? About { get; set; }
}