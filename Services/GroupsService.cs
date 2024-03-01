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
    private User? _user;

    private User User
    {
        get => _user ?? throw new NullReferenceException("User was null");
        set => _user = value;
    }

    public GroupsService(TrainingToolsDbContext dbContext)
    {
        _dbContext = dbContext;
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
        await _dbContext.SaveChangesAsync();
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

    public async Task Update(Guid groupId, IGroupsService.UpdateGroupModel groupModel)
    {
        var group = await _dbContext.Groups
            .Include(g => g.Workspace)
            .ThenInclude(w => w.Owner)
            .Where(g => g.Workspace.Owner.Id == User.Id)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        
        if (group == null) throw new NotFoundException($"{nameof(Group)} with id {groupId} was not found");
        
        group.Name = groupModel.Name;

        await _dbContext.SaveChangesAsync();
    }

    public async Task Remove(Group group)
    {
        if (group.Workspace.Owner.Id != User.Id) throw new OperationNotAllowedException("User is not owner of this exercise");
        _dbContext.Groups.Remove(group);
        await _dbContext.SaveChangesAsync();
    }
}