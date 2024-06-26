﻿using Application.Models.Shared;
using Domain.Models;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IGroupsRepository
{
    Task<IEnumerable<Group>> GetAll(FilterModel? filterModel = null);
    Task<Group?> GetByName(string? name);
    Task<Group?> GetById(Guid id);
    
    
    Task<OperationResult> CreateGroup(Group? group);
    Task<OperationResult> UpdateGroup(Group? group);
    Task<OperationResult> DeleteGroup(Guid id);
}