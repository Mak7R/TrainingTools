using System.Linq.Expressions;
using Contracts;
using Contracts.Exceptions;
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
    private readonly IAuthorizedUser _authorized;

    public WorkspacesService(IServiceProvider serviceProvider, TrainingToolsDbContext dbContext, IAuthorizedUser authorized)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _authorized = authorized;
    }

    public async Task Add(Workspace workspace)
    {
        workspace.Id = Guid.NewGuid();
        workspace.OwnerId = _authorized.User.Id;
        await _dbContext.Workspaces.AddAsync(workspace);
    }

    public async Task<Workspace?> Get(Expression<Func<Workspace, bool>> expression)
    {
        return await _dbContext.Workspaces
            .AsNoTracking()
            
            .Include(w => w.Owner)
            
            .Where(w => w.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Workspace>> GetAll()
    {
        return await _dbContext.Workspaces
            .AsNoTracking()
            
            .Include(w => w.Owner)
            
            .Where(w => w.Owner.Equals(_authorized.User))
            .ToListAsync();
    }

    public async Task Update(Guid workspaceId, Action<Workspace> updater)
    {
        var workspace = await _dbContext.Workspaces
            .Include(w => w.Owner)
            
            .Where(w => w.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace == null) throw new NotFoundException($"{nameof(Workspace)} with id {workspaceId} was not found");

        updater(workspace);
        
        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid workspaceId)
    {
        var workspace = await _dbContext.Workspaces
            .Include(w => w.Owner)
            .Include(w => w.Groups)
            .Include(w => w.Exercises)
            
            .Where(w => w.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
        
        if (workspace == null) throw new NotFoundException($"{nameof(Workspace)} with id {workspaceId} was not found");

        var groupsService = _serviceProvider.GetRequiredService<IGroupsService>();
        var exercisesService = _serviceProvider.GetRequiredService<IExercisesService>();

        foreach (var group in workspace.Groups) await groupsService.Remove(group.Id);
        foreach(var exercise in workspace.Exercises) await exercisesService.Remove(exercise.Id);
        
        _dbContext.Workspaces.Remove(workspace);
    }
}