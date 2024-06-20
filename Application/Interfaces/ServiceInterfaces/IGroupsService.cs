using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IGroupsService
{
    Task<IEnumerable<Group>> GetAll(OrderModel? orderModel = null, FilterModel? filterModel = null);
    
    
    Task<Group?> GetByName(string? name);
    Task<Group?> GetById(Guid id);
    
    
    Task<OperationResult> CreateGroup(Group? group);
    Task<OperationResult> UpdateGroup(Group? group);
    Task<OperationResult> DeleteGroup(Guid id);
}