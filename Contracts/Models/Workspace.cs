using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Models;

public class Workspace
{
    public const int MaxNameLength = 32;
    
    [Key]
    public Guid Id { get; set; }
    
    [StringLength(MaxNameLength)]
    public string Name { get; set; }
    
    [ForeignKey(nameof(Owner))]
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
}