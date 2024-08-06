using Application.Models.Shared;
using Domain.Models;
using Domain.Models.TrainingPlan;

namespace Application.Interfaces.Services;

public interface ITrainingPlansService
{
    Task<IEnumerable<TrainingPlan>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null,
        PageModel? pageModel = null);

    Task<TrainingPlan?> GetById(Guid trainingPlanId);
    Task<TrainingPlan?> GetByName(string? authorName, string? title);
    Task<int> Count(FilterModel? filterModel = null);

    Task<OperationResult> Create(TrainingPlan plan);
    Task<OperationResult> Update(TrainingPlan updatedPlan);
    Task<OperationResult> Delete(Guid trainingPlanId);
}