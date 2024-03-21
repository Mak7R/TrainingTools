using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Extensions;
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
    private readonly ISelectedWorkspace _selected;


    public GroupsService(IServiceProvider serviceProvider, TrainingToolsDbContext dbContext, ISelectedWorkspace selected)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _selected = selected;
    }
    
    public async Task Add(Group group)
    {
        if (!_selected.Permission.HasEditPermission()) throw new HasNotPermissionException();
        
        group.Id = Guid.NewGuid();
        group.WorkspaceId = _selected.Workspace.Id;
        
        await _dbContext.Groups.AddAsync(group);
    }

    public async Task<Group?> Get(Expression<Func<Group, bool>> expression)
    {
        if (!_selected.Permission.HasViewPermission()) throw new HasNotPermissionException();
        
        return await _dbContext.Groups
            .AsNoTracking()
            
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Followers)
            
            .Where(g => g.WorkspaceId == _selected.Workspace.Id)
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Group>> GetAll()
    {
        if (!_selected.Permission.HasViewPermission()) throw new HasNotPermissionException();
        
        return await _dbContext.Groups
            .AsNoTracking()
            
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(g => g.WorkspaceId == _selected.Workspace.Id)
            .ToListAsync();
    }

    public async Task Update(Guid groupId, Action<Group> updater)
    {
        if (!_selected.Permission.HasEditPermission()) throw new HasNotPermissionException();
        
        var group = await _dbContext.Groups
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            
            .Where(g => g.WorkspaceId == _selected.Workspace.Id)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        
        if (group == null) throw new NotFoundException($"{nameof(Group)} with id {groupId} was not found");

        updater(group);

        // I can paste here all checks and security. Like check user changed or another errors.
    }

    public async Task Remove(Guid groupId)
    {
        if (!_selected.Permission.HasEditPermission()) throw new HasNotPermissionException();
        
        var group = await _dbContext.Groups
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Include(g => g.Exercises)
            
            .Where(g => g.WorkspaceId == _selected.Workspace.Id)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        
        if (group == null) throw new NotFoundException($"{nameof(Group)} with id {groupId} was not found");

        var exercisesService = _serviceProvider.GetRequiredService<IExercisesService>();
        foreach (var exercise in group.Exercises) await exercisesService.Update(exercise.Id, e => e.Group = null);
        
        _dbContext.Groups.Remove(group);
    }
}