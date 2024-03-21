using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class SelectedWorkspace : ISelectedWorkspace
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrainingToolsDbContext _dbContext;

    private Workspace? _workspace;
    private WorkspacePermission? _permission;
    
    public SelectedWorkspace(IServiceProvider serviceProvider, IAuthorizedUser authorized, TrainingToolsDbContext dbContext)
    {
        _serviceProvider = serviceProvider;
        Authorized = authorized;
        _dbContext = dbContext;
    }

    public IAuthorizedUser Authorized { get; }

    public Workspace Workspace => _workspace ?? throw new Exception("Workspace was not selected");
    public WorkspacePermission Permission => _permission ?? throw new Exception("Workspace was not selected");
    
    
    public async Task Select(Guid workspaceId)
    {
        var workspace = await _dbContext.Workspaces
            .AsNoTracking()
            .Include(w => w.Owner)
            .Include(w => w.Followers)
            
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
        
        if (workspace == null) throw new NotFoundException("Workspace was not found");
        
        if (workspace.Owner.Equals(Authorized.User))
        {
            _workspace = workspace;
            _permission = WorkspacePermission.OwnerPermission;
            return;
        }

        var follower = workspace.Followers.FirstOrDefault(fr => fr.FollowerId == Authorized.User.Id);
        if (follower != null)
        {
            _workspace = workspace;
            _permission = follower.FollowerRights switch
            {
                FollowerRights.PendingAccess => WorkspacePermission.FollowedPermission,
                FollowerRights.ViewOnly => WorkspacePermission.ViewPermission,
                FollowerRights.UseOnly => WorkspacePermission.UsePermission,
                FollowerRights.All => WorkspacePermission.EditPermission,
                _ => throw new ArgumentOutOfRangeException()
            };
            return;
        }

        if (workspace.IsPublic)
        {
            _workspace = workspace;
            _permission = WorkspacePermission.PermissionDenied;
            return;
        }

        throw new NotFoundException("Workspace was not found");
    }

    public void Unselect()
    {
        _workspace = null;
        _permission = null;
    }
}