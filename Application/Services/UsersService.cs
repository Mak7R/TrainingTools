using System.Security.Claims;
using Application.Dtos;
using Application.Enums;
using Application.Identity;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Domain.Defaults;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UsersService : IUsersService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFriendsRepository _friendsRepository;
    private readonly ILogger<UsersService> _logger;

    public UsersService(UserManager<ApplicationUser> userManager, IFriendsRepository friendsRepository, ILogger<UsersService> logger)
    {
        _userManager = userManager;
        _friendsRepository = friendsRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserInfo>> GetAllUsers(ClaimsPrincipal? currentUserClaimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);

        var user = await _userManager.GetUserAsync(currentUserClaimsPrincipal);
        if (user == null) throw new ArgumentException("User was not found", nameof(currentUserClaimsPrincipal));
        
        var roles = await _userManager.GetRolesAsync(user);
        var users = 
            roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin))
                ? _userManager.Users.Where(u => u.Id != user.Id).ToList()
                : _userManager.Users.Where(u => u.Id != user.Id && u.IsPublic).ToList();
        
        var friends = (await _friendsRepository.GetFriendsFor(user.Id)).ToDictionary(f => f.Id);
        var inviters = (await _friendsRepository.GetInviters(user.Id)).ToDictionary(f => f.Id);
        var invited = (await _friendsRepository.GetInvitedUsersBy(user.Id)).ToDictionary(f => f.Id);
        
        var userInfos = new List<UserInfo>();

        foreach (var applicationUser in users)
            userInfos.Add(await CreateUserInfo(friends, inviters, invited, applicationUser));
        
        return userInfos;
    }

    public async Task<UserInfo?> GetById(ClaimsPrincipal? currentUserClaimsPrincipal, Guid id)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);

        var user = await _userManager.GetUserAsync(currentUserClaimsPrincipal);
        if (user == null) throw new ArgumentException("User was not found", nameof(currentUserClaimsPrincipal));

        if (user.Id == id) throw new ArgumentException("User has same id as searchable user", nameof(id));
        
        var roles = await _userManager.GetRolesAsync(user);
        var foundUser = 
            roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin))
                ? _userManager.Users.FirstOrDefault(u => u.Id == id)
                : _userManager.Users.FirstOrDefault(u => u.Id == id && u.IsPublic);
        
        return foundUser == null ? null : await CreateUserInfo(user.Id, foundUser);
    }

    public async Task<UserInfo?> GetByName(ClaimsPrincipal? currentUserClaimsPrincipal, string? userName)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        
        var user = await _userManager.GetUserAsync(currentUserClaimsPrincipal);
        if (user == null) throw new ArgumentException("User was not found", nameof(currentUserClaimsPrincipal));

        if (user.UserName == userName) throw new ArgumentException("User has same id as searchable user", nameof(userName));
        
        var roles = await _userManager.GetRolesAsync(user);
        var foundUser = 
            roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin))
                ? _userManager.Users.FirstOrDefault(u => u.UserName == userName)
                : _userManager.Users.FirstOrDefault(u => u.UserName == userName && u.IsPublic);

        return foundUser == null ? null : await CreateUserInfo(user.Id, foundUser);
    }

    public async Task<UserInfo?> GetByEmail(ClaimsPrincipal? currentUserClaimsPrincipal, string? email)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        
        var user = await _userManager.GetUserAsync(currentUserClaimsPrincipal);
        if (user == null) throw new ArgumentException("User was not found", nameof(currentUserClaimsPrincipal));

        if (user.Email == email) throw new ArgumentException("User has same id as searchable user", nameof(email));
        
        var roles = await _userManager.GetRolesAsync(user);
        var foundUser = 
            roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin))
                ? _userManager.Users.FirstOrDefault(u => u.Email == email)
                : _userManager.Users.FirstOrDefault(u => u.Email == email && u.IsPublic);

        return foundUser == null ? null : await CreateUserInfo(user.Id, foundUser);
    }

    public async Task<OperationResult> CreateUser(ClaimsPrincipal? currentUserClaimsPrincipal, CreateUserDto createUserDto)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        ArgumentNullException.ThrowIfNull(createUserDto);
        
        var user = await _userManager.GetUserAsync(currentUserClaimsPrincipal);
        if (user == null) throw new ArgumentException("User was not found", nameof(currentUserClaimsPrincipal));
        
        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains(nameof(Role.Admin)) || roles.Contains(nameof(Role.Root)))
        {
            var applicationUser = new ApplicationUser
            {
                UserName = createUserDto.Username, 
                Email = createUserDto.Email,
                PhoneNumber = createUserDto.Phone,
                IsPublic = createUserDto.IsPublic,
            };
            var result = await _userManager.CreateAsync(applicationUser, createUserDto.Password);

            if (result.Succeeded)
            {
                var addToRoleResult = await _userManager.AddToRoleAsync(applicationUser, nameof(Role.User));

                if (!addToRoleResult.Succeeded)
                {
                    const string message = "Add to role user was not succeeded";
                    _logger.LogError(message);
                    throw new Exception(message);
                }
                
                if (roles.Contains(nameof(Role.Root)) && createUserDto.IsAdmin)
                {
                    await _userManager.AddToRoleAsync(applicationUser, nameof(Role.Admin));
                }

                return new DefaultOperationResult(true);
            }
            else
            {
                return new DefaultOperationResult(false, errors: result.Errors.Select(err => err.Description));
            }
        }
        
        return new DefaultOperationResult(false, new OperationNotAllowedException("User is not admin or root"),
            new[] { "User is not admin or root" });
    }

    public Task<OperationResult> UpdateUser(ClaimsPrincipal? currentUserClaimsPrincipal, UpdateUserDto updateUserDto)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult> DeleteUser(ClaimsPrincipal? currentUserClaimsPrincipal, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        
        var currentUser = await _userManager.GetUserAsync(currentUserClaimsPrincipal);
        if (currentUser == null) throw new ArgumentException("User was not found", nameof(currentUserClaimsPrincipal));
        
        if (currentUserClaimsPrincipal.IsInRole(nameof(Role.Admin)) ||
            currentUserClaimsPrincipal.IsInRole(nameof(Role.Root)))
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                var message = $"User with id '{userId}' was not found";
                return new DefaultOperationResult(false, 
                    new NotFoundException(message),
                    new[] { message });
            }

            if (await _userManager.IsInRoleAsync(user, nameof(Role.Root)))
            {
                return new DefaultOperationResult(false, new OperationNotAllowedException("Impossible to delete root user"),
                    new[] { "Impossible to delete root user" });
            }

            var isUserAdmin = await _userManager.IsInRoleAsync(user, nameof(Role.Admin));
            if (!isUserAdmin || (isUserAdmin && await _userManager.IsInRoleAsync(currentUser, nameof(Role.Root))))
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return new DefaultOperationResult(true);
                }
                else
                {
                    return new DefaultOperationResult(false, errors: result.Errors.Select(err => err.Description));
                }
            }
            
            return new DefaultOperationResult(false, new OperationNotAllowedException("Only Root can delete admins"),
                new[] { "Only Root can delete admins" });
        }
        return new DefaultOperationResult(false, new OperationNotAllowedException("User is not admin or root"),
            new[] { "User is not admin or root" });
    }
    
    private async Task<UserInfo> CreateUserInfo(Dictionary<Guid, ApplicationUser> friends,
        Dictionary<Guid, ApplicationUser> inviters, Dictionary<Guid, ApplicationUser> invited, ApplicationUser user)
    {
        var relationshipState = 
            friends.ContainsKey(user.Id) ? RelationshipState.Friends
            : invited.ContainsKey(user.Id) ? RelationshipState.Invited
            : inviters.ContainsKey(user.Id) ? RelationshipState.CanBeAccepted
            : RelationshipState.None;

        var userRoles = await _userManager.GetRolesAsync(user);
        
        return new UserInfo(user, relationshipState, userRoles);
    }
    
    private async Task<UserInfo> CreateUserInfo(Guid currentUserId, ApplicationUser user)
    {
        var friends = (await _friendsRepository.GetFriendsFor(currentUserId)).ToDictionary(f => f.Id);
        var inviters = (await _friendsRepository.GetInviters(currentUserId)).ToDictionary(f => f.Id);
        var invited = (await _friendsRepository.GetInvitedUsersBy(currentUserId)).ToDictionary(f => f.Id);

        return await CreateUserInfo(friends, inviters, invited, user);
    }
}