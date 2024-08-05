using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Application.Services;
using AutoFixture;
using Domain.Defaults;
using Domain.Identity;
using Domain.Models.Friendship;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests;

public class FriendshipsServiceTests
{
    private readonly Mock<IRepository<Friendship, (Guid, Guid)>> _friendsRepositoryMock;
    private readonly Mock<IRepository<FriendInvitation, (Guid, Guid)>> _friendInvitationsRepositoryMock;
    private readonly IFriendsService _friendsService;
    private readonly IFixture _fixture;

    public FriendshipsServiceTests()
    {
        _friendsRepositoryMock = new Mock<IRepository<Friendship, (Guid, Guid)>>();
        _friendInvitationsRepositoryMock = new Mock<IRepository<FriendInvitation, (Guid, Guid)>>();
        
        var loggerMock = new Mock<Logger<FriendshipsService>>();
        
        _friendsService = new FriendshipsService(_friendInvitationsRepositoryMock.Object, _friendsRepositoryMock.Object,loggerMock.Object);
        _fixture = new Fixture();
    }
}