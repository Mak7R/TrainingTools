using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.DbContexts;

namespace Services;

public class GroupsService : IGroupsService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrainingToolsDbContext _dbContext;
    private readonly IAuthorizedUser _authorized;


    public GroupsService(IServiceProvider serviceProvider, TrainingToolsDbContext dbContext, IAuthorizedUser authorized)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _authorized = authorized;
    }
    
    public async Task Add(Group group)
    {
        var workspace = await _dbContext.Workspaces
            .Where(w => w.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(w => w.Id == group.WorkspaceId);
        if (workspace == null) throw new NotFoundException("Workspace was not found");

        group.Id = Guid.NewGuid();
        group.Workspace = workspace;
        
        await _dbContext.Groups.AddAsync(group);
    }

    public async Task<Group?> Get(Expression<Func<Group, bool>> expression)
    {
        return await _dbContext.Groups
            .AsNoTracking()
            
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(g => g.Workspace.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Group>> GetAll()
    {
        return await _dbContext.Groups
            .AsNoTracking()
            
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(g => g.Workspace.Owner.Equals(_authorized.User))
            .ToListAsync();
    }

    public async Task Update(Guid groupId, Action<Group> updater)
    {
        var group = await _dbContext.Groups
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(g => g.Workspace.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(g => g.Id == groupId);
        
        if (group == null) throw new NotFoundException($"{nameof(Group)} with id {groupId} was not found");

        updater(group);

        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid groupId)
    {
        var group = await _dbContext.Groups
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(g => g.Exercises)
            
            .Where(g => g.Workspace.Owner.Equals(_authorized.User))
            .FirstOrDefaultAsync(g => g.Id == groupId);
        
        if (group == null) throw new NotFoundException($"{nameof(Group)} with id {groupId} was not found");

        var exercisesService = _serviceProvider.GetRequiredService<IExercisesService>();
        foreach (var exercise in group.Exercises) await exercisesService.Update(exercise.Id, e => e.Group = null);
        
        _dbContext.Groups.Remove(group);
    }
}