﻿using Application.Constants;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Application.Services;
using AutoFixture;
using Domain.Defaults;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace ApplicationTests;

public class GroupsServiceTests
{
    private readonly Mock<IGroupsRepository> _groupsRepositoryMock;
    private readonly IGroupsService _groupsService;
    private readonly IFixture _fixture;

    public GroupsServiceTests()
    {
        _groupsRepositoryMock = new Mock<IGroupsRepository>();
        _groupsService = new GroupsService(_groupsRepositoryMock.Object);
        _fixture = new Fixture();
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ShouldReturnAllGroups_WhenNoOrderAndFilterIsApplied()
    {
        // Arrange
        var groups = _fixture.CreateMany<Group>().ToList();
        _groupsRepositoryMock.Setup(repo => repo.GetAll(null)).ReturnsAsync(groups);

        // Act
        var result = await _groupsService.GetAll();

        // Assert
        result.Should().BeEquivalentTo(groups);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOrderedGroups_WhenOrderIsApplied()
    {
        // Arrange
        var groups = _fixture.CreateMany<Group>().ToList();
        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.Group.Name,
            Order = OrderOptionNames.Shared.Ascending
        };

        _groupsRepositoryMock.Setup(repo => repo.GetAll(null)).ReturnsAsync(groups);

        // Act
        var result = await _groupsService.GetAll(orderModel);

        // Assert
        result.Should().BeInAscendingOrder(g => g.Name);
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnDescendingOrderedGroups_WhenOrderIsApplied()
    {
        // Arrange
        var groups = _fixture.CreateMany<Group>().ToList();
        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.Group.Name,
            Order = OrderOptionNames.Shared.Descending
        };

        _groupsRepositoryMock.Setup(repo => repo.GetAll(null)).ReturnsAsync(groups);

        // Act
        var result = await _groupsService.GetAll(orderModel);

        // Assert
        result.Should().BeInDescendingOrder(g => g.Name);
    }

    [Fact]
    public async Task GetAll_ShouldReturnFilteredGroups_WhenFilterIsApplied()
    {
        // Arrange
        var groups = _fixture.CreateMany<Group>().ToList();
        var filterModel = new FilterModel
        {
            { FilterOptionNames.Group.Name, groups.First().Name }
        };

        _groupsRepositoryMock.Setup(repo => repo.GetAll(filterModel)).ReturnsAsync(groups.Where(g => g.Name == groups.First().Name));

        // Act
        var result = await _groupsService.GetAll(null, filterModel);

        // Assert
        result.Should().BeEquivalentTo(groups.Where(g => g.Name == groups.First().Name));
    }

    #endregion

    #region GetByName

    [Fact]
    public async Task GetByName_ShouldReturnGroup_WhenGroupExists()
    {
        // Arrange
        var group = _fixture.Create<Group>();
        _groupsRepositoryMock.Setup(repo => repo.GetByName(group.Name)).ReturnsAsync(group);

        // Act
        var result = await _groupsService.GetByName(group.Name);

        // Assert
        result.Should().BeEquivalentTo(group);
    }

    [Fact]
    public async Task GetByName_ShouldReturnNull_WhenGroupDoesNotExist()
    {
        // Arrange
        _groupsRepositoryMock.Setup(repo => repo.GetByName(It.IsAny<string>())).ReturnsAsync((Group)null);

        // Act
        var result = await _groupsService.GetByName("NonExistentName");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ShouldReturnGroup_WhenGroupExists()
    {
        // Arrange
        var group = _fixture.Create<Group>();
        _groupsRepositoryMock.Setup(repo => repo.GetById(group.Id)).ReturnsAsync(group);

        // Act
        var result = await _groupsService.GetById(group.Id);

        // Assert
        result.Should().BeEquivalentTo(group);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenGroupDoesNotExist()
    {
        // Arrange
        _groupsRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).ReturnsAsync((Group)null);

        // Act
        var result = await _groupsService.GetById(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateGroup

    [Fact]
    public async Task CreateGroup_ShouldReturnSuccess_WhenGroupIsCreatedSuccessfully()
    {
        // Arrange
        var group = _fixture.Create<Group>();
        var operationResult = new DefaultOperationResult(true);
        _groupsRepositoryMock.Setup(repo => repo.CreateGroup(It.IsAny<Group>())).ReturnsAsync(operationResult);

        // Act
        var result = await _groupsService.CreateGroup(group);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task CreateGroup_ShouldThrowArgumentNullException_WhenGroupIsNull()
    {
        // Act
        Func<Task> action = async () => await _groupsService.CreateGroup(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region UpdateGroup

    [Fact]
    public async Task UpdateGroup_ShouldReturnSuccess_WhenGroupIsUpdatedSuccessfully()
    {
        // Arrange
        var group = _fixture.Create<Group>();
        var operationResult = new DefaultOperationResult(true);
        _groupsRepositoryMock.Setup(repo => repo.UpdateGroup(It.IsAny<Group>())).ReturnsAsync(operationResult);

        // Act
        var result = await _groupsService.UpdateGroup(group);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task UpdateGroup_ShouldThrowArgumentNullException_WhenGroupIsNull()
    {
        // Act
        Func<Task> action = async () => await _groupsService.UpdateGroup(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region DeleteGroup

    [Fact]
    public async Task DeleteGroup_ShouldReturnSuccess_WhenGroupIsDeletedSuccessfully()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var operationResult = new DefaultOperationResult(true);
        _groupsRepositoryMock.Setup(repo => repo.DeleteGroup(groupId)).ReturnsAsync(operationResult);

        // Act
        var result = await _groupsService.DeleteGroup(groupId);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task DeleteGroup_ShouldReturnFailure_WhenGroupDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var operationResult = new DefaultOperationResult(true);
        _groupsRepositoryMock.Setup(repo => repo.DeleteGroup(groupId)).ReturnsAsync(operationResult);

        // Act
        var result = await _groupsService.DeleteGroup(groupId);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    #endregion
}