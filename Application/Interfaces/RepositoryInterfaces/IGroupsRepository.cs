using Domain.Models;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IGroupsRepository
{
    Task<OperationResult> CreateGroup(Group? group);
    
    Task<IEnumerable<Group>> GetAll();
    Task<Group?> GetByName(string? name);
    Task<Group?> GetById(Guid id);
    
    Task<OperationResult> UpdateGroup(Group? group);
    Task<OperationResult> DeleteGroup(Guid id);
}