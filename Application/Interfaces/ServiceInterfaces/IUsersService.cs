using System.Security.Claims;
using Application.Dtos;
using Domain.Identity;
using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IUsersService
{
    Task<IEnumerable<UserInfo>> GetAllUsers(ApplicationUser? currentUser);
    Task<Stream> GetAllUsersAsCsv();

    Task<UserInfo?> GetById(ApplicationUser? currentUser, Guid id);
    Task<UserInfo?> GetByName(ApplicationUser? currentUser, string? userName);
    Task<UserInfo?> GetByEmail(ApplicationUser? currentUser, string? email);


    Task<OperationResult> CreateUser(ApplicationUser? currentUser, CreateUserDto createUserDto);
    Task<OperationResult> UpdateUser(ApplicationUser? currentUser, UpdateUserDto updateUserDto);
    Task<OperationResult> DeleteUser(ApplicationUser? currentUser, Guid userId);
}