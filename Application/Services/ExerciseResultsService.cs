using Application.Constants;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
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
            return DefaultOperationResult.FromException(new InvalidOperationException("Weight and count cannot be less than 0"));
        }
        
        if (result.ApproachInfos.Select(ai => ai.Comment).Any(comment => comment?.Contains(SpecialConstants.DefaultSeparator) ?? false))
        {
            return DefaultOperationResult.FromException(new InvalidOperationException($"Comments cannot contain symbol {SpecialConstants.DefaultSeparator}"));
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

    public async Task<IEnumerable<ExerciseResult>> GetForUser(Guid ownerId, OrderModel? orderModel = null, FilterModel? filterModel = null)
    {
        var results = await _exerciseResultsRepository.GetForUser(ownerId, filterModel);
        
        if (orderModel is null || string.IsNullOrWhiteSpace(orderModel.OrderBy)) return results;
        
        if (orderModel.OrderBy.Equals(OrderOptionNames.ExerciseResults.ForUser.GroupName, StringComparison.CurrentCultureIgnoreCase))
        {
            if (orderModel.Order?.Equals(OrderOptionNames.Shared.Descending, StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                results = results
                    .OrderByDescending(e => e.Exercise.Group.Name)
                    .ThenByDescending(e => e.Exercise.Name);
            }
            else
            {
                results = results
                    .OrderBy(e => e.Exercise.Group.Name)
                    .ThenBy(e => e.Exercise.Name);
            }
        }

        return results;
    }

    public async Task<IEnumerable<ExerciseResult>> GetForExercise(Guid exerciseId, OrderModel? orderModel = null, FilterModel? filterModel = null)
    {
        var results = await _exerciseResultsRepository.GetForExercise(exerciseId, filterModel);
        if (orderModel is null || string.IsNullOrWhiteSpace(orderModel.OrderBy)) return results;
        
        if (orderModel.OrderBy.Equals(OrderOptionNames.ExerciseResults.ForExercise.OwnerName, StringComparison.CurrentCultureIgnoreCase))
        {
            if (orderModel.Order?.Equals(OrderOptionNames.Shared.Descending, StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                results = results.OrderByDescending(e => e.Owner.UserName);
            }
            else
            {
                results = results.OrderBy(e => e.Owner.UserName);
            }
        }

        return results;
    }

    public async Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId, OrderModel? orderModel = null, FilterModel? filterModel = null)
    {
        var results = await _exerciseResultsRepository.GetOnlyUserAndFriendsResultForExercise(userId, exerciseId, filterModel);

        if (orderModel is null || string.IsNullOrWhiteSpace(orderModel.OrderBy)) return results;
        
        if (orderModel.OrderBy.Equals(OrderOptionNames.ExerciseResults.ForExercise.OwnerName, StringComparison.CurrentCultureIgnoreCase))
        {
            if (orderModel.Order?.Equals(OrderOptionNames.Shared.Descending, StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                results = results.OrderByDescending(e => e.Owner.UserName);
            }
            else
            {
                results = results.OrderBy(e => e.Owner.UserName);
            }
        }
        
        return results;
    }
}