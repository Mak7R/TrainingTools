using System.Linq.Expressions;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class GroupsService : IGroupsService
{
    private readonly TrainingToolsDbContext _dbContext;
    private readonly IUsersAuthorizer _usersAuthorizer;
    private User? _user;

    private User User
    {
        get => _user ?? throw new NullReferenceException("User was null");
        set => _user = value;
    }

    public GroupsService(TrainingToolsDbContext dbContext, IUsersAuthorizer usersAuthorizer)
    {
        _dbContext = dbContext;
        _usersAuthorizer = usersAuthorizer;
    }

    public void SetUser(User user)
    {
        User = user;
    }

    public async Task Add(Group group)
    {
        var workspace = await _dbContext.Workspaces
            .Where(w => w.Owner.Id == User.Id)
            .FirstOrDefaultAsync(w => w.Id == group.WorkspaceId);
        if (workspace == null) throw new NotFoundException("Workspace was not found");
        
        group.Workspace = workspace;
        
        await _dbContext.Groups.AddAsync(group);
    }

    public async Task<Group?> Get(Expression<Func<Group, bool>> expression)
    {
        return await _dbContext.Groups
            .AsNoTracking()
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(g => g.Workspace.Owner.Id == User.Id)
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Group>> GetAll()
    {
        return await _dbContext.Groups
            .AsNoTracking()
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(g => g.Workspace.Owner.Id == User.Id)
            .ToListAsync();
    }

    public async Task Update(Guid groupId, Action<Group> updater)
    {
        var group = await _dbContext.Groups
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(g => g.Workspace.Owner.Id == User.Id)
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
            .Where(g => g.Workspace.Owner.Id == User.Id)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        
        if (group == null) throw new NotFoundException($"{nameof(Group)} with id {groupId} was not found");

        var exercisesService = _usersAuthorizer.GetServiceForUser<IExercisesService>(User);
        foreach (var exercise in group.Exercises) await exercisesService.Update(exercise.Id, e => e.Group = null);
        
        _dbContext.Groups.Remove(group);
    }
}