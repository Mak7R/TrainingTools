using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Domain.Models;
using Domain.Models.TrainingPlan;

namespace Application.Services;

public class TrainingPlansService : ITrainingPlansService
{
    private readonly IRepository<TrainingPlan, Guid> _trainingPlansRepository;

    public TrainingPlansService(IRepository<TrainingPlan, Guid> trainingPlansRepository)
    {
        _trainingPlansRepository = trainingPlansRepository;
    }

    public async Task<IEnumerable<TrainingPlan>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null,
        PageModel? pageModel = null)
    {
        return await _trainingPlansRepository.GetAll(filterModel, orderModel, pageModel);
    }

    public async Task<TrainingPlan?> GetById(Guid trainingPlanId)
    {
        return await _trainingPlansRepository.GetById(trainingPlanId);
    }

    public async Task<TrainingPlan?> GetByName(string? authorName, string? title)
    {
        return (await _trainingPlansRepository.GetAll(
                new FilterModel
                {
                    { FilterOptionNames.TrainingPlan.AuthorNameEquals, authorName },
                    { FilterOptionNames.TrainingPlan.TitleEquals, title }
                }, null, new PageModel { PageSize = 1 }))
            .FirstOrDefault();
    }

    public async Task<int> Count(FilterModel? filterModel = null)
    {
        return await _trainingPlansRepository.Count(filterModel);
    }

    public async Task<OperationResult> Create(TrainingPlan plan)
    {
        return await _trainingPlansRepository.Create(plan);
    }

    public async Task<OperationResult> Update(TrainingPlan updatedPlan)
    {
        return await _trainingPlansRepository.Update(updatedPlan);
    }

    public async Task<OperationResult> Delete(Guid trainingPlanId)
    {
        return await _trainingPlansRepository.Delete(trainingPlanId);
    }
}