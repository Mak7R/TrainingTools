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

public class ExercisesRepository : IRepository<Exercise, Guid>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ExercisesRepository> _logger;
    private readonly IMapper _mapper;

    public ExercisesRepository(ApplicationDbContext dbContext, ILogger<ExercisesRepository> logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }
    
    private static readonly ReadOnlyDictionary<string, Func<string, Expression<Func<ExerciseEntity, bool>>>> ExerciseFilters =
        new(new Dictionary<string, Func<string, Expression<Func<ExerciseEntity, bool>>>>
        {
            { FilterOptionNames.Exercise.GroupId, value => Guid.TryParse(value, out var groupId) ? e => e.GroupId == groupId : _ => false },
            { FilterOptionNames.Exercise.Name, value => e => e.Name.Contains(value) }
        });

    private static readonly ReadOnlyDictionary<OrderModel, Func<IQueryable<ExerciseEntity>, IQueryable<ExerciseEntity>>> ExerciseOrders = 
        new(new Dictionary<OrderModel, Func<IQueryable<ExerciseEntity>, IQueryable<ExerciseEntity>>>()
    {
        {new OrderModel{OrderBy = OrderOptionNames.Exercise.Name, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(g => g.Name)},
        {new OrderModel{OrderBy = OrderOptionNames.Exercise.Name, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(g => g.Name)},
        {new OrderModel{OrderBy = OrderOptionNames.Exercise.GroupName, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(e => e.Group.Name).ThenBy(e => e.Name)},
        {new OrderModel{OrderBy = OrderOptionNames.Exercise.GroupName, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(e => e.Group.Name).ThenByDescending(e => e.Name)}
    });

    public async Task<IEnumerable<Exercise>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        try
        {
            var query = _dbContext.Exercises.Include(exerciseEntity => exerciseEntity.Group).AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, ExerciseFilters);

            if (orderModel is not null)
                query = orderModel.Order(query, ExerciseOrders);

            if (pageModel is not null)
                query = pageModel.TakePage(query);
            
            return (await query.ToListAsync()).Select(e => _mapper.Map<Exercise>(e));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all exercises");
            throw new DataBaseException("Error while receiving exercises from database", e);
        }
    }

    public async Task<Exercise?> GetById(Guid id)
    {
        try
        {
            var exercise = await _dbContext.Exercises
                .AsNoTracking()
                .Include(e => e.Group)
                
                .FirstOrDefaultAsync(e => e.Id == id);

            return exercise == null ? null : _mapper.Map<Exercise>(exercise);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving exercise by id '{id}'", id);
            throw new DataBaseException("Error while receiving exercise from database", e);
        }
    }

    public async Task<int> Count(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.Exercises.AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, ExerciseFilters);

            return await query.CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving exercises count by filter '{filter}' from database", filterModel);
            throw new DataBaseException("Error while receiving exercises count from database", e);
        }
    }

    public async Task<OperationResult> Create(Exercise? exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentException.ThrowIfNullOrWhiteSpace(exercise.Name);
        ArgumentNullException.ThrowIfNull(exercise.Group);

        var exerciseEntity = _mapper.Map<ExerciseEntity>(exercise);
        try
        {
            var sameName = await _dbContext.Exercises.AsNoTracking().FirstOrDefaultAsync(e => e.Name == exercise.Name && e.GroupId == exercise.Group.Id);
            if (sameName is not null)
                throw new AlreadyExistsException($"Exercise with name '{exercise.Name}' already exist in database");
            
            await _dbContext.Exercises.AddAsync(exerciseEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Exercise with name '{exerciseName}' already exist in database", exercise.Name);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new exercise to database");
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding exercise to database", e));
        }

        return new DefaultOperationResult(exercise);
    }

    public async Task<OperationResult> Update(Exercise? exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentException.ThrowIfNullOrWhiteSpace(exercise.Name);
        ArgumentNullException.ThrowIfNull(exercise.Group);
        
        try
        {
            var exerciseEntity = await _dbContext.Exercises.FirstOrDefaultAsync(e => e.Id == exercise.Id);

            if (exerciseEntity is null)
                throw new NotFoundException($"Exercise with id '{exercise.Id}' was not found");
            
            var sameName = await _dbContext.Exercises.AsNoTracking().FirstOrDefaultAsync(e => e.Name == exercise.Name && e.GroupId != exercise.Group.Id);
            if (sameName != null)
                throw new AlreadyExistsException($"Exercise with name '{exercise.Name}' already exist in database");

            _mapper.Map(exercise, exerciseEntity);
            
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException e)
        {
            _logger.LogInformation(e, "NotFoundException was thrown for when exercise updating");
            return DefaultOperationResult.FromException(e);
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Exercise with name '{exerciseName}' already exist in database", exercise.Name);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating exercise with id '{exerciseId}'", exercise.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while updating exercise in database", e));
        }

        return new DefaultOperationResult(exercise);
    }

    public async Task<OperationResult> Delete(Guid id)
    {
        Exercise? exercise;
        try
        {
            var exerciseEntity = await _dbContext.Exercises
                .Include(exerciseEntity => exerciseEntity.Group)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exerciseEntity is null)
                throw new NotFoundException($"Exercise with id '{id}' was not found");

            exercise = _mapper.Map<Exercise>(exerciseEntity);
            
            _dbContext.Exercises.Remove(exerciseEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogInformation(notFoundException, "NotFoundException was thrown for {entity} with id '{entityId}'", "Exercise", id);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while deleting exercise with id '{exerciseId}'", id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while deleting exercise from database", e));
        }

        return new DefaultOperationResult(exercise);
    }
}