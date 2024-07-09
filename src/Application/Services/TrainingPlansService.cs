using System.Collections.ObjectModel;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Models;
using Domain.Models.TrainingPlan;
using OrderOptionNames = Application.Constants.OrderOptionNames.TrainingPlan;

namespace Application.Services;

public class TrainingPlansService : ITrainingPlansService
{
    private readonly ITrainingPlansRepository _trainingPlansRepository;

    public TrainingPlansService(ITrainingPlansRepository trainingPlansRepository)
    {
        _trainingPlansRepository = trainingPlansRepository;
    }


    private readonly ReadOnlyDictionary<OrderModel, Func<IEnumerable<TrainingPlan>, IEnumerable<TrainingPlan>>> _orders =
        new (
            new Dictionary<OrderModel, Func<IEnumerable<TrainingPlan>, IEnumerable<TrainingPlan>>>()
            {
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.Name,
                        OrderOption = Constants.OrderOptionNames.Shared.Ascending
                    },
                    enumerable => enumerable.OrderBy(p => p.Name)
                        .ThenBy(p => p.Author.UserName)
                },
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.Name,
                        OrderOption = Constants.OrderOptionNames.Shared.Descending
                    },
                    enumerable => enumerable.OrderByDescending(p => p.Name)
                        .ThenByDescending(p => p.Author.UserName)
                },
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.Author,
                        OrderOption = Constants.OrderOptionNames.Shared.Ascending
                    },
                    enumerable => enumerable.OrderBy(p => p.Author.UserName)
                        .ThenBy(p => p.Name)
                },
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.Author,
                        OrderOption = Constants.OrderOptionNames.Shared.Descending
                    },
                    enumerable => enumerable.OrderByDescending(p => p.Author.UserName)
                        .ThenByDescending(p => p.Name)
                }
            });

    public async Task<IEnumerable<TrainingPlan>> GetAll(OrderModel? orderModel = null, FilterModel? filterModel = null)
    {
        var trainingPlans = await _trainingPlansRepository.GetAll(filterModel);
        return orderModel?.Order(trainingPlans, _orders) ?? trainingPlans;
    }

    public async Task<TrainingPlan?> GetById(Guid trainingPlanId)
    {
        return await _trainingPlansRepository.GetById(trainingPlanId);
    }

    public async Task<TrainingPlan?> GetByName(string? authorName, string? name)
    {
        return await _trainingPlansRepository.GetByName(authorName, name);
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