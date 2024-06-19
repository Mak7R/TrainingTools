using Application.Enums;
using Domain.Identity;

namespace Application.Dtos;

public record UserInfo(ApplicationUser User, RelationshipState RelationshipState, IEnumerable<string> Roles);