using Application.Constants;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Models;

namespace Application.Services;

public class GroupsService : IGroupsService
{
    private readonly IGroupsRepository _groupsRepository;
    
    public GroupsService(IGroupsRepository groupsRepository)
    {
        _groupsRepository = groupsRepository;
    }
    
    public async Task<IEnumerable<Group>> GetAll(OrderModel? orderModel = null, FilterModel? filterModel = null)
    {
        var groups = await _groupsRepository.GetAll(filterModel);
        
        if (orderModel is null || string.IsNullOrWhiteSpace(orderModel.OrderBy)) return groups;
        
        if (orderModel.OrderBy.Equals(OrderOptionNames.Group.Name, StringComparison.CurrentCultureIgnoreCase))
        {
            if (orderModel.Order?.Equals(OrderOptionNames.Shared.Descending, StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                groups = groups.OrderByDescending(g => g.Name);
            }
            else
            {
                groups = groups.OrderBy(g => g.Name);
            }
        }

        return groups.ToList();
    }
    
    public async Task<Group?> GetByName(string? name)
    {
        return await _groupsRepository.GetByName(name);
    }

    public async Task<Group?> GetById(Guid id)
    {
        return await _groupsRepository.GetById(id);
    }
    
    public async Task<OperationResult> CreateGroup(Group? group)
    {
        ArgumentNullException.ThrowIfNull(group);
        group.Id = Guid.NewGuid();
        group.Name = group.Name?.Trim();
        return await _groupsRepository.CreateGroup(group);
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