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
    private User? _user;

    private User User
    {
        get => _user ?? throw new NullReferenceException("User was null");
        set => _user = value;
    }

    public WorkspacesService(TrainingToolsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void SetUser(User user)
    {
        User = user;
    }

    public async Task Add(Workspace workspace)
    {
        workspace.OwnerId = User.Id;
        await _dbContext.Workspaces.AddAsync(workspace);
        await _dbContext.SaveChangesAsync();
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

    public async Task Update(Guid workspaceId, IWorkspacesService.UpdateWorkspaceModel workspaceModel)
    {
        var workspace = await _dbContext.Workspaces
            .Include(w => w.Owner)
            .Where(w => w.Owner.Id == User.Id)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace == null) throw new NotFoundException($"{nameof(Workspace)} with id {workspaceId} was not found");
        
        workspace.Name = workspaceModel.Name;

        await _dbContext.SaveChangesAsync();
    }

    public async Task Remove(Workspace workspace)
    {
        if (workspace.Owner.Id != User.Id) throw new OperationNotAllowedException("User is not owner of this workspace");
        _dbContext.Workspaces.Remove(workspace);
        await _dbContext.SaveChangesAsync();
    }
}