using Application.Enums;
using Application.Identity;

namespace Application.Dtos;

public record UserInfo(ApplicationUser User, RelationshipState RelationshipState, IEnumerable<string> Roles);