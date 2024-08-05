using Application.Constants;
using Application.Dtos;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Application.Services;
using Domain.Enums;
using Domain.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ApplicationTests;

public class UsersServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IFriendsService> _friendsServiceMock;
    private readonly UsersService _usersService;

    public UsersServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _friendsServiceMock = new Mock<IFriendsService>();
        var loggerMock = new Mock<ILogger<UsersService>>();
        var localizerMock = new Mock<IStringLocalizer<UsersService>>();
        
        _usersService = new UsersService(
            _userManagerMock.Object,
            _friendsServiceMock.Object,
            loggerMock.Object,
            localizerMock.Object
            );
    }

    #region GetAllUsers

    [Fact]
    public async Task GetAllUsers_ShouldThrowArgumentNullException_WhenCurrentUserIsNull()
    {
        // Arrange
        ApplicationUser currentUser = null!;

        // Act
        Func<Task> action = async () => await _usersService.GetAll(currentUser);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }
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

        _friendsServiceMock.Setup(mock => mock.GetFriendsFor(user))
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
        var updateUserDto = new UpdateUserDto { UserId = Guid.NewGuid(), IsAdmin = true};

        _userManagerMock.Setup(mock => mock.GetRolesAsync(currentUser))
            .ReturnsAsync(new List<string> { nameof(Role.Admin) });

        var existingUser = new ApplicationUser { Id = updateUserDto.UserId };
        _userManagerMock.Setup(mock => mock.FindByIdAsync(existingUser.Id.ToString()))
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

        // Act
        Func<Task> action = async () => await _usersService.Delete(currentUser, Guid.Empty);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnSuccess_WhenAdminDeletesUser()
    {
        // Arrange
        var currentUser = new ApplicationUser { Id = Guid.NewGuid() };

        _userManagerMock.Setup(mock => mock.GetRolesAsync(currentUser))
            .ReturnsAsync(new List<string> { nameof(Role.Admin) });

        var userToDelete = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(mock => mock.FindByIdAsync(userToDelete.Id.ToString()))
            .ReturnsAsync(userToDelete);

        _userManagerMock.Setup(mock => mock.DeleteAsync(userToDelete))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersService.Delete(currentUser, userToDelete.Id);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    // Add more tests for DeleteUser method covering various scenarios

    #endregion

    // Add tests for other methods in UsersService as per your application requirements
}