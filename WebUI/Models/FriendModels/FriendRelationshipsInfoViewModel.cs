using WebUI.Models.UserModels;

namespace WebUI.Models.FriendModels;

public class FriendRelationshipsInfoViewModel
{
    public IEnumerable<FriendInvitationViewModel> InvitationsFor { get; set; }
    public IEnumerable<UserViewModel> Friends { get; set; }
    public IEnumerable<FriendInvitationViewModel> InvitationsOf { get; set; }
}