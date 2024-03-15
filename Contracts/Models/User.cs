using System.ComponentModel.DataAnnotations;

namespace Contracts.Models;

public class User
{
    public const int MaxNameLength = 32;
    public const int MaxEmailLength = 320;
    public const int MaxPasswordLength = 32;
    
    [Key]
    public Guid Id { get; set; }
    
    [StringLength(MaxNameLength)]
    public string Name { get; set; }
    
    [StringLength(MaxEmailLength)]
    public string Email { get; set; }
    
    [StringLength(MaxPasswordLength)]
    public string Password { get; set; }
    
    public List<ExerciseResults> UserResults { get; set; }
    public List<Workspace> Workspaces { get; set; }
    
    public override bool Equals(object? obj) => obj is User user && Id.Equals(user.Id);

    public override int GetHashCode() => Id.GetHashCode();
}