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

public class ExerciseResultsRepository : IRepository<ExerciseResult, (Guid OwnerId, Guid ExerciseId)>
{
    private readonly ApplicationDbContext _dbContext;
   
    private readonly ILogger<ExerciseResultsRepository> _logger;
    private readonly IMapper _mapper;

    public ExerciseResultsRepository(ApplicationDbContext dbContext, ILogger<ExerciseResultsRepository> logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    private static readonly ReadOnlyDictionary<string, Func<string, Expression<Func<ExerciseResultEntity, bool>>>> ExerciseResultsFilters =
        new(new Dictionary<string, Func<string, Expression<Func<ExerciseResultEntity, bool>>>>
        {
            { FilterOptionNames.ExerciseResults.OwnerName, value => r => r.Owner.UserName != null && r.Owner.UserName.Contains(value) },
            { FilterOptionNames.ExerciseResults.ExerciseId, value => Guid.TryParse(value, out var exerciseId) ? r => r.ExerciseId == exerciseId : _ => false},
            { FilterOptionNames.ExerciseResults.FullName, value =>
            {
                var nameParts = value.Split("/");
                if (nameParts.Length > 2)
                    return _ => false;
                if (nameParts.Length == 1)
                    return er => er.Exercise.Group.Name.Contains(nameParts[0]) || er.Exercise.Name.Contains(nameParts[0]);
                if (nameParts.Length == 2)
                    return er => er.Exercise.Group.Name.Contains(nameParts[0]) && er.Exercise.Name.Contains(nameParts[1]);
                return _ => true;
            }},
            { FilterOptionNames.ExerciseResults.FullNameEquals, value =>
                {
                    var nameParts = value.Split("/");
                    if (nameParts.Length == 2)
                        return er => er.Exercise.Group.Name.Equals(nameParts[0]) && er.Exercise.Name.Contains(nameParts[1]);

                    return _ => false;
                }
            },
            {FilterOptionNames.ExerciseResults.OwnerId, value => Guid.TryParse(value, out var ownerId) ? r => r.Owner.Id == ownerId : _ => false },
            {FilterOptionNames.ExerciseResults.OwnerIds, value =>
            {
                var ids = value.Split(FilterOptionNames.Shared.MultiplyFilterValuesSeparator);

                var guids = new List<Guid>();
                foreach (var id in ids)
                    if (Guid.TryParse(id, out var guid))
                        guids.Add(guid);
                
                return r => guids.Contains(r.OwnerId);
            }},
            {FilterOptionNames.ExerciseResults.OwnerNameEquals, value => er => er.Owner.UserName != null && er.Owner.UserName.Equals(value)}
        });

    private static readonly ReadOnlyDictionary<OrderModel, Func<IQueryable<ExerciseResultEntity>, IQueryable<ExerciseResultEntity>>> ExerciseResultsOrders = 
        new(new Dictionary<OrderModel, Func<IQueryable<ExerciseResultEntity>, IQueryable<ExerciseResultEntity>>>()
        {
            {new OrderModel{OrderBy = OrderOptionNames.ExerciseResults.OwnerName, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(e => e.Owner.UserName)},
            {new OrderModel{OrderBy = OrderOptionNames.ExerciseResults.OwnerName, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(e => e.Owner.UserName)},
            {new OrderModel{OrderBy = OrderOptionNames.ExerciseResults.GroupName, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(e => e.Exercise.Group.Name).ThenBy(e => e.Exercise.Name)},
            {new OrderModel{OrderBy = OrderOptionNames.ExerciseResults.GroupName, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(e => e.Exercise.Group.Name).ThenByDescending(e => e.Exercise.Name)}
        });
    
    public async Task<IEnumerable<ExerciseResult>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        try
        {
            var query = _dbContext.ExerciseResults
                .Include(e => e.Exercise)
                .ThenInclude(e =>e.Group)
                .Include(e => e.Owner)
                .AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, ExerciseResultsFilters);

            if (orderModel is not null)
                query = orderModel.Order(query, ExerciseResultsOrders);

            if (pageModel is not null)
                query = pageModel.TakePage(query);
            
            return (await query.ToListAsync()).Select(e => _mapper.Map<ExerciseResult>(e));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all exercise results");
            throw new DataBaseException("Error while receiving exercise results from database", e);
        }
    }

    public async Task<ExerciseResult?> GetById((Guid OwnerId, Guid ExerciseId) id)
    {
        try
        {
            var resultEntity = await _dbContext.ExerciseResults
                .AsNoTracking()
                .Include(e => e.Exercise)
                .ThenInclude(e => e.Group)
                .Include(e => e.Owner)
                .FirstOrDefaultAsync(r => r.OwnerId == id.OwnerId && r.ExerciseId == id.ExerciseId);

            return resultEntity == null ? null : _mapper.Map<ExerciseResult>(resultEntity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving exercise result with ownerId: '{ownerId}' and exerciseId: {exerciseId}", id.OwnerId, id.ExerciseId);
            throw new DataBaseException("Error while receiving exercise results from database", e);
        }
    }

    public async Task<int> Count(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.ExerciseResults.AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, ExerciseResultsFilters);

            return await query.CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving exercise results count by filter '{filter}' from database", filterModel);
            throw new DataBaseException("Error while receiving exercises results count from database", e);
        }
    }
    
    public async Task<OperationResult> Create(ExerciseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var exerciseResultEntity = _mapper.Map<ExerciseResultEntity>(result);

        try
        {
            var existResult = await _dbContext.ExerciseResults.FirstOrDefaultAsync(r =>
                r.ExerciseId == result.Exercise.Id && r.OwnerId == result.Owner.Id);

            if (existResult is not null)
                throw new AlreadyExistsException("This result already exists");

            await _dbContext.ExerciseResults.AddAsync(exerciseResultEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Exercise results already exist with ownerId: '{ownerId}' and exerciseId: {exerciseId}", result.Owner.Id, result.Exercise.Id);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new exercise results '{exerciseResult}' to database", exerciseResultEntity);
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding exercise result to database", e));
        }

        return new DefaultOperationResult(result);
    }

    public async Task<OperationResult> Update(ExerciseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        try
        {
            var resultEntity = await _dbContext.ExerciseResults.FirstOrDefaultAsync(r =>
                r.ExerciseId == result.Exercise.Id && r.OwnerId == result.Owner.Id);

            if (resultEntity is null)
                throw new NotFoundException("Exercise results was not found");

            _mapper.Map(result, resultEntity);

            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogInformation(notFoundException, "Exercise results was not found with ownerId: '{ownerId}' and exerciseId: {exerciseId}", result.Owner.Id, result.Exercise.Id);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating exercise result with ownerId: '{ownerId}' and exerciseId: {exerciseId}", result.Owner.Id, result.Exercise.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while updating exercise result in database", e));
        }

        return new DefaultOperationResult(result);
    }

    public async Task<OperationResult> Delete((Guid OwnerId, Guid ExerciseId) id)
    {
        ExerciseResult? result;
        try
        {
            var resultEntity = await _dbContext.ExerciseResults.FirstOrDefaultAsync(r =>
                r.ExerciseId == id.ExerciseId && r.OwnerId == id.OwnerId);

            if (resultEntity is null)
                throw new NotFoundException("Exercise result was not found");

            result = _mapper.Map<ExerciseResult>(resultEntity);
            
            _dbContext.ExerciseResults.Remove(resultEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogInformation(notFoundException, "Exercise results was not found with ownerId: '{ownerId}' and exerciseId: {exerciseId}", id.OwnerId, id.ExerciseId);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while deleting exercise result with ownerId: '{ownerId}' and exerciseId: {exerciseId}", id.OwnerId, id.ExerciseId);
            return DefaultOperationResult.FromException(new DataBaseException("Error while deleting exercise result from database", e));
        }

        return new DefaultOperationResult(result);
    }
}