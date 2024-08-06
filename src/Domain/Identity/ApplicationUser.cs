using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using static Domain.Rules.DataSizes.ApplicationUserDataSizes;

namespace Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    [StringLength(MaxUsernameSize, MinimumLength = MinUsernameSize)]
    public override string? UserName { get; set; }

    [StringLength(MaxAboutSize)] public string? About { get; set; }

    public bool IsPublic { get; set; }

    public DateTime RegistrationDateTime { get; set; }
}