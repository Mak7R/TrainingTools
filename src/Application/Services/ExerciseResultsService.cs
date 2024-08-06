using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Domain.Defaults;
using Domain.Models;
using Domain.Models.Friendship;
using Domain.Rules;

namespace Application.Services;

public class ExerciseResultsService : IExerciseResultsService
{
    private readonly IRepository<ExerciseResult, (Guid, Guid)> _exerciseResultsRepository;
    private readonly IRepository<Friendship, (Guid, Guid)> _friendshipsRepository;

    public ExerciseResultsService(IRepository<ExerciseResult, (Guid, Guid)> exerciseResultsRepository,
        IRepository<Friendship, (Guid, Guid)> friendshipsRepository)
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


    public async Task<IEnumerable<ExerciseResult>> GetAll(FilterModel? filterModel = null,
        OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        return await _exerciseResultsRepository.GetAll(filterModel, orderModel, pageModel);
    }

    public async Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId,
        FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        filterModel ??= new FilterModel();
        filterModel[FilterOptionNames.ExerciseResults.ExerciseId] = exerciseId.ToString();
        var userIds = (await _friendshipsRepository.GetAll(
                new FilterModel
                {
                    { FilterOptionNames.Relationships.Friendship.FriendId, userId.ToString() }
                }))
            .Select(f => f.FirstFriend.Id == userId ? f.SecondFriend.Id : f.FirstFriend.Id).ToList();

        userIds.Add(userId);
        filterModel[FilterOptionNames.ExerciseResults.OwnerIds] =
            string.Join(FilterOptionNames.Shared.MultiplyFilterValuesSeparator, userIds);

        return await _exerciseResultsRepository.GetAll(filterModel, orderModel, pageModel);
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
            return DefaultOperationResult.FromException(
                new InvalidOperationException("Weight and count cannot be less than 0"));

        return await _exerciseResultsRepository.Update(result);
    }

    public async Task<OperationResult> Delete(Guid ownerId, Guid exerciseId)
    {
        return await _exerciseResultsRepository.Delete((ownerId, exerciseId));
    }
}