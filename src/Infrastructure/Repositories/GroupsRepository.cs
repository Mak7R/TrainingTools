using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Models.Shared;
using AutoMapper;
using Domain.Defaults;
using Domain.Exceptions;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class GroupsRepository : IRepository<Group, Guid>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GroupsRepository> _logger;
    private readonly IMapper _mapper;
    
    public GroupsRepository(ApplicationDbContext dbContext, ILogger<GroupsRepository> logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }
    
    private static readonly ReadOnlyDictionary<string, Func<string, Expression<Func<GroupEntity, bool>>>> GroupFilters =
        new(new Dictionary<string, Func<string, Expression<Func<GroupEntity, bool>>>>
        {
            { FilterOptionNames.Group.Name, value => p => p.Name.Contains(value) },
        });

    private static readonly
        ReadOnlyDictionary<OrderModel, Func<IQueryable<GroupEntity>, IQueryable<GroupEntity>>> GroupOrders =
            new(new Dictionary<OrderModel, Func<IQueryable<GroupEntity>, IQueryable<GroupEntity>>>
            {
                {new OrderModel{OrderBy = OrderOptionNames.Group.Name, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(g => g.Name)},
                {new OrderModel{OrderBy = OrderOptionNames.Group.Name, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(g => g.Name)},
            });
    public async Task<IEnumerable<Group>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        try
        {
            var query = _dbContext.Groups.AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, GroupFilters);

            if (orderModel is not null)
                query = orderModel.Order(query, GroupOrders);

            if (pageModel is not null)
                query = pageModel.TakePage(query);
            
            return await query.Select(g => _mapper.Map<Group>(g)).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all groups");
            throw new DataBaseException("Error while receiving groups from database", e);
        }
    }

    public async Task<int> Count(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.Groups.AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, GroupFilters);

            return await query.CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving groups count by filter '{filter}' from database", filterModel);
            throw new DataBaseException("Error while receiving groups count from database", e);
        }
        
    }

    public async Task<Group?> GetById(Guid id)
    {
        try
        {
            var groupEntity = await _dbContext.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            return _mapper.Map<Group>(groupEntity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving group by id '{id}' from database", id);
            throw new DataBaseException("Error while receiving group from database", e);
        }
    }
    
    public async Task<OperationResult> Create(Group group)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(group.Name);
        
        var groupEntity = _mapper.Map<GroupEntity>(group);
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
            _logger.LogWarning(alreadyExistsException, "Group with name '{groupName}' already exist in database", group.Name);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new group '{groupName}' to database", group.Name);
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding group to database", e));
        }

        return new DefaultOperationResult(group);
    }

    public async Task<OperationResult> Update(Group group)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(group.Name);

        try
        {
            var groupEntity = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == group.Id);

            if (groupEntity == null)
                throw new NotFoundException($"Group with id '{group.Id}' was not found");

            if (groupEntity.Name != group.Name)
            {
                var sameName = await _dbContext.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Name == group.Name);
                if (sameName != null)
                    throw new AlreadyExistsException($"Group with name '{group.Name}' already exist in database");
            }

            _mapper.Map(group, groupEntity);
            
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogWarning(notFoundException, "NotFoundException was thrown for {entity} with id '{entityId}'", "Group", group.Id);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogWarning(alreadyExistsException, "Group with name '{groupName}' already exist in database", group.Name);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating group with id '{groupId}'", group.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while updating group in database", e));
        }

        return new DefaultOperationResult(group);
    }

    public async Task<OperationResult> Delete(Guid id)
    {
        Group? group;
        try
        {
            var groupEntity = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == id);

            if (groupEntity == null)
                throw new NotFoundException($"Group with id '{id}' was not found");

            group = _mapper.Map<Group>(groupEntity);
            
            _dbContext.Groups.Remove(groupEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogWarning(notFoundException, "NotFoundException was thrown for {entity} with id '{entityId}'", "Group", id);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while deleting group with id '{groupId}'", id);
            return DefaultOperationResult.FromException(
                new DataBaseException("Error while deleting group from database", e));
        }

        return new DefaultOperationResult(group);
    }
}