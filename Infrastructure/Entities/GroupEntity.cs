using System.ComponentModel.DataAnnotations;
using Domain.Rules;

namespace Infrastructure.Entities;

public class GroupEntity
{
    [Key] public Guid Id { get; set; }
    
    [StringLength(DataSizes.Group.MaxNameSize, MinimumLength = DataSizes.Group.MinNameSize)] public string Name { get; set; }
}