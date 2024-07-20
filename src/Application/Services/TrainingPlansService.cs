using System.Collections.ObjectModel;
using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Models;
using Domain.Models.TrainingPlan;
using OrderOptionNames = Application.Constants.OrderOptionNames.TrainingPlan;

namespace Application.Services;

public class TrainingPlansService : ITrainingPlansService
{
    private readonly IRepository<TrainingPlan, Guid> _trainingPlansRepository;

    public TrainingPlansService(IRepository<TrainingPlan, Guid> trainingPlansRepository)
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
                        OrderBy = OrderOptionNames.Title,
                        OrderOption = Constants.OrderOptionNames.Shared.Ascending
                    },
                    enumerable => enumerable.OrderBy(p => p.Title)
                        .ThenBy(p => p.Author.UserName)
                },
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.Title,
                        OrderOption = Constants.OrderOptionNames.Shared.Descending
                    },
                    enumerable => enumerable.OrderByDescending(p => p.Title)
                        .ThenByDescending(p => p.Author.UserName)
                },
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.AuthorName,
                        OrderOption = Constants.OrderOptionNames.Shared.Ascending
                    },
                    enumerable => enumerable.OrderBy(p => p.Author.UserName)
                        .ThenBy(p => p.Title)
                },
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.AuthorName,
                        OrderOption = Constants.OrderOptionNames.Shared.Descending
                    },
                    enumerable => enumerable.OrderByDescending(p => p.Author.UserName)
                        .ThenByDescending(p => p.Title)
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

    public async Task<TrainingPlan?> GetByName(string? authorName, string? title)
    {
        return (await _trainingPlansRepository.GetAll(
                new FilterModel
                {
                    {FilterOptionNames.TrainingPlan.AuthorNameEquals, authorName},
                    {FilterOptionNames.TrainingPlan.TitleEquals, title}
                }, null, new PageModel{PageSize = 1}))
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