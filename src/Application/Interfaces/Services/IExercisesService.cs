using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.Services;

public interface IExercisesService
{
    Task<IEnumerable<Exercise>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null);
    Task<Exercise?> GetByName(string? name);
    Task<Exercise?> GetById(Guid id);
    Task<int> Count(FilterModel? filterModel = null);
    
    Task<OperationResult> Create(Exercise? exercise);
    Task<OperationResult> Update(Exercise? group);
    Task<OperationResult> Delete(Guid id);
}