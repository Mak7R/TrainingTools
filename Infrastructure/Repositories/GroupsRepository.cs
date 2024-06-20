using System.Linq.Expressions;
using Application.Constants;
using Application.Interfaces.RepositoryInterfaces;
using Application.Models.Shared;
using Domain.Defaults;
using Domain.Exceptions;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class GroupsRepository : IGroupsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GroupsRepository> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GroupsRepository(ApplicationDbContext dbContext, ILogger<GroupsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Group>> GetAll(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.Groups.AsNoTracking();

            if ((filterModel?.TryGetValue(FilterOptionNames.Group.Name, out var value) ?? false) && !string.IsNullOrWhiteSpace(value))
            {
                query = query.Where(g => g.Name.Contains(value));
            }
            
            return await query.Select(g => g.ToGroup()).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all groups");
            throw new DataBaseException("Error while receiving groups from database", e);
        }
    }

    public async Task<Group?> GetBy(Expression<Func<GroupEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        try
        {
            var groupEntity = await _dbContext.Groups.AsNoTracking().FirstOrDefaultAsync(predicate);
            return groupEntity?.ToGroup();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving group with expression '{expression}'", predicate);
            throw new DataBaseException("Error while receiving group from database", e);
        }
    }
    
    public async Task<Group?> GetByName(string? name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return await GetBy(g => g.Name == name);
    }

    public async Task<Group?> GetById(Guid id)
    {
        return await GetBy(g => g.Id == id);
    }
    
    public async Task<OperationResult> CreateGroup(Group? group)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(group.Name);
        if (group.Id == Guid.Empty) throw new ArgumentException("GroupId was empty id");
        
        var groupEntity = new GroupEntity { Id = group.Id, Name = group.Name };
        try
        {
            var sameName = await _dbContext.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Name == group.Name);
            if (sameName != null)
                throw new AlreadyExistsException($"Group with name '{group.Name}' already exist in database");
            
            await _dbContext.Groups.AddAsync(groupEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Group with name '{groupName}' already exist in database", group.Name);
            return new DefaultOperationResult(false, alreadyExistsException, new []{alreadyExistsException.Message});
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new group '{groupName}' to database", group.Name);
            var ex = new DataBaseException("Error while adding group to database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true, group);
    }

    public async Task<OperationResult> UpdateGroup(Group? group)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(group.Name);

        try
        {
            var groupEntity = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == group.Id);

            if (groupEntity == null)
                throw new NotFoundException($"Group with id '{group.Id}' was not found");

            if (groupEntity.Name == group.Name)
                return new DefaultOperationResult(true, group);
            
            var sameName = await _dbContext.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Name == group.Name);
            if (sameName != null)
                throw new AlreadyExistsException($"Group with name '{group.Name}' already exist in database");

            groupEntity.Name = group.Name;

            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException e)
        {
            _logger.LogWarning(e, "NotFoundException was thrown for {entity} with id '{entityId}'", "Group", group.Id);
            return new DefaultOperationResult(false, e, new[] { e.Message });
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Group with name '{groupName}' already exist in database", group.Name);
            return new DefaultOperationResult(false, alreadyExistsException, new []{alreadyExistsException.Message});
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating group with id '{groupId}'", group.Id);
            var ex = new DataBaseException("Error while updating group in database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true, group);
    }

    public async Task<OperationResult> DeleteGroup(Guid id)
    {
        Group? group;
        try
        {
            var groupEntity = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == id);

            if (groupEntity == null)
                throw new NotFoundException($"Group with id '{id}' was not found");

            group = new Group
            {
                Id = groupEntity.Id,
                Name = groupEntity.Name
            };
            
            _dbContext.Groups.Remove(groupEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException e)
        {
            _logger.LogWarning(e, "NotFoundException was thrown for {entity} with id '{entityId}'", "Group", id);
            return new DefaultOperationResult(false, e, new[] { e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while deleting group with id '{groupId}'", id);
            var ex = new DataBaseException("Error while deleting group from database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true, group);
    }
}