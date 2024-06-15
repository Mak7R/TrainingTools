using Application.Identity;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Application.Models;
using Domain.Models;

namespace Application.Services;

public class FriendsService : IFriendsService
{
    private readonly IFriendsRepository _friendsRepository;

    public FriendsService(IFriendsRepository friendsRepository)
    {
        _friendsRepository = friendsRepository;
    }
    
    public async Task<OperationResult> CreateInvitation(ApplicationUser invitor, ApplicationUser target)
    {
        ArgumentNullException.ThrowIfNull(invitor);
        ArgumentNullException.ThrowIfNull(target);
        
        var friendInvitation = new FriendInvitation
        {
            Invitor = invitor,
            Target = target,
            InvitationTime = DateTime.Now
        };

        return await _friendsRepository.CreateInvitation(friendInvitation);
    }

    public async Task<OperationResult> AcceptInvitation(ApplicationUser invitor, ApplicationUser target)
    {
        ArgumentNullException.ThrowIfNull(invitor);
        ArgumentNullException.ThrowIfNull(target);
        
        var friendInvitation = new FriendInvitation
        {
            Invitor = invitor,
            Target = target,
            InvitationTime = DateTime.Now
        };

        return await _friendsRepository.AcceptInvitation(friendInvitation);
    }

    public async Task<OperationResult> RemoveInvitation(ApplicationUser invitor, ApplicationUser target)
    {
        ArgumentNullException.ThrowIfNull(invitor);
        ArgumentNullException.ThrowIfNull(target);
        
        var friendInvitation = new FriendInvitation
        {
            Invitor = invitor,
            Target = target,
            InvitationTime = DateTime.Now
        };

        return await _friendsRepository.RemoveInvitation(friendInvitation);
    }

    public async Task<IEnumerable<FriendInvitation>> GetInvitationsFor(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        return await _friendsRepository.GetInvitationsFor(user.Id);
    }

    public async Task<IEnumerable<FriendInvitation>> GetInvitationsOf(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        return await _friendsRepository.GetInvitationsOf(user.Id);
    }

    public async Task<IEnumerable<ApplicationUser>> GetFriendsFor(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        return await _friendsRepository.GetFriendsFor(user.Id);
    }

    public async Task<OperationResult> RemoveFriendship(ApplicationUser user1, ApplicationUser user2)
    {
        ArgumentNullException.ThrowIfNull(user1);
        ArgumentNullException.ThrowIfNull(user2);
        
        return await _friendsRepository.RemoveFriendship(user1.Id, user2.Id);
    }
}