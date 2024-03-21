using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Extensions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class FollowersService : IFollowersService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrainingToolsDbContext _dbContext;
    private readonly ISelectedWorkspace _selected;

    public FollowersService(IServiceProvider serviceProvider, TrainingToolsDbContext dbContext, ISelectedWorkspace selected)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _selected = selected;
    }

    public async Task<IEnumerable<FollowerRelationship>> GetFollows()
    {
        return await _dbContext.FollowerRelationships
            .AsNoTracking()
            .Where(fr => fr.FollowerId == _selected.Authorized.User.Id)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<FollowerRelationship>> GetFollowers()
    {
        if (!_selected.Permission.HasOwnerPermission()) throw new HasNotPermissionException("User is not an owner of workspace");
        
        return await _dbContext.FollowerRelationships
            .Include(fr => fr.Workspace)
            .Include(fr => fr.Follower)
            
            .Where(fr => fr.WorkspaceId == _selected.Workspace.Id)
            .ToListAsync();
    }

    public async Task AddFollower(Guid workspaceId)
    {
        var workspace = await _dbContext.Workspaces
            .Include(w => w.Owner)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null) throw new NotFoundException("Workspace was not found");
        if (workspace.Owner.Equals(_selected.Authorized.User)) throw new Exception("User cannot follow own workspace");

        await _dbContext.FollowerRelationships
            .AddAsync(new FollowerRelationship
                { 
                    WorkspaceId = workspace.Id, 
                    FollowerId = _selected.Authorized.User.Id, 
                    FollowerRights = FollowerRights.PendingAccess 
                });
    }

    public async Task UpdateFollower(Guid followerId, Action<FollowerRelationship> updater)
    {
       if (!_selected.Permission.HasOwnerPermission()) throw new HasNotPermissionException("User is not an owner of workspace");

        var follower = await _dbContext.FollowerRelationships.FirstOrDefaultAsync(fr =>
            fr.WorkspaceId == _selected.Workspace.Id && fr.FollowerId == followerId);

        if (follower == null) throw new NotFoundException("Follower was not found");
        updater(follower);
        
        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task RemoveFollower(Guid followerId)
    {
        if (!_selected.Permission.HasOwnerPermission()) throw new HasNotPermissionException("User is not an owner of workspace");
        
        var follower = await _dbContext.FollowerRelationships.FirstOrDefaultAsync(fr =>
            fr.WorkspaceId == _selected.Workspace.Id && fr.FollowerId == followerId);

        if (follower == null) throw new NotFoundException("Follower was not found");

        _dbContext.Remove(follower);
    }
}