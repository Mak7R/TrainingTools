using System.Linq.Expressions;
using System.Security.Claims;
using Application.Dtos;
using Application.Enums;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Domain.Defaults;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UsersService : IUsersService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFriendsRepository _friendsRepository;
    private readonly ILogger<UsersService> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
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

    private async Task<UserInfo?> GetBy(ClaimsPrincipal? currentUserClaimsPrincipal,
        Expression<Func<ApplicationUser, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        
        var user = await _userManager.GetUserAsync(currentUserClaimsPrincipal);
        if (user == null) throw new ArgumentException("User was not found", nameof(currentUserClaimsPrincipal));
        
        var roles = await _userManager.GetRolesAsync(user);
        
        var friends = (await _friendsRepository.GetFriendsFor(user.Id)).ToDictionary(f => f.Id);
        var inviters = (await _friendsRepository.GetInviters(user.Id)).ToDictionary(f => f.Id);
        var invited = (await _friendsRepository.GetInvitedUsersBy(user.Id)).ToDictionary(f => f.Id);

        ApplicationUser? foundUser;
        {
            var friendsIds = friends.Select(f => f.Value.Id);
            var invitersIds = inviters.Select(f => f.Value.Id);
            
            foundUser = 
                roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin))
                    ? _userManager.Users.FirstOrDefault(predicate)
                    : _userManager.Users.Where(u => u.IsPublic || friendsIds.Contains(u.Id) || invitersIds.Contains(u.Id)).FirstOrDefault(predicate);
        }
        
        if (foundUser == null) return null;
        
        var relationshipState = 
            friends.ContainsKey(foundUser.Id) ? RelationshipState.Friends
            : invited.ContainsKey(foundUser.Id) ? RelationshipState.Invited
            : inviters.ContainsKey(foundUser.Id) ? RelationshipState.CanBeAccepted
            : RelationshipState.None;

        var userRoles = await _userManager.GetRolesAsync(foundUser);
        
        return new UserInfo(foundUser, relationshipState, userRoles);
    }
    
    public async Task<UserInfo?> GetById(ClaimsPrincipal? currentUserClaimsPrincipal, Guid id)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);

        return await GetBy(currentUserClaimsPrincipal, u => u.Id == id);
    }

    public async Task<UserInfo?> GetByName(ClaimsPrincipal? currentUserClaimsPrincipal, string? userName)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        return await GetBy(currentUserClaimsPrincipal, u => u.UserName == userName);
    }

    public async Task<UserInfo?> GetByEmail(ClaimsPrincipal? currentUserClaimsPrincipal, string? email)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return await GetBy(currentUserClaimsPrincipal, u => u.Email == email);
    }

    public async Task<OperationResult> CreateUser(ClaimsPrincipal? currentUserClaimsPrincipal, CreateUserDto createUserDto)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        ArgumentNullException.ThrowIfNull(createUserDto);
        ArgumentException.ThrowIfNullOrWhiteSpace(createUserDto.Password);
        
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

    public async Task<OperationResult> UpdateUser(ClaimsPrincipal? currentUserClaimsPrincipal, UpdateUserDto updateUserDto)
    {
        ArgumentNullException.ThrowIfNull(currentUserClaimsPrincipal);
        ArgumentNullException.ThrowIfNull(updateUserDto);
        
        var user = await _userManager.GetUserAsync(currentUserClaimsPrincipal);
        if (user == null) throw new NotFoundException("User was not found");
        
        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains(nameof(Role.Admin)) || roles.Contains(nameof(Role.Root)))
        {
            var applicationUser = await _userManager.FindByIdAsync(updateUserDto.UserId.ToString());
            if (applicationUser == null) return new DefaultOperationResult(false, new NotFoundException("User was not found"), new []{"User was not found"}) ;

            if (await _userManager.IsInRoleAsync(applicationUser, nameof(Role.Root)))
            {
                throw new OperationNotAllowedException("Root cannot be edited by another user");
            }

            var isAdmin = await _userManager.IsInRoleAsync(applicationUser, nameof(Role.Admin));
            if (!isAdmin || (isAdmin && roles.Contains(nameof(Role.Root))))
            {
                applicationUser.UserName = updateUserDto.Username;
                applicationUser.IsPublic = applicationUser.IsPublic && !updateUserDto.SetPrivate;
                if (updateUserDto.ClearAbout) applicationUser.About = string.Empty;
                
                var result = await _userManager.UpdateAsync(applicationUser);

                if (result.Succeeded)
                {
                    if (roles.Contains(nameof(Role.Root)))
                    {
                        if (!isAdmin && updateUserDto.IsAdmin)
                        {
                            await _userManager.AddToRoleAsync(applicationUser, nameof(Role.Admin));
                        }
                        else if (isAdmin && !updateUserDto.IsAdmin)
                        {
                            await _userManager.RemoveFromRoleAsync(applicationUser, nameof(Role.Admin));
                        }
                    }
                    return new DefaultOperationResult(true);
                }
                else
                {
                    return new DefaultOperationResult(false, errors: result.Errors.Select(err => err.Description));
                }
            }

            return new DefaultOperationResult(false, new OperationNotAllowedException("Only root can edit admins"));
        }
        
        return new DefaultOperationResult(false, new OperationNotAllowedException("User is not admin or root"),
            new[] { "User is not admin or root" });
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
}