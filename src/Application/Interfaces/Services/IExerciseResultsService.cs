using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.Services;

public interface IExerciseResultsService
{
    Task<ExerciseResult?> GetById(Guid ownerId, Guid exerciseId);
    Task<int> Count(FilterModel? filterModel);

    Task<IEnumerable<ExerciseResult>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null,
        PageModel? pageModel = null);

    Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId,
        FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null);

    Task<OperationResult> Create(ExerciseResult result);
    Task<OperationResult> Update(ExerciseResult result);
    Task<OperationResult> Delete(Guid ownerId, Guid exerciseId);
}