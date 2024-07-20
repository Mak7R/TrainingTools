using Application.Models.Shared;
using Domain.Models;
using Domain.Models.TrainingPlan;

namespace Application.Interfaces.Repositories;

public interface ITrainingPlansRepository
{
    Task<IEnumerable<TrainingPlan>> GetAll(FilterModel? filterModel = null);
    Task<TrainingPlan?> GetById(Guid trainingPlanId);
    Task<TrainingPlan?> GetByName(string? authorName, string? name);
    Task<OperationResult> Create(TrainingPlan trainingPlan);
    Task<OperationResult> Update(TrainingPlan updatedPlan);
    Task<OperationResult> Delete(Guid trainingPlanId);
}