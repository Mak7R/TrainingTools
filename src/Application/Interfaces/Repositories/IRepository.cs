using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IRepository<TModel, TKey>
{
    /// <summary>
    /// Gets all elements from repository which is filtered by filterModel ordered by orderModel and paged by pageModel
    /// </summary>
    /// <param name="filterModel">Represents filters which must be applied to elements</param>
    /// <param name="orderModel">Represents order for ordering elements</param>
    /// <param name="pageModel">Represents page settings for model</param>
    /// <returns></returns>
    Task<IEnumerable<TModel>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null);
    Task<TModel?> GetById(TKey id);
    Task<int> Count(FilterModel? filterModel = null);
    
    Task<OperationResult> Create(TModel group);
    Task<OperationResult> Update(TModel group);
    Task<OperationResult> Delete(TKey id);
}