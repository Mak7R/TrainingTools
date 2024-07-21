using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Domain.Defaults;
using Domain.Identity;
using Domain.Models;
using Domain.Models.Friendship;
using Domain.Rules;

namespace Application.Services;

public class ExerciseResultsService : IExerciseResultsService
{
    private readonly IRepository<ExerciseResult, (Guid, Guid)> _exerciseResultsRepository;
    private readonly IRepository<Friendship, (Guid, Guid)> _friendshipsRepository;
    
    public ExerciseResultsService(IRepository<ExerciseResult, (Guid, Guid)> exerciseResultsRepository, IRepository<Friendship, (Guid, Guid)> friendshipsRepository)
    {
        _exerciseResultsRepository = exerciseResultsRepository;
        _friendshipsRepository = friendshipsRepository;
    }
    
    public async Task<ExerciseResult?> GetById(Guid ownerId, Guid exerciseId)
    {
        return await _exerciseResultsRepository.GetById((ownerId, exerciseId));
    }

    public async Task<int> Count(FilterModel? filterModel)
    {
        return await _exerciseResultsRepository.Count(filterModel);
    }
    
    
    public async Task<IEnumerable<ExerciseResult>> GetForUser(string ownerUserName, FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        filterModel ??= new FilterModel();
        filterModel[FilterOptionNames.ExerciseResults.OwnerName] = ownerUserName;
        return await _exerciseResultsRepository.GetAll(filterModel, orderModel, pageModel);
    }

    public async Task<IEnumerable<ExerciseResult>> GetForExercise(string groupName, string exerciseName, FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        filterModel ??= new FilterModel();
        filterModel[FilterOptionNames.ExerciseResults.FullNameEquals] = $"{groupName}/{exerciseName}";
        return await _exerciseResultsRepository.GetAll(filterModel, orderModel, pageModel);
    }
    
    public async Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(ApplicationUser user, string groupName, string exerciseName, FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        // todo slow bad method
        filterModel ??= new FilterModel();
        filterModel[FilterOptionNames.ExerciseResults.FullNameEquals] = $"{groupName}/{exerciseName}";
        var results = await _exerciseResultsRepository.GetAll(filterModel, orderModel);

        var userIds = (await _friendshipsRepository.GetAll(
                new FilterModel
                {
                    { FilterOptionNames.Relationships.Friendship.FriendId, user.Id.ToString() }
                }))
            .Select(f => f.FirstFriend.Id == user.Id ? f.SecondFriend.Id : f.FirstFriend.Id);

        results = results.Where(r => r.Owner.Id == user.Id || userIds.Contains(r.Owner.Id)); // must be executed on repository level

        if (pageModel is not null) // also must be executed on repository level, but now it is not impossible because filtering is on service level ^
            results = pageModel.TakePage(results);
        return results.ToList();
    }
    
    
    public async Task<OperationResult> Create(ExerciseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.ApproachInfos = new[]
        {
            new Approach(0, 0, ""),
            new Approach(0, 0, ""),
            new Approach(0, 0, "")
        };

        return await _exerciseResultsRepository.Create(result);
    }

    public async Task<OperationResult> Update(ExerciseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.ApproachInfos.Any(
                a => a.Count < 0 || 
                a.Weight < 0 || 
                (a.Comment?.Contains(SpecialConstants.DefaultSeparator) ?? false)))
        {
            return DefaultOperationResult.FromException(new InvalidOperationException("Weight and count cannot be less than 0"));
        }
        
        return await _exerciseResultsRepository.Update(result);
    }

    public async Task<OperationResult> Delete(Guid ownerId, Guid exerciseId)
    {
        return await _exerciseResultsRepository.Delete((ownerId, exerciseId));
    }
}