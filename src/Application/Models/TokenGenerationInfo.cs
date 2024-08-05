using Domain.Identity;

namespace Application.Models;

public class TokenGenerationInfo
{
    public ApplicationUser User { get; set; }
    public IEnumerable<string> Roles { get; set; }
}