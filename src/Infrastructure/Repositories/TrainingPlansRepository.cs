using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Application.Constants;
using Application.Interfaces.RepositoryInterfaces;
using Application.Models.Shared;
using Domain.Defaults;
using Domain.Exceptions;
using Domain.Models;
using Domain.Models.TrainingPlan;
using Infrastructure.Data;
using Infrastructure.Entities.TrainingPlanEntities;
using Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Repositories;

public class TrainingPlansRepository : ITrainingPlansRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TrainingPlansRepository> _logger;

    public TrainingPlansRepository(ApplicationDbContext dbContext, ILogger<TrainingPlansRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    private static readonly ReadOnlyDictionary<string, Func<string, Expression<Func<TrainingPlanEntity, bool>>>> TrainingPlanFilters =
        new(new Dictionary<string, Func<string, Expression<Func<TrainingPlanEntity, bool>>>>
        {
            { FilterOptionNames.TrainingPlan.Name, value => p => p.Name.Contains(value) },
            { FilterOptionNames.TrainingPlan.NameEquals, value => p => p.Name == value},
            {
                FilterOptionNames.TrainingPlan.Author,
                value => p => p.Author.UserName != null && p.Author.UserName.Contains(value)
            },
            { FilterOptionNames.TrainingPlan.AuthorId, value => Guid.TryParse(value, out var authorId) ? _ => false : p => p.AuthorId == authorId },
            { FilterOptionNames.TrainingPlan.PublicOnly, value => value == "true" ? p => true : p => p.IsPublic }
        });
    public async Task<IEnumerable<TrainingPlan>> GetAll(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.TrainingPlans
                .Include(plan => plan.Author)
                .Include(plan => plan.TrainingPlanBlocks)
                .ThenInclude(block => block.TrainingPlanBlockEntries)
                .AsNoTracking();

            if (filterModel is not null)
                query = filterModel.FilterBy(query, TrainingPlanFilters);
            
            return await query.Select(entity => entity.ToTrainingPlan()).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all training plans");
            throw new DataBaseException("Error while receiving training plans from database", e);
        }
    }
    
    private async Task<TrainingPlan?> GetBy(Expression<Func<TrainingPlanEntity, bool>> predicate)
    {
        try
        {
            var trainingPlanEntity = await _dbContext.TrainingPlans
                .Include(plan => plan.Author)
                .Include(plan => plan.TrainingPlanBlocks)
                .ThenInclude(block => block.TrainingPlanBlockEntries)
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
            return trainingPlanEntity?.ToTrainingPlan();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving training plan with expression '{expression}'", predicate);
            throw new DataBaseException("Error while receiving training plan from database", e);
        }
    }

    public Task<TrainingPlan?> GetById(Guid trainingPlanId)
    {
        return GetBy(plan => plan.Id == trainingPlanId);
    }

    public Task<TrainingPlan?> GetByName(string? authorName, string? name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return GetBy(plan => plan.Name == name && plan.Author.UserName == authorName);
    }

    public async Task<OperationResult> Create(TrainingPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        
        var planEntity = plan.ToTrainingPlanEntity(true);

        try
        {
            var existsPlan = await _dbContext.TrainingPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == plan.Name && e.Author.Id == plan.Author.Id);

            if (existsPlan is not null)
                throw new AlreadyExistsException("Plan with this name already exists");

            await _dbContext.TrainingPlans.AddAsync(planEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Training plan with name '{trainingPlanName}' already exist in database", plan.Name);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new training plan '{plan}' to database", plan.Name);
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding training plan to database", e));
        }

        return new DefaultOperationResult(true, plan);
    }

    public async Task<OperationResult> Update(TrainingPlan updatedPlan)
    {
        ArgumentNullException.ThrowIfNull(updatedPlan);
        
        var updatedPlanEntity = updatedPlan.ToTrainingPlanEntity(true);

        try
        {
            var existEntity = await _dbContext.TrainingPlans
                .Include(p => p.TrainingPlanBlocks)
                .ThenInclude(b => b.TrainingPlanBlockEntries)
                .FirstOrDefaultAsync(e => e.Id == updatedPlan.Id);

            if (existEntity is null)
                throw new NotFoundException("Training plan was not found");
            
            var planWithSameName = await _dbContext.TrainingPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id != existEntity.Id && p.AuthorId == updatedPlan.Author.Id && p.Name == updatedPlan.Name);

            if (planWithSameName is not null)
                throw new AlreadyExistsException("Plan with this name already exists");

            existEntity.Name = updatedPlan.Name;
            existEntity.IsPublic = updatedPlan.IsPublic;
            existEntity.TrainingPlanBlocks = updatedPlanEntity.TrainingPlanBlocks;

            await _dbContext.SaveChangesAsync();

        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogWarning(notFoundException, "Training plan with id '{trainingPlanId}' was not found", updatedPlan.Id);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Training plan with name '{trainingPlan}' already exist in database", updatedPlan.Name);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new training plan '{trainingPlanId}' to database", updatedPlan.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding group to database", e));
        }

        return new DefaultOperationResult(true, updatedPlan);
    }

    public async Task<OperationResult> Delete(Guid trainingPlanId)
    {
        try
        {
            var trainingPlan = await _dbContext.TrainingPlans.FirstOrDefaultAsync(plan => plan.Id == trainingPlanId);

            if (trainingPlan is null)
                throw new NotFoundException("Training plan was not found");

            _dbContext.TrainingPlans.Remove(trainingPlan);
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogInformation(notFoundException, "Training plan with id '{trainingPlanId} was not found'",
                trainingPlanId);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (Exception)
        {
            _logger.LogWarning("Error when deleting training plan with id '{trainingPlanId}'", trainingPlanId);
            return DefaultOperationResult.FromException(new DataBaseException("Error while deleting training plan"));
        }

        return new DefaultOperationResult(true);
    }
}