using System.Linq.Expressions;
using Contracts;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services.DbContexts;

namespace Services;

public class WorkspacesService : IWorkspacesService
{
    private readonly TrainingToolsDbContext _dbContext;
    private readonly IUsersAuthorizer _usersAuthorizer;
    private User? _user;

    private User User
    {
        get => _user ?? throw new NullReferenceException("User was null");
        set => _user = value;
    }

    public WorkspacesService(TrainingToolsDbContext dbContext, IUsersAuthorizer usersAuthorizer)
    {
        _dbContext = dbContext;
        _usersAuthorizer = usersAuthorizer;
    }

    public void SetUser(User user)
    {
        User = user;
    }

    public async Task Add(Workspace workspace)
    {
        workspace.OwnerId = User.Id;
        await _dbContext.Workspaces.AddAsync(workspace);
    }

    public async Task<Workspace?> Get(Expression<Func<Workspace, bool>> expression)
    {
        return await _dbContext.Workspaces
            .AsNoTracking()
            .Include(w => w.Owner)
            .Where(w => w.Owner.Id == User.Id)
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<Workspace>> GetAll()
    {
        return await _dbContext.Workspaces
            .AsNoTracking()
            .Include(w => w.Owner)
            .Where(w => w.Owner.Id == User.Id)
            .ToListAsync();
    }

    public async Task Update(Guid workspaceId, Action<Workspace> updater)
    {
        var workspace = await _dbContext.Workspaces
            .Include(w => w.Owner)
            .Where(w => w.Owner.Id == User.Id)
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
            .Where(w => w.Owner.Id == User.Id)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
        
        if (workspace == null) throw new NotFoundException($"{nameof(Workspace)} with id {workspaceId} was not found");

        var groupsService = _usersAuthorizer.GetServiceForUser<IGroupsService>(User);
        var exercisesService = _usersAuthorizer.GetServiceForUser<IExercisesService>(User);

        foreach (var group in workspace.Groups) await groupsService.Remove(group.Id);
        foreach(var exercise in workspace.Exercises) await exercisesService.Remove(exercise.Id);
        
        _dbContext.Workspaces.Remove(workspace);
    }
}