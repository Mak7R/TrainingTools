using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IGroupsRepository
{
    Task<IEnumerable<Group>> GetAll(FilterModel? filterModel = null);
    Task<Group?> GetByName(string name);
    Task<Group?> GetById(Guid id);
    
    
    Task<OperationResult> Create(Group group);
    Task<OperationResult> Update(Group group);
    Task<OperationResult> Delete(Guid id);
}