using System.Linq.Expressions;
using Application.Interfaces.RepositoryInterfaces;
using Domain.Defaults;
using Domain.Exceptions;
using Domain.Models;
using Domain.Rules;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class ExerciseResultsRepository : IExerciseResultsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFriendsRepository _friendsRepository;
    private readonly ILogger<ExerciseResultsRepository> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ExerciseResultsRepository(ApplicationDbContext dbContext, IFriendsRepository friendsRepository, ILogger<ExerciseResultsRepository> logger)
    {
        _dbContext = dbContext;
        _friendsRepository = friendsRepository;
        _logger = logger;
    }
    
    public async Task<OperationResult> CreateResult(ExerciseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        
        var exerciseResultEntity = new ExerciseResultEntity
        {
            OwnerId = result.Owner.Id,
            ExerciseId = result.Exercise.Id,
            Weights = string.Join(SpecialConstants.DefaultSeparator, result.ApproachInfos.Select(ai => Math.Round(ai.Weight, 3))),
            Counts = string.Join(SpecialConstants.DefaultSeparator, result.ApproachInfos.Select(ai => ai.Count)),
            Comments = string.Join(SpecialConstants.DefaultSeparator, result.ApproachInfos.Select(ai => ai.Comment))
        };

        try
        {
            await _dbContext.ExerciseResults.AddAsync(exerciseResultEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new exercise result '{exerciseResult}' to database", exerciseResultEntity);
            var ex = new DataBaseException("Error while adding exercise result to database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true, result);
    }

    public async Task<OperationResult> UpdateResult(ExerciseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        ExerciseResultEntity? resultEntity;
        try
        {
            resultEntity = await _dbContext.ExerciseResults.FirstOrDefaultAsync(r =>
                r.ExerciseId == result.Exercise.Id && r.OwnerId == result.Owner.Id);
            
            if (resultEntity is null)
            {
                var ex = new NotFoundException("Exercise result was not found");
                return new DefaultOperationResult(false, ex, new []{ ex.Message });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating exercise result with ownerId: '{ownerId}' and exerciseId: {exerciseId}", result.Owner.Id, result.Exercise.Id);
            var ex = new DataBaseException("Error while updating exercise result in database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }
        
        try
        {
            resultEntity.Weights = string.Join(SpecialConstants.DefaultSeparator, result.ApproachInfos.Select(ai => Math.Round(ai.Weight, 3)));
            resultEntity.Counts = string.Join(SpecialConstants.DefaultSeparator, result.ApproachInfos.Select(ai => ai.Count));
            resultEntity.Comments = string.Join(SpecialConstants.DefaultSeparator, result.ApproachInfos.Select(ai => ai.Comment));
                
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating exercise result with ownerId: '{ownerId}' and exerciseId: {exerciseId}", result.Owner.Id, result.Exercise.Id);
            var ex = new DataBaseException("Error while updating exercise result in database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true, result);
    }

    public async Task<OperationResult> DeleteResult(Guid ownerId, Guid exerciseId)
    {
        ExerciseResultEntity? resultEntity;
        try
        {
            resultEntity = await _dbContext.ExerciseResults.FirstOrDefaultAsync(r =>
                r.ExerciseId == exerciseId && r.OwnerId == ownerId);
            
            if (resultEntity is null)
            {
                var ex = new NotFoundException("Exercise result was not found");
                return new DefaultOperationResult(false, ex, new []{ ex.Message });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while deleting exercise result with ownerId: '{ownerId}' and exerciseId: {exerciseId}", ownerId, exerciseId);
            var ex = new DataBaseException("Error while deleting exercise result from database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }
        
        try
        {
            _dbContext.ExerciseResults.Remove(resultEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while deleting exercise result with ownerId: '{ownerId}' and exerciseId: {exerciseId}", ownerId, exerciseId);
            var ex = new DataBaseException("Error while deleting exercise result from database", e);
            return new DefaultOperationResult(false, ex, new[] { ex.Message });
        }

        return new DefaultOperationResult(true);
    }

    public async Task<ExerciseResult?> Get(Guid ownerId, Guid exerciseId)
    {
        try
        {
            var resultEntity = await _dbContext.ExerciseResults
                .Include(exerciseResultEntity => exerciseResultEntity.Exercise)
                .ThenInclude(exerciseEntity => exerciseEntity.Group)
                .FirstOrDefaultAsync(r =>
                r.OwnerId == ownerId && r.ExerciseId == exerciseId);

            return resultEntity?.ToExerciseResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving exercise result with ownerId: '{ownerId}' and exerciseId: {exerciseId}", ownerId, exerciseId);
            throw new DataBaseException("Error while receiving exercise result from database", e);
        }
    }

    private async Task<IEnumerable<ExerciseResult>> GetFor(Expression<Func<ExerciseResultEntity, bool>> predicate)
    {
        try
        {
            var results = await _dbContext.ExerciseResults
                .Include(exerciseResultEntity => exerciseResultEntity.Exercise)
                .ThenInclude(exerciseEntity => exerciseEntity.Group)
                .Where(predicate).ToListAsync();

            return results.Select(r => r.ToExerciseResult());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving exercise results with expression: '{expression}'", predicate);
            throw new DataBaseException("Error while receiving exercise results from database", e);
        }
    }
    
    public async Task<IEnumerable<ExerciseResult>> GetForUser(Guid ownerId)
    {
        return await GetFor(r => r.OwnerId == ownerId);
    }

    public async Task<IEnumerable<ExerciseResult>> GetForExercise(Guid exerciseId)
    {
        return await GetFor(r => r.ExerciseId == exerciseId);
    }

    public async Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId)
    {
        var userIds = (await _friendsRepository.GetFriendsFor(userId)).Select(u => u.Id);

        return await GetFor(r => r.ExerciseId == exerciseId && (r.OwnerId == userId || userIds.Contains(r.OwnerId)) );
    }
}