using Application.Identity;
using Application.Models;
using Domain.Models;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IFriendsRepository
{
    Task<OperationResult> CreateInvitation(FriendInvitation friendInvitation);
    Task<OperationResult> AcceptInvitation(FriendInvitation friendInvitation);
    Task<OperationResult> RemoveInvitation(FriendInvitation friendInvitation);
    
    
    /// <summary>
    /// Returns friend invitations for (where user is target) user with id userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<FriendInvitation>> GetInvitationsFor(Guid userId);
    
    /// <summary>
    /// Returns friend invitations for (where user is invitor) user with id userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<FriendInvitation>> GetInvitationsOf(Guid userId);

    Task<IEnumerable<ApplicationUser>> GetInvitedUsersBy(Guid userId);
    Task<IEnumerable<ApplicationUser>> GetInviters(Guid userId);
    
    Task<IEnumerable<ApplicationUser>> GetFriendsFor(Guid userId);

    // Task<FriendRelationship?> GetRelationshipFor(Guid user1Id, Guid user2Id);
    
    Task<OperationResult> RemoveFriendship(Guid user1Id, Guid user2Id);
}