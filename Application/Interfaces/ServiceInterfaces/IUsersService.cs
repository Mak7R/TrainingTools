using System.Security.Claims;
using Application.Dtos;
using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IUsersService
{
    public Task<IEnumerable<UserInfo>> GetAllUsers(ClaimsPrincipal? currentUserClaimsPrincipal);

    public Task<UserInfo?> GetById(ClaimsPrincipal? currentUserClaimsPrincipal, Guid id);
    public Task<UserInfo?> GetByName(ClaimsPrincipal? currentUserClaimsPrincipal, string? userName);
    public Task<UserInfo?> GetByEmail(ClaimsPrincipal? currentUserClaimsPrincipal, string? email);


    public Task<OperationResult> CreateUser(ClaimsPrincipal? currentUserClaimsPrincipal, CreateUserDto createUserDto);
    public Task<OperationResult> UpdateUser(ClaimsPrincipal? currentUserClaimsPrincipal, UpdateUserDto updateUserDto);
    public Task<OperationResult> DeleteUser(ClaimsPrincipal? currentUserClaimsPrincipal, Guid userId);
}