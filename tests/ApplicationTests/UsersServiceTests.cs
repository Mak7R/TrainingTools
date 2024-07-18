using Application.Constants;
using Application.Dtos;
using Application.Interfaces.RepositoryInterfaces;
using Application.Models.Shared;
using Application.Services;
using Domain.Enums;
using Domain.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests;

public class UsersServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IFriendsRepository> _friendsRepositoryMock;
    private readonly Mock<ILogger<UsersService>> _loggerMock;
    private readonly UsersService _usersService;
    private readonly Mock<IStringLocalizer<UsersService>> _localizerMock;

    public UsersServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _friendsRepositoryMock = new Mock<IFriendsRepository>();
        _loggerMock = new Mock<ILogger<UsersService>>();
        _localizerMock = new Mock<IStringLocalizer<UsersService>>();
        
        _usersService = new UsersService(
            _userManagerMock.Object,
            _friendsRepositoryMock.Object,
            _loggerMock.Object,
            _localizerMock.Object
            );
    }

    #region GetAllUsers

    [Fact]
    public async Task GetAllUsers_ShouldThrowArgumentNullException_WhenCurrentUserIsNull()
    {
        // Arrange
        ApplicationUser currentUser = null;

        // Act
        Func<Task> action = async () => await _usersService.GetAll(currentUser);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnFilteredUsers_WhenFilterModelIsProvided()
    {
        // Arrange
        var currentUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "currentUser", IsPublic = true };
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = Guid.NewGuid(), UserName = "user1", IsPublic = true },
            new ApplicationUser { Id = Guid.NewGuid(), UserName = "user2", IsPublic = false },
            new ApplicationUser { Id = Guid.NewGuid(), UserName = "user3", IsPublic = true }
        };

        _userManagerMock.Setup(mock => mock.GetRolesAsync(currentUser))
            .ReturnsAsync(new List<string>());

        _userManagerMock.Setup(mock => mock.Users)
            .Returns(users.AsQueryable());

        var filterModel = new FilterModel()
        {
            {FilterOptionNames.User.Name, "user"}
        };

        // Act
        var result = await _usersService.GetAll(currentUser, filterModel: filterModel);

        // Assert
        result.Should().HaveCount(2); // user1 and user3
        result.Should().NotContain(user => !user.User.IsPublic);
    }

    // Add more tests for GetAllUsers method covering various scenarios

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ShouldThrowArgumentNullException_WhenCurrentUserIsNull()
    {
        // Arrange
        ApplicationUser currentUser = null;
        var userId = Guid.NewGuid();

        // Act
        Func<Task> action = async () => await _usersService.GetById(currentUser, userId);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetById_ShouldReturnUserInfo_WhenUserExists()
    {
        // Arrange
        var currentUser = new ApplicationUser { Id = Guid.NewGuid() };
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "testUser" };

        _userManagerMock.Setup(mock => mock.FindByNameAsync("testUser"))
            .ReturnsAsync(user);

        _friendsRepositoryMock.Setup(mock => mock.GetFriendsFor(userId))
            .ReturnsAsync(new List<ApplicationUser>());

        _userManagerMock.Setup(mock => mock.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>() { nameof(Role.User), nameof(Role.Admin), nameof(Role.Root) });

        _userManagerMock.Setup(mock => mock.Users).Returns(new List<ApplicationUser>(){currentUser, user}.AsQueryable());
        
        // Act
        var result = await _usersService.GetById(currentUser, userId);

        // Assert
        result.Should().NotBeNull();
        result.User.Id.Should().Be(userId);
    }

    // Add more tests for GetById method covering various scenarios

    #endregion

    #region CreateUser

    [Fact]
    public async Task CreateUser_ShouldThrowArgumentNullException_WhenCurrentUserIsNull()
    {
        // Arrange
        ApplicationUser currentUser = null;
        var createUserDto = new CreateUserDto();

        // Act
        Func<Task> action = async () => await _usersService.Create(currentUser, createUserDto);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateUser_ShouldReturnSuccess_WhenAdminCreatesUser()
    {
        // Arrange
        var currentUser = new ApplicationUser { Id = Guid.NewGuid() };
        var createUserDto = new CreateUserDto { Username = "newUser", Email = "newUser@example.com", Password = "Password1!" };

        _userManagerMock.Setup(mock => mock.GetRolesAsync(currentUser))
            .ReturnsAsync(new List<string> { nameof(Role.Admin) });

        _userManagerMock.Setup(mock => mock.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(mock => mock.AddToRoleAsync(It.IsAny<ApplicationUser>(), nameof(Role.User)))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersService.Create(currentUser, createUserDto);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    // Add more tests for CreateUser method covering various scenarios

    #endregion

    #region UpdateUser

    [Fact]
    public async Task UpdateUser_ShouldThrowArgumentNullException_WhenCurrentUserIsNull()
    {
        // Arrange
        ApplicationUser currentUser = null;
        var updateUserDto = new UpdateUserDto();

        // Act
        Func<Task> action = async () => await _usersService.Update(currentUser, updateUserDto);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnSuccess_WhenAdminUpdatesUser()
    {
        // Arrange
        var currentUser = new ApplicationUser { Id = Guid.NewGuid() };
        var updateUserDto = new UpdateUserDto { UserName = "existingUser", IsAdmin = true};

        _userManagerMock.Setup(mock => mock.GetRolesAsync(currentUser))
            .ReturnsAsync(new List<string> { nameof(Role.Admin) });

        var existingUser = new ApplicationUser { UserName = "existingUser" };
        _userManagerMock.Setup(mock => mock.FindByNameAsync("existingUser"))
            .ReturnsAsync(existingUser);

        _userManagerMock.Setup(mock => mock.UpdateAsync(existingUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersService.Update(currentUser, updateUserDto);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    // Add more tests for UpdateUser method covering various scenarios

    #endregion

    #region DeleteUser

    [Fact]
    public async Task DeleteUser_ShouldThrowArgumentNullException_WhenCurrentUserIsNull()
    {
        // Arrange
        ApplicationUser currentUser = null;
        var userName = "testUser";

        // Act
        Func<Task> action = async () => await _usersService.Delete(currentUser, userName);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnSuccess_WhenAdminDeletesUser()
    {
        // Arrange
        var currentUser = new ApplicationUser { Id = Guid.NewGuid() };
        var userName = "userToDelete";

        _userManagerMock.Setup(mock => mock.GetRolesAsync(currentUser))
            .ReturnsAsync(new List<string> { nameof(Role.Admin) });

        var userToDelete = new ApplicationUser { UserName = userName };
        _userManagerMock.Setup(mock => mock.FindByNameAsync(userName))
            .ReturnsAsync(userToDelete);

        _userManagerMock.Setup(mock => mock.DeleteAsync(userToDelete))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersService.Delete(currentUser, userName);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    // Add more tests for DeleteUser method covering various scenarios

    #endregion

    // Add tests for other methods in UsersService as per your application requirements
}