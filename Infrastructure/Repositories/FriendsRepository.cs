using Application.Identity;
using Application.Interfaces.RepositoryInterfaces;
using Application.Models;
using Domain.Defaults;
using Domain.Exceptions;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class FriendsRepository : IFriendsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FriendsRepository> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FriendsRepository(ApplicationDbContext dbContext, ILogger<FriendsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<OperationResult> CreateInvitation(FriendInvitation friendInvitation)
    {
        ArgumentNullException.ThrowIfNull(friendInvitation);
        ArgumentNullException.ThrowIfNull(friendInvitation.Invitor);
        ArgumentNullException.ThrowIfNull(friendInvitation.Target);
        
        FriendRelationshipEntity? existFriendship;
        try
        {
            existFriendship = await _dbContext.FriendRelationships.FirstOrDefaultAsync(fr =>
                (fr.FirstFriendId == friendInvitation.Invitor.Id && fr.SecondFriendId == friendInvitation.Target.Id) || 
                (fr.FirstFriendId == friendInvitation.Target.Id && fr.SecondFriendId == friendInvitation.Invitor.Id));
        }
        catch (Exception e)
        {
            return DataBaseExceptionResult(e);
        }
        
        if (existFriendship != null)
        {
            _logger.LogError(
                "Users '{invitor}' and '{target}' already are friends. Impossible to create new Invitation", friendInvitation.Invitor.Id, friendInvitation.Target.Id);
            var exception = new NotFoundException($"Users '{friendInvitation.Invitor.UserName}' and '{friendInvitation.Target.UserName}' already are friends");
            return new DefaultOperationResult(false, exception,new []{exception.Message});
        }

        FriendInvitationEntity? existFriendInvitation;
        try
        {
            existFriendInvitation = await _dbContext.FriendInvitations
                .Include(friendInvitationEntity => friendInvitationEntity.Invitor)
                .Include(friendInvitationEntity => friendInvitationEntity.Target)
                .FirstOrDefaultAsync(fi =>
                    (fi.InvitorId == friendInvitation.Invitor.Id && fi.TargetId == friendInvitation.Target.Id) || 
                    (fi.InvitorId == friendInvitation.Target.Id && fi.TargetId == friendInvitation.Invitor.Id));
        }
        catch (Exception e)
        {
            return DataBaseExceptionResult(e);
        }

        if (existFriendInvitation != null)
        {
            _logger.LogError(
                "'{Invitor}' has already invited '{Target}'. Impossible to create new Invitation", existFriendInvitation.Invitor.UserName, existFriendInvitation.Target.UserName);
            var exception = new NotFoundException($"'{existFriendInvitation.Invitor.UserName}' has already invited '{existFriendInvitation.Target.UserName}'");
            return new DefaultOperationResult(false, exception,new []{exception.Message});
        }

        var invitation = new FriendInvitationEntity { Invitor = friendInvitation.Invitor, Target = friendInvitation.Invitor, InvitationTime = friendInvitation.InvitationTime };

        try
        {
            await _dbContext.FriendInvitations.AddAsync(invitation);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return DataBaseExceptionResult(e);
        }

        return new DefaultOperationResult(true, invitation);

        DefaultOperationResult DataBaseExceptionResult(Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new friend invitation (Invitor:'{invitor}' Target:{target}) to database", friendInvitation.Invitor.Id, friendInvitation.Target.Id);
            var ex = new DataBaseException("Error while adding new friend invitation", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }
    }

    public async Task<OperationResult> AcceptInvitation(FriendInvitation friendInvitation)
    {
        FriendInvitationEntity? invitation;
        try
        {
            invitation = await _dbContext.FriendInvitations.FirstOrDefaultAsync(fi => fi.InvitorId == friendInvitation.Invitor.Id && fi.TargetId == friendInvitation.Target.Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while accepting friend invitation (Invitor:'{invitor}' Target:{target}) to database", friendInvitation.Invitor.Id, friendInvitation.Target.Id);
            var ex = new DataBaseException("Error while accepting friend invitation", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }
        
        if (invitation == null)
        {
            _logger.LogError("Invitation ({invitor}; {target}) was not found", friendInvitation.Invitor.Id, friendInvitation.Target.Id);
            var exception = new NotFoundException("Invitation was not found");
            return new DefaultOperationResult(false, exception,new []{ exception.Message});
        }
        
        try
        {
            var friendRelationshipEntity = new FriendRelationshipEntity
                { FirstFriend = friendInvitation.Invitor, SecondFriend = friendInvitation.Target, FriendsFrom = DateTime.Now };
            _dbContext.FriendInvitations.Remove(invitation);
            await _dbContext.FriendRelationships.AddAsync(friendRelationshipEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while accepting friend invitation (Invitor:'{invitor}' Target:{target}) to database", friendInvitation.Invitor.Id, friendInvitation.Target.Id);
            var ex = new DataBaseException("Error while accepting friend invitation", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true);
    }

    public async Task<OperationResult> RemoveInvitation(FriendInvitation friendInvitation)
    {
        FriendInvitationEntity? invitation;
        try
        {
            invitation = await _dbContext.FriendInvitations.FirstOrDefaultAsync(fi => fi.InvitorId == friendInvitation.Invitor.Id && fi.TargetId == friendInvitation.Target.Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while removing friend invitation (Invitor:'{invitor}' Target:{target}) to database", friendInvitation.Invitor.Id, friendInvitation.Target.Id);
            var ex = new DataBaseException("Error while adding new friend invitation", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        if (invitation == null)
        {
            _logger.LogError(
                "Invitation ({invitor}; {target}) was not found", friendInvitation.Invitor.Id, friendInvitation.Target.Id);
            var exception = new NotFoundException("Invitation was not found");
            return new DefaultOperationResult(false, exception,new []{ exception.Message});
        }

        try
        {
            _dbContext.FriendInvitations.Remove(invitation);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while removing invitation ({invitor}; {target}) from database", friendInvitation.Invitor.Id, friendInvitation.Target.Id);
            var ex = new DataBaseException("Error while removing invitation", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true);
    }
    public async Task<IEnumerable<FriendInvitation>> GetInvitationsFor(Guid userId)
    {
        try
        {
            return await _dbContext.FriendInvitations
                .Where(fi => fi.TargetId == userId)
                .Select(fi => new FriendInvitation
                    { Invitor = fi.Invitor, Target = fi.Target, InvitationTime = fi.InvitationTime})
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while getting invitations for user '{userId}' from database", userId);
            throw new DataBaseException("Error while getting invitations for user", e);
        }
    }
    public async Task<IEnumerable<FriendInvitation>> GetInvitationsOf(Guid userId)
    {
        try
        {
            return await _dbContext.FriendInvitations
                .Where(fi => fi.InvitorId == userId)
                .Select(fi => new FriendInvitation
                    { Invitor = fi.Invitor, Target = fi.Target, InvitationTime = fi.InvitationTime})
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while getting invitations of user '{userId}' from database", userId);
            throw new DataBaseException("Error while getting invitations of user", e);
        }
    }
    
    public async Task<IEnumerable<ApplicationUser>> GetInvitedUsersBy(Guid userId)
    {
        try
        {
            return await _dbContext.FriendInvitations
                .Where(fi => fi.InvitorId == userId)
                .Select(fi => fi.Target)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while getting invited users by user '{userId}' from database", userId);
            throw new DataBaseException("Error while getting invited users by user", e);
        }
    }

    public async Task<IEnumerable<ApplicationUser>> GetInviters(Guid userId)
    {
        try
        {
            return await _dbContext.FriendInvitations
                .Where(fi => fi.TargetId == userId)
                .Select(fi => fi.Invitor)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while getting inviters for user '{userId}' from database", userId);
            throw new DataBaseException("Error while getting inviters for user", e);
        }
    }
    public async Task<IEnumerable<ApplicationUser>> GetFriendsFor(Guid userId)
    {
        try
        {
            return await _dbContext.FriendRelationships
                .Where(fr =>
                    fr.FirstFriendId == userId || fr.SecondFriendId == userId)
                .Select(fr => fr.FirstFriendId == userId ? fr.SecondFriend : fr.FirstFriend)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while getting friends for user '{userId}' from database", userId);
            throw new DataBaseException("Error while getting friends for user", e);
        }
    }

    public async Task<OperationResult> RemoveFriendship(Guid user1Id, Guid user2Id)
    {
        var friendship = await _dbContext.FriendRelationships.FirstOrDefaultAsync(fr =>
            (fr.FirstFriendId == user1Id && fr.SecondFriendId == user2Id) || 
            (fr.FirstFriendId == user2Id && fr.SecondFriendId == user1Id));

        if (friendship == null)
        {
            _logger.LogError(
                "Users '{user1}' and '{user2}' are not friends. Impossible to delete friendship", user1Id, user2Id);
            var exception = new NotFoundException($"Users '{user1Id}' and '{user2Id}' are not friends");
            return new DefaultOperationResult(false, exception, new []{exception.Message});
        }

        try
        {
            _dbContext.FriendRelationships.Remove(friendship);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while removing friendship of '{firstFriend}' Target:{secondFriend}) from database", user1Id, user2Id);
            var ex = new DataBaseException("Error while adding removing friendship", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true);
    }
}