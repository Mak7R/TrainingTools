using Application.Interfaces.Repositories;
using Application.Interfaces.ServiceInterfaces;
using Application.Interfaces.Services;
using Application.Models;
using Application.Services;
using AutoFixture;
using Domain.Defaults;
using Domain.Identity;
using FluentAssertions;
using Moq;

namespace ApplicationTests;

public class FriendshipsServiceTests
{
    private readonly Mock<IFriendsRepository> _friendsRepositoryMock;
    private readonly IFriendsService _friendsService;
    private readonly IFixture _fixture;

    public FriendshipsServiceTests()
    {
        _friendsRepositoryMock = new Mock<IFriendsRepository>();
        _friendsService = new FriendshipsService(_friendsRepositoryMock.Object);
        _fixture = new Fixture();
    }

    #region GetInvitationsFor

    [Fact]
    public async Task GetInvitationsFor_ShouldThrowArgumentNullException_WhenUserIsNull()
    {
        // Act
        Func<Task> action = async () => await _friendsService.GetInvitationsFor(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetInvitationsFor_ShouldReturnInvitations_WhenUserIsValid()
    {
        // Arrange
        var user = _fixture.Create<ApplicationUser>();
        var invitations = _fixture.CreateMany<FriendInvitation>().ToList();
        _friendsRepositoryMock.Setup(repo => repo.GetInvitationsFor(user.Id)).ReturnsAsync(invitations);

        // Act
        var result = await _friendsService.GetInvitationsFor(user);

        // Assert
        result.Should().BeEquivalentTo(invitations);
    }

    #endregion

    #region GetInvitationsOf

    [Fact]
    public async Task GetInvitationsOf_ShouldThrowArgumentNullException_WhenUserIsNull()
    {
        // Act
        Func<Task> action = async () => await _friendsService.GetInvitationsOf(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetInvitationsOf_ShouldReturnInvitations_WhenUserIsValid()
    {
        // Arrange
        var user = _fixture.Create<ApplicationUser>();
        var invitations = _fixture.CreateMany<FriendInvitation>().ToList();
        _friendsRepositoryMock.Setup(repo => repo.GetInvitationsOf(user.Id)).ReturnsAsync(invitations);

        // Act
        var result = await _friendsService.GetInvitationsOf(user);

        // Assert
        result.Should().BeEquivalentTo(invitations);
    }

    #endregion

    #region GetFriendsFor

    [Fact]
    public async Task GetFriendsFor_ShouldThrowArgumentNullException_WhenUserIsNull()
    {
        // Act
        Func<Task> action = async () => await _friendsService.GetFriendsFor(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetFriendsFor_ShouldReturnFriends_WhenUserIsValid()
    {
        // Arrange
        var user = _fixture.Create<ApplicationUser>();
        var friends = _fixture.CreateMany<ApplicationUser>().ToList();
        _friendsRepositoryMock.Setup(repo => repo.GetFriendsFor(user.Id)).ReturnsAsync(friends);

        // Act
        var result = await _friendsService.GetFriendsFor(user);

        // Assert
        result.Should().BeEquivalentTo(friends);
    }

    #endregion

    #region CreateInvitation

    [Fact]
    public async Task CreateInvitation_ShouldThrowArgumentNullException_WhenInvitorIsNull()
    {
        // Arrange
        var target = _fixture.Create<ApplicationUser>();

        // Act
        Func<Task> action = async () => await _friendsService.CreateInvitation(null, target);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateInvitation_ShouldThrowArgumentNullException_WhenTargetIsNull()
    {
        // Arrange
        var invitor = _fixture.Create<ApplicationUser>();

        // Act
        Func<Task> action = async () => await _friendsService.CreateInvitation(invitor, null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateInvitation_ShouldReturnSuccess_WhenInvitationIsCreatedSuccessfully()
    {
        // Arrange
        var invitor = _fixture.Create<ApplicationUser>();
        var target = _fixture.Create<ApplicationUser>();
        var operationResult = new DefaultOperationResult(true);
        _friendsRepositoryMock.Setup(repo => repo.CreateInvitation(It.IsAny<FriendInvitation>())).ReturnsAsync(operationResult);

        // Act
        var result = await _friendsService.CreateInvitation(invitor, target);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    #endregion

    #region AcceptInvitation

    [Fact]
    public async Task AcceptInvitation_ShouldThrowArgumentNullException_WhenInvitorIsNull()
    {
        // Arrange
        var target = _fixture.Create<ApplicationUser>();

        // Act
        Func<Task> action = async () => await _friendsService.AcceptInvitation(null, target);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AcceptInvitation_ShouldThrowArgumentNullException_WhenTargetIsNull()
    {
        // Arrange
        var invitor = _fixture.Create<ApplicationUser>();

        // Act
        Func<Task> action = async () => await _friendsService.AcceptInvitation(invitor, null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AcceptInvitation_ShouldReturnSuccess_WhenInvitationIsAcceptedSuccessfully()
    {
        // Arrange
        var invitor = _fixture.Create<ApplicationUser>();
        var target = _fixture.Create<ApplicationUser>();
        var operationResult = new DefaultOperationResult(true);
        _friendsRepositoryMock.Setup(repo => repo.AcceptInvitation(It.IsAny<FriendInvitation>())).ReturnsAsync(operationResult);

        // Act
        var result = await _friendsService.AcceptInvitation(invitor, target);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    #endregion

    #region RemoveInvitation

    [Fact]
    public async Task RemoveInvitation_ShouldThrowArgumentNullException_WhenInvitorIsNull()
    {
        // Arrange
        var target = _fixture.Create<ApplicationUser>();

        // Act
        Func<Task> action = async () => await _friendsService.RemoveInvitation(null, target);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RemoveInvitation_ShouldThrowArgumentNullException_WhenTargetIsNull()
    {
        // Arrange
        var invitor = _fixture.Create<ApplicationUser>();

        // Act
        Func<Task> action = async () => await _friendsService.RemoveInvitation(invitor, null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RemoveInvitation_ShouldReturnSuccess_WhenInvitationIsRemovedSuccessfully()
    {
        // Arrange
        var invitor = _fixture.Create<ApplicationUser>();
        var target = _fixture.Create<ApplicationUser>();
        var operationResult = new DefaultOperationResult(true);
        _friendsRepositoryMock.Setup(repo => repo.RemoveInvitation(It.IsAny<FriendInvitation>())).ReturnsAsync(operationResult);

        // Act
        var result = await _friendsService.RemoveInvitation(invitor, target);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    #endregion

    #region RemoveFriendship

    [Fact]
    public async Task RemoveFriendship_ShouldThrowArgumentNullException_WhenUser1IsNull()
    {
        // Arrange
        var user2 = _fixture.Create<ApplicationUser>();

        // Act
        Func<Task> action = async () => await _friendsService.RemoveFriendship(null, user2);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RemoveFriendship_ShouldThrowArgumentNullException_WhenUser2IsNull()
    {
        // Arrange
        var user1 = _fixture.Create<ApplicationUser>();

        // Act
        Func<Task> action = async () => await _friendsService.RemoveFriendship(user1, null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RemoveFriendship_ShouldReturnSuccess_WhenFriendshipIsRemovedSuccessfully()
    {
        // Arrange
        var user1 = _fixture.Create<ApplicationUser>();
        var user2 = _fixture.Create<ApplicationUser>();
        var operationResult = new DefaultOperationResult(true);
        _friendsRepositoryMock.Setup(repo => repo.RemoveFriendship(user1.Id, user2.Id)).ReturnsAsync(operationResult);

        // Act
        var result = await _friendsService.RemoveFriendship(user1, user2);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    #endregion
}