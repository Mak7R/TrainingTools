using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Domain.Models;

namespace Application.Services;

public class GroupsService : IGroupsService
{
    private readonly IGroupsRepository _groupsRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GroupsService(IGroupsRepository groupsRepository)
    {
        _groupsRepository = groupsRepository;
    }

    public async Task<OperationResult> CreateGroup(Group? group)
    {
        ArgumentNullException.ThrowIfNull(group);
        group.Id = Guid.NewGuid();
        group.Name = group.Name?.Trim();
        return await _groupsRepository.CreateGroup(group);
    }

    public async Task<IEnumerable<Group>> GetAll()
    {
        return await _groupsRepository.GetAll();
    }
    
    public async Task<Group?> GetByName(string? name)
    {
        return await _groupsRepository.GetByName(name);
    }

    public async Task<Group?> GetById(Guid id)
    {
        return await _groupsRepository.GetById(id);
    }

    public async Task<OperationResult> UpdateGroup(Group? group)
    {
        ArgumentNullException.ThrowIfNull(group);
        group.Name = group.Name?.Trim();
        return await _groupsRepository.UpdateGroup(group);
    }

    public async Task<OperationResult> DeleteGroup(Guid id)
    {
        return await _groupsRepository.DeleteGroup(id);
    }
}