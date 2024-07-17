using WebUI.Models.User;

namespace WebUI.Models.Friend;

public class FriendRelationshipsInfoViewModel
{
    public IEnumerable<FriendInvitationViewModel> InvitationsFor { get; set; }
    public IEnumerable<UserViewModel> Friends { get; set; }
    public IEnumerable<FriendInvitationViewModel> InvitationsOf { get; set; }
}