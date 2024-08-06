using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq.Expressions;
using Application.Constants;
using Application.Dtos;
using Application.Enums;
using Application.Interfaces.Services;
using Application.Models.Shared;
using CsvHelper;
using Domain.Defaults;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UsersService : IUsersService
{
    private static readonly ReadOnlyDictionary<string, Func<string, Expression<Func<ApplicationUser, bool>>>>
        ApplicationUserFilters =
            new(new Dictionary<string, Func<string, Expression<Func<ApplicationUser, bool>>>>
            {
                { FilterOptionNames.User.Name, value => u => u.UserName != null && u.UserName.Contains(value) },
                { FilterOptionNames.User.PublicOnly, value => value == "true" ? u => u.IsPublic : _ => true },
                { FilterOptionNames.User.Id, value => Guid.TryParse(value, out var id) ? u => u.Id == id : _ => false }
            });

    private static readonly ReadOnlyDictionary<string, Func<string, Func<UserInfo, bool>>> UserInfoFilters =
        new(new Dictionary<string, Func<string, Func<UserInfo, bool>>>
        {
            {
                FilterOptionNames.User.Role, value =>
                {
                    if (string.Equals(value, nameof(Role.Root), StringComparison.CurrentCultureIgnoreCase))
                        return _ => false;
                    if (string.Equals(value, nameof(Role.User), StringComparison.CurrentCultureIgnoreCase))
                        return u => u.Roles.Count() == 1 &&
                                    u.Roles.Contains(value, StringComparer.CurrentCultureIgnoreCase);
                    return u => u.Roles.Contains(value, StringComparer.CurrentCultureIgnoreCase);
                }
            },
            {
                FilterOptionNames.User.RelationshipsState,
                value => u => u.RelationshipState.ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase)
            }
        });

    private static readonly
        ReadOnlyDictionary<OrderModel, Func<IEnumerable<UserInfo>, IEnumerable<UserInfo>>> UserInfosOrders =
            new(new Dictionary<OrderModel, Func<IEnumerable<UserInfo>, IEnumerable<UserInfo>>>
            {
                {
                    new OrderModel
                        { OrderBy = OrderOptionNames.User.Name, OrderOption = OrderOptionNames.Shared.Ascending },
                    query => query.OrderBy(i => i.User.UserName)
                },
                {
                    new OrderModel
                        { OrderBy = OrderOptionNames.User.Name, OrderOption = OrderOptionNames.Shared.Descending },
                    query => query.OrderByDescending(i => i.User.UserName)
                },
                {
                    new OrderModel
                        { OrderBy = OrderOptionNames.User.Role, OrderOption = OrderOptionNames.Shared.Ascending },
                    query => query.OrderBy(i => i.Roles, new RolesComparer())
                },
                {
                    new OrderModel
                        { OrderBy = OrderOptionNames.User.Role, OrderOption = OrderOptionNames.Shared.Descending },
                    query => query.OrderByDescending(i => i.Roles, new RolesComparer())
                },
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.User.RelationshipsState,
                        OrderOption = OrderOptionNames.Shared.Ascending
                    },
                    query => query.OrderBy(i => i.RelationshipState, new RelationshipStateComparer())
                },
                {
                    new OrderModel
                    {
                        OrderBy = OrderOptionNames.User.RelationshipsState,
                        OrderOption = OrderOptionNames.Shared.Descending
                    },
                    query => query.OrderByDescending(i => i.RelationshipState, new RelationshipStateComparer())
                }
            });

    private readonly IFriendsService _friendsService;
    private readonly IStringLocalizer<UsersService> _localizer;
    private readonly ILogger<UsersService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersService(UserManager<ApplicationUser> userManager, IFriendsService friendsService,
        ILogger<UsersService> logger, IStringLocalizer<UsersService> localizer)
    {
        _userManager = userManager;
        _friendsService = friendsService;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<IEnumerable<UserInfo>> GetAll(ApplicationUser? currentUser, FilterModel? filterModel = null,
        OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        var roles = await _userManager.GetRolesAsync(currentUser);

        var query = _userManager.Users.AsNoTracking();

        filterModel ??= new FilterModel();
        if (!(roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin))))
            filterModel[FilterOptionNames.User.PublicOnly] = "true";
        query = filterModel.Filter(query.Where(u => u.Id != currentUser.Id), ApplicationUserFilters);

        var users = await query.ToListAsync();

        var friends = (await _friendsService.GetFriendsFor(currentUser)).ToDictionary(f => f.Id);
        var inviters = (await _friendsService.GetInvitationsFor(currentUser)).Select(i => i.Invitor)
            .ToDictionary(f => f.Id);
        var invited = (await _friendsService.GetInvitationsOf(currentUser)).Select(i => i.Invited)
            .ToDictionary(f => f.Id);

        var userInfos = new List<UserInfo>();

        foreach (var applicationUser in users)
            userInfos.Add(await CreateUserInfo(friends, inviters, invited, applicationUser));

        var result = filterModel.Filter(userInfos, UserInfoFilters);

        if (orderModel is not null)
            result = orderModel.Order(result, UserInfosOrders);

        if (pageModel is not null)
            result = pageModel.TakePage(result);
        return result.ToList();
    }

    public async Task<int> Count(ApplicationUser? currentUser, FilterModel? filterModel)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        var roles = await _userManager.GetRolesAsync(currentUser);

        var query = _userManager.Users.AsNoTracking();
        filterModel ??= new FilterModel();
        if (!(roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin))))
            filterModel[FilterOptionNames.User.PublicOnly] = "true";
        return await query.CountAsync();
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

    public async Task<OperationResult> Create(ApplicationUser? currentUser, CreateUserDto createUserDto)
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
                IsPublic = createUserDto.IsPublic
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
                    await _userManager.AddToRoleAsync(applicationUser, nameof(Role.Admin));

                return new DefaultOperationResult(true);
            }

            return new DefaultOperationResult(result.Errors.Select(err => err.Description));
        }

        return DefaultOperationResult.FromException(new OperationNotAllowedException("User is not admin or root"));
    }

    public async Task<OperationResult> Update(ApplicationUser? currentUser, UpdateUserDto updateUserDto)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(updateUserDto);

        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

        if (currentUserRoles.Contains(nameof(Role.Admin)) || currentUserRoles.Contains(nameof(Role.Root)))
        {
            var updatingUser = await _userManager.FindByIdAsync(updateUserDto.UserId.ToString());
            if (updatingUser == null)
                return DefaultOperationResult.FromException(new NotFoundException("User was not found"));

            if (await _userManager.IsInRoleAsync(updatingUser, nameof(Role.Root)))
                throw new OperationNotAllowedException("Root cannot be edited by another user");

            var updatingUserIsAdmin = await _userManager.IsInRoleAsync(updatingUser, nameof(Role.Admin));
            if (!updatingUserIsAdmin || (updatingUserIsAdmin && currentUserRoles.Contains(nameof(Role.Root))))
            {
                updatingUser.IsPublic = updatingUser.IsPublic && !updateUserDto.SetPrivate;
                if (updateUserDto.ClearAbout) updatingUser.About = string.Empty;

                var result = await _userManager.UpdateAsync(updatingUser);

                if (result.Succeeded)
                {
                    if (currentUserRoles.Contains(nameof(Role.Root)))
                    {
                        if (!updatingUserIsAdmin && updateUserDto.IsAdmin)
                            await _userManager.AddToRoleAsync(updatingUser, nameof(Role.Admin));
                        else if (updatingUserIsAdmin && !updateUserDto.IsAdmin)
                            await _userManager.RemoveFromRoleAsync(updatingUser, nameof(Role.Admin));
                    }

                    var updatingUserIsTrainer = await _userManager.IsInRoleAsync(updatingUser, nameof(Role.Trainer));
                    if (!updatingUserIsTrainer && updateUserDto.IsTrainer)
                        await _userManager.AddToRoleAsync(updatingUser, nameof(Role.Trainer));
                    else if (updatingUserIsTrainer && !updateUserDto.IsTrainer)
                        await _userManager.RemoveFromRoleAsync(updatingUser, nameof(Role.Trainer));

                    return new DefaultOperationResult(true);
                }

                return new DefaultOperationResult(result.Errors.Select(err => err.Description));
            }

            return DefaultOperationResult.FromException(new OperationNotAllowedException("Only root can edit admins"));
        }

        return DefaultOperationResult.FromException(new OperationNotAllowedException("User is not admin or root"));
    }

    public async Task<OperationResult> Delete(ApplicationUser? currentUser, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

        if (currentUserRoles.Contains(nameof(Role.Admin)) || currentUserRoles.Contains(nameof(Role.Root)))
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                var message = $"User with id '{userId}' was not found";
                return DefaultOperationResult.FromException(new NotFoundException(message));
            }

            if (await _userManager.IsInRoleAsync(user, nameof(Role.Root)))
                return DefaultOperationResult.FromException(
                    new OperationNotAllowedException("Impossible to delete root user"));

            var isUserAdmin = await _userManager.IsInRoleAsync(user, nameof(Role.Admin));
            if (!isUserAdmin || (isUserAdmin && await _userManager.IsInRoleAsync(currentUser, nameof(Role.Root))))
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                    return new DefaultOperationResult(true);
                return new DefaultOperationResult(result.Errors.Select(err => err.Description));
            }

            return DefaultOperationResult.FromException(
                new OperationNotAllowedException("Only Root can delete admins"));
        }

        return DefaultOperationResult.FromException(
            new OperationNotAllowedException(_localizer["UserIsNotRootOrAdmin"]));
    }

    private async Task<UserInfo?> GetBy(ApplicationUser? currentUser, Expression<Func<ApplicationUser, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        var roles = await _userManager.GetRolesAsync(currentUser);
        var isCurrentUserAdmin = roles.Contains(nameof(Role.Root)) || roles.Contains(nameof(Role.Admin));

        var friends = (await _friendsService.GetFriendsFor(currentUser)).ToDictionary(f => f.Id);
        var inviters = (await _friendsService.GetInvitationsFor(currentUser)).Select(i => i.Invitor)
            .ToDictionary(f => f.Id);
        var invited = (await _friendsService.GetInvitationsOf(currentUser)).Select(i => i.Invited)
            .ToDictionary(f => f.Id);

        ApplicationUser? foundUser;
        {
            var friendsIds = friends.Select(f => f.Value.Id);
            var invitersIds = inviters.Select(f => f.Value.Id);

            foundUser = isCurrentUserAdmin
                ? _userManager.Users.FirstOrDefault(predicate)
                : _userManager.Users.Where(u => u.IsPublic || friendsIds.Contains(u.Id) || invitersIds.Contains(u.Id))
                    .FirstOrDefault(predicate);
        }

        if (foundUser == null) return null;

        if (!isCurrentUserAdmin)
            foundUser = new ApplicationUser
            {
                Id = foundUser.Id,
                UserName = foundUser.UserName,
                About = foundUser.About,
                IsPublic = foundUser.IsPublic
            };

        var relationshipState =
            friends.ContainsKey(foundUser.Id) ? RelationshipState.Friends
            : invited.ContainsKey(foundUser.Id) ? RelationshipState.Invited
            : inviters.ContainsKey(foundUser.Id) ? RelationshipState.CanBeAccepted
            : RelationshipState.None;

        var userRoles = await _userManager.GetRolesAsync(foundUser);

        return new UserInfo(foundUser, relationshipState, userRoles);
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

    private class RolesComparer : IComparer<IEnumerable<string>>
    {
        private static readonly Dictionary<string, int> RolePriorities = new()
        {
            { nameof(Role.Root), 3 },
            { nameof(Role.Admin), 2 },
            { nameof(Role.Trainer), 1 }
        };

        public int Compare(IEnumerable<string>? x, IEnumerable<string>? y)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);

            var xValue = GetRolePriority(x);
            var yValue = GetRolePriority(y);

            return yValue.CompareTo(xValue);
        }

        private static int GetRolePriority(IEnumerable<string> roles)
        {
            var rolesAsArray = roles as string[] ?? roles.ToArray();
            return (from role in RolePriorities.Keys where rolesAsArray.Contains(role) select RolePriorities[role])
                .FirstOrDefault();
        }
    }

    private class RelationshipStateComparer : IComparer<RelationshipState>
    {
        public int Compare(RelationshipState x, RelationshipState y)
        {
            // Определяем приоритеты состояний
            var xPriority = GetRelationshipStatePriority(x);
            var yPriority = GetRelationshipStatePriority(y);

            // Сравниваем по приоритетам
            return yPriority.CompareTo(xPriority);
        }

        private static int GetRelationshipStatePriority(RelationshipState state)
        {
            return state switch
            {
                RelationshipState.Friends => 3,
                RelationshipState.CanBeAccepted => 2,
                RelationshipState.Invited => 1,
                _ => 0
            };
        }
    }
}