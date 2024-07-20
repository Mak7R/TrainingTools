using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Domain.Defaults;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Domain.Models.Friendship;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class FriendshipsService : IFriendsService
{
    private readonly IRepository<FriendInvitation, (Guid, Guid)> _friendInvitationsRepository;
    private readonly IRepository<Friendship, (Guid, Guid)> _friendshipsRepository;
    private readonly ILogger<FriendshipsService> _logger;

    public FriendshipsService(IRepository<FriendInvitation, (Guid, Guid)> friendInvitationsRepository, IRepository<Friendship, (Guid, Guid)> friendshipsRepository, ILogger<FriendshipsService> logger)
    {
        _friendInvitationsRepository = friendInvitationsRepository;
        _friendshipsRepository = friendshipsRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<FriendInvitation>> GetInvitationsFor(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        return await _friendInvitationsRepository.GetAll(new FilterModel{{FilterOptionNames.Relationships.FriendInvitation.Invited, user.Id.ToString()}});
    }

    public async Task<IEnumerable<FriendInvitation>> GetInvitationsOf(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        return await _friendInvitationsRepository.GetAll(new FilterModel{{FilterOptionNames.Relationships.FriendInvitation.Invitor, user.Id.ToString()}});
    }

    public async Task<IEnumerable<ApplicationUser>> GetFriendsFor(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return (await _friendshipsRepository.GetAll(new FilterModel
                { { FilterOptionNames.Relationships.Friendship.FriendId, user.Id.ToString() } }))
            .Select(f => f.FirstFriend.Id == user.Id ? f.SecondFriend : f.FirstFriend);
    }

    public async Task<OperationResult> CreateInvitation(ApplicationUser invitor, ApplicationUser invited)
    {
        ArgumentNullException.ThrowIfNull(invitor);
        ArgumentNullException.ThrowIfNull(invited);

        var existFriendship = await _friendshipsRepository.GetById((invitor.Id, invited.Id));
        if (existFriendship is not null)
        {
            _logger.LogInformation("Users ('{invitor}'; '{invitor}') already are friends", invitor.Id, invited.Id);
            return DefaultOperationResult.FromException(new NotFoundException("Users already are friends"));
        }
        
        return await _friendInvitationsRepository.Create(new FriendInvitation
        {
            Invitor = invitor,
            Invited = invited,
            InvitationDateTime = DateTime.Now
        });
    }

    public async Task<OperationResult> AcceptInvitation(ApplicationUser invitor, ApplicationUser invited)
    {
        ArgumentNullException.ThrowIfNull(invitor);
        ArgumentNullException.ThrowIfNull(invited);

        var result = await _friendInvitationsRepository.Delete((invitor.Id, invited.Id));

        if (!result.IsSuccessful) return result;
        
        var friendship = new Friendship
        {
            FirstFriend = invitor,
            SecondFriend = invited,
            FriendsFromDateTime = DateTime.Now
        };

        return await _friendshipsRepository.Create(friendship);
    }

    public async Task<OperationResult> RemoveInvitation(ApplicationUser invitor, ApplicationUser invited)
    {
        ArgumentNullException.ThrowIfNull(invitor);
        ArgumentNullException.ThrowIfNull(invited);

        return await _friendInvitationsRepository.Delete((invitor.Id, invited.Id));
    }
    
    public async Task<OperationResult> RemoveFriendship(ApplicationUser user1, ApplicationUser user2)
    {
        ArgumentNullException.ThrowIfNull(user1);
        ArgumentNullException.ThrowIfNull(user2);
        
        return await _friendshipsRepository.Delete((user1.Id, user2.Id));
    }
}