using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IExerciseResultsService
{
    Task<ExerciseResult?> Get(Guid ownerId, Guid exerciseId);
    Task<IEnumerable<ExerciseResult>> GetForUser(Guid ownerId, OrderModel? orderModel = null, FilterModel? filterModel = null);
    Task<IEnumerable<ExerciseResult>> GetForExercise(Guid exerciseId, OrderModel? orderModel = null, FilterModel? filterModel = null);
    Task<IEnumerable<ExerciseResult>> GetOnlyUserAndFriendsResultForExercise(Guid userId, Guid exerciseId, OrderModel? orderModel = null, FilterModel? filterModel = null);
    
    Task<OperationResult> Create(ExerciseResult result);
    Task<OperationResult> Update(ExerciseResult result);
    Task<OperationResult> Delete(Guid ownerId, Guid exerciseId);
}