using Domain.Identity;
using Domain.Models;
using Domain.Models.Friendship;

namespace Application.Interfaces.Services;

public interface IFriendsService
{ 
    public Task<IEnumerable<FriendInvitation>> GetInvitationsFor(ApplicationUser user);
    public Task<IEnumerable<FriendInvitation>> GetInvitationsOf(ApplicationUser user);
    
    public Task<IEnumerable<ApplicationUser>> GetFriendsFor(ApplicationUser user);
    
    public Task<OperationResult> CreateInvitation(ApplicationUser invitor, ApplicationUser invited);
    public Task<OperationResult> AcceptInvitation(ApplicationUser invitor, ApplicationUser invited);
    public Task<OperationResult> RemoveInvitation(ApplicationUser invitor, ApplicationUser invited);
    public Task<OperationResult> RemoveFriendship(ApplicationUser user1, ApplicationUser user2);
}