using Application.Models;
using Domain.Identity;

namespace WebUI.Models.FriendModels;

public class FriendRelationshipsInfo
{
    public IEnumerable<FriendInvitation> InvitationsFor { get; set; }
    public IEnumerable<ApplicationUser> Friends { get; set; }
    public IEnumerable<FriendInvitation> InvitationsOf { get; set; }
}