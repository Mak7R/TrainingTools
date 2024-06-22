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

public class ExercisesRepository : IExercisesRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ExercisesRepository> _logger;

    public ExercisesRepository(ApplicationDbContext dbContext, ILogger<ExercisesRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<OperationResult> CreateExercise(Exercise? exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentException.ThrowIfNullOrWhiteSpace(exercise.Name);
        ArgumentNullException.ThrowIfNull(exercise.Group);
        
        if (exercise.Id == Guid.Empty) throw new ArgumentException("ExerciseId was empty id");
        
        var exerciseEntity = new ExerciseEntity { Id = exercise.Id, Name = exercise.Name, GroupId = exercise.Group.Id, About = exercise.About };
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
            return new DefaultOperationResult(false, alreadyExistsException, new []{alreadyExistsException.Message});
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new exercise '{exerciseName}' to database", exerciseEntity.Name);
            var ex = new DataBaseException("Error while adding exercise to database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true, exercise);
    }

    public async Task<IEnumerable<Exercise>> GetAll(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.Exercises.Include(exerciseEntity => exerciseEntity.Group).AsNoTracking();

            if ((filterModel?.TryGetValue(FilterOptionNames.Exercise.Group, out var group) ?? false) && Guid.TryParse(group, out var groupId))
            {
                query = query.Where(e => e.Group.Id == groupId);
            }
            if ((filterModel?.TryGetValue(FilterOptionNames.Exercise.Name, out var namePart) ?? false) && !string.IsNullOrWhiteSpace(namePart))
            {
                query = query.Where(e => e.Name.Contains(namePart));
            }
            
            return await query.Select(e => e.ToExercise()).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all exercises");
            throw new DataBaseException("Error while receiving exercises from database", e);
        }
    }

    private async Task<Exercise?> GetBy(Expression<Func<ExerciseEntity, bool>> predicate)
    {
        try
        {
            var exerciseEntity = await _dbContext.Exercises
                .AsNoTracking()
                .Include(e => e.Group)
                .FirstOrDefaultAsync(predicate);
            return exerciseEntity?.ToExercise();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving exercise by expression '{expression}'", predicate);
            throw new DataBaseException("Error while receiving exercise from database", e);
        }
    }
    
    public async Task<Exercise?> GetByName(string? name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return await GetBy(e => e.Name == name);
    }

    public async Task<Exercise?> GetById(Guid id)
    {
        return await GetBy(e => e.Id == id);
    }

    public async Task<OperationResult> UpdateExercise(Exercise? exercise)
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

            exerciseEntity.Name = exercise.Name;
            exerciseEntity.Group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == exercise.Group.Id) ?? 
                                   throw new NotFoundException($"Group with id '{exercise.Group.Id}' was not found");
            exerciseEntity.About = exercise.About;
            
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException e)
        {
            _logger.LogWarning(e, "NotFoundException was thrown for when exercise updating");
            return new DefaultOperationResult(false, e, new[] { e.Message });
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Exercise with name '{exerciseName}' already exist in database", exercise.Name);
            return new DefaultOperationResult(false, alreadyExistsException, new []{alreadyExistsException.Message});
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating exercise with id '{exerciseId}'", exercise.Id);
            var ex = new DataBaseException("Error while updating exercise in database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true, exercise);
    }

    public async Task<OperationResult> DeleteExercise(Guid id)
    {
        Exercise? exercise;
        try
        {
            var exerciseEntity = await _dbContext.Exercises
                .Include(exerciseEntity => exerciseEntity.Group)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exerciseEntity is null)
                throw new NotFoundException($"Exercise with id '{id}' was not found");

            exercise = new Exercise
            {
                Id = exerciseEntity.Id,
                Name = exerciseEntity.Name,
                Group = new Group{Id = exerciseEntity.Group.Id, Name = exerciseEntity.Group.Name}
            };
            
            _dbContext.Exercises.Remove(exerciseEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException e)
        {
            _logger.LogWarning(e, "NotFoundException was thrown for {entity} with id '{entityId}'", "Exercise", id);
            return new DefaultOperationResult(false, e, new[] { e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while deleting exercise with id '{exerciseId}'", id);
            var ex = new DataBaseException("Error while deleting exercise from database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true, exercise);
    }
}