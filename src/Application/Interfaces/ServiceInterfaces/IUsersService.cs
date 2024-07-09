using System.Security.Claims;
using Application.Dtos;
using Application.Models.Shared;
using Domain.Identity;
using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IUsersService
{
    Task<IEnumerable<UserInfo>> GetAll(ApplicationUser? currentUser, OrderModel? orderModel = null, FilterModel? filterModel = null);
    Task<Stream> GetAllUsersAsCsv();

    Task<UserInfo?> GetById(ApplicationUser? currentUser, Guid id);
    Task<UserInfo?> GetByName(ApplicationUser? currentUser, string? userName);
    Task<UserInfo?> GetByEmail(ApplicationUser? currentUser, string? email);


    Task<OperationResult> Create(ApplicationUser? currentUser, CreateUserDto createUserDto);
    Task<OperationResult> Update(ApplicationUser? currentUser, UpdateUserDto updateUserDto);
    Task<OperationResult> Delete(ApplicationUser? currentUser, string userName);
}