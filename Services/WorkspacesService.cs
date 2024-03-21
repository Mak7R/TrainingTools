using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Extensions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.DbContexts;

namespace Services;

public class WorkspacesService : IWorkspacesService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrainingToolsDbContext _dbContext;
    private readonly ISelectedWorkspace _selected;
    private readonly IAuthorizedUser _authorized;

    public WorkspacesService(IServiceProvider serviceProvider, TrainingToolsDbContext dbContext, ISelectedWorkspace selected)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _selected = selected;
        _authorized = selected.Authorized;
    }

    public async Task Add(Workspace workspace)
    {
        workspace.Id = Guid.NewGuid();
        workspace.OwnerId = _authorized.User.Id;
        await _dbContext.Workspaces.AddAsync(workspace);
    }
    
    public async Task<Workspace?> Get(Guid workspaceId)
    {
        return await _dbContext.Workspaces
            .AsNoTracking()
            .Include(w => w.Owner)
            .Where(w => w.IsPublic)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
    }

    public async Task<IEnumerable<Workspace>> GetRange(Expression<Func<Workspace, bool>> predicate)
    {
        return await _dbContext.Workspaces
            .AsNoTracking()
            
            .Include(w => w.Owner)
            
            .Where(w => (_authorized.IsAuthorized && w.Owner.Equals(_authorized.User)) || w.IsPublic)
            .Where(predicate)
            .ToListAsync();
    }
    
    /// <summary>
    /// Workspace must be selected before updating
    /// </summary>
    /// <param name="updater"></param>
    /// <exception cref="HasNotPermissionException"></exception>
    public async Task Update(Action<Workspace> updater)
    {
        if (!_selected.Permission.HasEditPermission()) throw new HasNotPermissionException();

        var workspace = await _dbContext.Workspaces
            .Include(w => w.Groups)
            .Include(w => w.Exercises)
            .Include(w => w.Followers)

            .FirstOrDefaultAsync(w => w.Id == _selected.Workspace.Id);

        if (workspace == null) throw new NotFoundException("Workspace was not found");
        
        updater(workspace);

        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid workspaceId)
    {
        var workspace = await _dbContext.Workspaces
            .Include(w => w.Groups)
            .Include(w => w.Exercises)
            .Include(w => w.Followers)
            .Where(w => w.OwnerId == _authorized.User.Id)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null) throw new NotFoundException("Workspace was not found");

        var groupsService = _serviceProvider.GetRequiredService<IGroupsService>();
        var exercisesService = _serviceProvider.GetRequiredService<IExercisesService>();

        await _selected.Select(workspaceId);
        foreach(var exercise in workspace.Exercises) await exercisesService.Remove(exercise.Id);
        foreach (var follower in workspace.Followers) _dbContext.FollowerRelationships.Remove(follower);
        foreach (var group in workspace.Groups) await groupsService.Remove(group.Id);
        
        _dbContext.Workspaces.Remove(workspace);
    }
}