using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Domain.Models;

namespace Application.Services;

public class GroupsService : IGroupsService
{
    private readonly IRepository<Group, Guid> _groupsRepository;
    
    public GroupsService(IRepository<Group, Guid> groupsRepository)
    {
        _groupsRepository = groupsRepository;
    }
    
    public async Task<IEnumerable<Group>> GetAll(OrderModel? orderModel = null, FilterModel? filterModel = null, PageModel? pageModel = null)
    {
        return await _groupsRepository.GetAll(filterModel, orderModel, pageModel);
    }
    
    public async Task<Group?> GetByName(string? name)
    {
        return (await _groupsRepository.GetAll(new FilterModel{{FilterOptionNames.Group.NameEquals, name}}, null, new PageModel{PageSize = 1})).FirstOrDefault();
    }

    public async Task<Group?> GetById(Guid id)
    {
        return await _groupsRepository.GetById(id);
    }

    public async Task<int> Count(FilterModel? filterModel = null)
    {
        return await _groupsRepository.Count(filterModel);
    }

    public async Task<OperationResult> Create(Group? group)
    {
        ArgumentNullException.ThrowIfNull(group);
        group.Id = Guid.NewGuid();
        group.Name = group.Name?.Trim();
        return await _groupsRepository.Create(group);
    }

    public async Task<OperationResult> Update(Group? group)
    {
        ArgumentNullException.ThrowIfNull(group);
        group.Name = group.Name?.Trim();
        return await _groupsRepository.Update(group);
    }

    public async Task<OperationResult> Delete(Guid id)
    {
        return await _groupsRepository.Delete(id);
    }
}