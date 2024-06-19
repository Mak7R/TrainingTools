﻿using System.Globalization;
using System.Linq.Expressions;
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
using CsvHelper;

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

    public async Task<IEnumerable<UserInfo>> GetAllUsers(ApplicationUser? currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        
        var roles = await _userManager.GetRolesAsync(currentUser);
        
        var users = 
            roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin))
                ? _userManager.Users.Where(u => u.Id != currentUser.Id).ToList()
                : _userManager.Users.Where(u => u.Id != currentUser.Id && u.IsPublic).ToList();
        
        var friends = (await _friendsRepository.GetFriendsFor(currentUser.Id)).ToDictionary(f => f.Id);
        var inviters = (await _friendsRepository.GetInviters(currentUser.Id)).ToDictionary(f => f.Id);
        var invited = (await _friendsRepository.GetInvitedUsersBy(currentUser.Id)).ToDictionary(f => f.Id);
        
        var userInfos = new List<UserInfo>();

        foreach (var applicationUser in users)
            userInfos.Add(await CreateUserInfo(friends, inviters, invited, applicationUser));
        
        return userInfos;
    }

    public async Task<Stream> GetAllUsersAsCsv()
    {
        var users = _userManager.Users.ToList();

        var stream = new MemoryStream();
        var csvWriter = new CsvWriter(new StreamWriter(stream), CultureInfo.InvariantCulture, true);
        
        csvWriter.WriteField("Username");
        csvWriter.WriteField("Email");
        csvWriter.WriteField("Phone");
        csvWriter.WriteField("IsPublic");
        csvWriter.WriteField("Roles");
        
        await csvWriter.NextRecordAsync();

        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            csvWriter.WriteField(user.UserName);
            csvWriter.WriteField(user.Email);
            csvWriter.WriteField(user.PhoneNumber);
            csvWriter.WriteField(user.IsPublic);
            csvWriter.WriteField(string.Join(';', userRoles));
            await csvWriter.NextRecordAsync();
            await csvWriter.FlushAsync();
        }

        stream.Position = 0;
        return stream;
    }

    private async Task<UserInfo?> GetBy(ApplicationUser? currentUser, Expression<Func<ApplicationUser, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        
        var roles = await _userManager.GetRolesAsync(currentUser);
        
        var friends = (await _friendsRepository.GetFriendsFor(currentUser.Id)).ToDictionary(f => f.Id);
        var inviters = (await _friendsRepository.GetInviters(currentUser.Id)).ToDictionary(f => f.Id);
        var invited = (await _friendsRepository.GetInvitedUsersBy(currentUser.Id)).ToDictionary(f => f.Id);

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
    
    public async Task<UserInfo?> GetById(ApplicationUser? currentUser, Guid id)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        return await GetBy(currentUser, u => u.Id == id);
    }

    public async Task<UserInfo?> GetByName(ApplicationUser? currentUser, string? userName)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        return await GetBy(currentUser, u => u.UserName == userName);
    }

    public async Task<UserInfo?> GetByEmail(ApplicationUser? currentUser, string? email)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return await GetBy(currentUser, u => u.Email == email);
    }

    public async Task<OperationResult> CreateUser(ApplicationUser? currentUser, CreateUserDto createUserDto)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(createUserDto);
        ArgumentException.ThrowIfNullOrWhiteSpace(createUserDto.Password);
        
        var roles = await _userManager.GetRolesAsync(currentUser);

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

    public async Task<OperationResult> UpdateUser(ApplicationUser? currentUser, UpdateUserDto updateUserDto)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(updateUserDto);
        
        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

        if (currentUserRoles.Contains(nameof(Role.Admin)) || currentUserRoles.Contains(nameof(Role.Root)))
        {
            var updatingUser = await _userManager.FindByIdAsync(updateUserDto.UserId.ToString());
            if (updatingUser == null) return new DefaultOperationResult(false, new NotFoundException("User was not found"), new []{"User was not found"}) ;

            if (await _userManager.IsInRoleAsync(updatingUser, nameof(Role.Root)))
            {
                throw new OperationNotAllowedException("Root cannot be edited by another user");
            }

            var updatingUserIsAdmin = await _userManager.IsInRoleAsync(updatingUser, nameof(Role.Admin));
            if (!updatingUserIsAdmin || (updatingUserIsAdmin && currentUserRoles.Contains(nameof(Role.Root))))
            {
                updatingUser.UserName = updateUserDto.Username;
                updatingUser.IsPublic = updatingUser.IsPublic && !updateUserDto.SetPrivate;
                if (updateUserDto.ClearAbout) updatingUser.About = string.Empty;
                
                var result = await _userManager.UpdateAsync(updatingUser);

                if (result.Succeeded)
                {
                    if (currentUserRoles.Contains(nameof(Role.Root)))
                    {
                        if (!updatingUserIsAdmin && updateUserDto.IsAdmin)
                        {
                            await _userManager.AddToRoleAsync(updatingUser, nameof(Role.Admin));
                        }
                        else if (updatingUserIsAdmin && !updateUserDto.IsAdmin)
                        {
                            await _userManager.RemoveFromRoleAsync(updatingUser, nameof(Role.Admin));
                        }
                    }

                    var updatingUserIsTrainer = await _userManager.IsInRoleAsync(updatingUser, nameof(Role.Trainer));
                    if (!updatingUserIsTrainer && updateUserDto.IsTrainer)
                    {
                        await _userManager.AddToRoleAsync(updatingUser, nameof(Role.Trainer));
                    }
                    else if (updatingUserIsTrainer && !updateUserDto.IsTrainer)
                    {
                        await _userManager.RemoveFromRoleAsync(updatingUser, nameof(Role.Trainer));
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

    public async Task<OperationResult> DeleteUser(ApplicationUser? currentUser, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
        
        if (currentUserRoles.Contains(nameof(Role.Admin)) || currentUserRoles.Contains(nameof(Role.Root)))
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