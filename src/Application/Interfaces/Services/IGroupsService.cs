using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.Services;

public interface IGroupsService
{
    Task<IEnumerable<Group>> GetAll(OrderModel? orderModel = null, FilterModel? filterModel = null, PageModel? pageModel = null);
    
    Task<Group?> GetByName(string? name);
    Task<Group?> GetById(Guid id);

    Task<int> Count(FilterModel? filterModel = null);
    
    
    Task<OperationResult> Create(Group? group);
    Task<OperationResult> Update(Group? group);
    Task<OperationResult> Delete(Guid id);
}