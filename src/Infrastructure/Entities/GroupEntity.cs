using System.ComponentModel.DataAnnotations;
using Domain.Rules;

namespace Infrastructure.Entities;

public class GroupEntity
{
    [Key] public Guid Id { get; set; }

    [StringLength(DataSizes.GroupDataSizes.MaxNameSize, MinimumLength = DataSizes.GroupDataSizes.MinNameSize)]
    public string Name { get; set; } = string.Empty;
}