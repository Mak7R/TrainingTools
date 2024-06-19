using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Domain.Defaults;
using Domain.Models;
using Domain.Rules;

namespace Application.Services;

public class ExerciseResultsService : IExerciseResultsService
{
    private readonly IExerciseResultsRepository _exerciseResultsRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ExerciseResultsService(IExerciseResultsRepository exerciseResultsRepository)
    {
        _exerciseResultsRepository = exerciseResultsRepository;
    }
    
    public async Task<OperationResult> CreateResult(ExerciseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.ApproachInfos = new[]
        {
            new Approach(0, 0, ""),
            new Approach(0, 0, ""),
            new Approach(0, 0, "")
        };

        return await _exerciseResultsRepository.CreateResult(result);
    }

    public async Task<OperationResult> UpdateResult(ExerciseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.ApproachInfos.Select(ai => ai.Weight).Any(w => w < 0) || result.ApproachInfos.Select(ai => ai.Count).Any(c => c < 0))
        {
            var exception = new InvalidOperationException("Weight and count cannot be less than 0");
            return new DefaultOperationResult(false, exception, new[] { exception.Message });
        }
        
        if (result.ApproachInfos.Select(ai => ai.Comment).Any(comment => comment?.Contains(SpecialConstants.DefaultSeparator) ?? false))
        {
            var exception = new InvalidOperationException($"Comments cannot contain symbol {SpecialConstants.DefaultSeparator}");
            return new DefaultOperationResult(false, exception, new[] { exception.Message });
        }
        
        return await _exerciseResultsRepository.UpdateResult(result);
    }

    public async Task<OperationResult> DeleteResult(Guid ownerId, Guid exerciseId)
    {
        return await _exerciseResultsRepository.DeleteResult(ownerId, exerciseId);
    }

    public async Task<ExerciseResult?> Get(Guid ownerId, Guid exerciseId)
    {
        return await _exerciseResultsRepository.Get(ownerId, exerciseId);
    }

    public async Task<IEnumerable<ExerciseResult>> GetForUser(Guid ownerId)
    {
        return await _exerciseResultsRepository.GetForUser(ownerId);
    }

    public async Task<IEnumerable<ExerciseResult>> GetForExercise(Guid exerciseId)
    {
        return await _exerciseResultsRepository.GetForExercise(exerciseId);
    }

    public async Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId)
    {
        return await _exerciseResultsRepository.GetOnlyUserAndFriendsResultForExercise(userId, exerciseId);
    }
}