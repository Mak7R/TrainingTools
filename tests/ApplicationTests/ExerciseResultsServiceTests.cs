using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Application.Services;
using AutoFixture;
using Domain.Defaults;
using Domain.Models;
using Domain.Rules;
using FluentAssertions;
using Moq;

namespace ApplicationTests;

public class ExerciseResultsServiceTests
{
    private readonly Mock<IExerciseResultsRepository> _exerciseResultsRepositoryMock;
    private readonly IExerciseResultsService _exerciseResultsService;
    private readonly IFixture _fixture;

    public ExerciseResultsServiceTests()
    {
        _exerciseResultsRepositoryMock = new Mock<IExerciseResultsRepository>();
        _exerciseResultsService = new ExerciseResultsService(_exerciseResultsRepositoryMock.Object);
        _fixture = new Fixture();
    }

    #region GetForUser

    [Fact]
    public async Task GetForUser_ShouldReturnAllResults_WhenNoOrderAndFilterIsApplied()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var results = _fixture.CreateMany<ExerciseResult>().ToList();
        _exerciseResultsRepositoryMock.Setup(repo => repo.GetForUser(ownerId, null)).ReturnsAsync(results);

        // Act
        var result = await _exerciseResultsService.GetForUser(ownerId);

        // Assert
        result.Should().BeEquivalentTo(results);
    }

    [Fact]
    public async Task GetForUser_ShouldReturnOrderedResultsByGroupName_WhenOrderByGroupNameIsApplied()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var results = _fixture.CreateMany<ExerciseResult>().ToList();
        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.ExerciseResults.GroupName,
            OrderOption = OrderOptionNames.Shared.Ascending
        };

        _exerciseResultsRepositoryMock.Setup(repo => repo.GetForUser(ownerId, null)).ReturnsAsync(results);

        // Act
        var result = await _exerciseResultsService.GetForUser(ownerId, orderModel);

        // Assert
        result.Should().BeInAscendingOrder(e => e.Exercise.Group.Name);
    }

    #endregion

    #region GetForExercise

    [Fact]
    public async Task GetForExercise_ShouldReturnAllResults_WhenNoOrderAndFilterIsApplied()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var results = _fixture.CreateMany<ExerciseResult>().ToList();
        _exerciseResultsRepositoryMock.Setup(repo => repo.GetForExercise(exerciseId, null)).ReturnsAsync(results);

        // Act
        var result = await _exerciseResultsService.GetForExercise(exerciseId);

        // Assert
        result.Should().BeEquivalentTo(results);
    }

    [Fact]
    public async Task GetForExercise_ShouldReturnOrderedResultsByOwnerName_WhenOrderByOwnerNameIsApplied()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var results = _fixture.CreateMany<ExerciseResult>().ToList();
        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.ExerciseResults.OwnerName,
            OrderOption = OrderOptionNames.Shared.Ascending
        };

        _exerciseResultsRepositoryMock.Setup(repo => repo.GetForExercise(exerciseId, null)).ReturnsAsync(results);

        // Act
        var result = await _exerciseResultsService.GetForExercise(exerciseId, orderModel);

        // Assert
        result.Should().BeInAscendingOrder(e => e.Owner.UserName);
    }

    #endregion

    #region GetOnlyUserAndFriendsResultForExercise

    [Fact]
    public async Task GetOnlyUserAndFriendsResultForExercise_ShouldReturnAllResults_WhenNoOrderAndFilterIsApplied()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var results = _fixture.CreateMany<ExerciseResult>().ToList();
        _exerciseResultsRepositoryMock.Setup(repo => repo.GetOnlyUserAndFriendsResultForExercise(userId, exerciseId, null)).ReturnsAsync(results);

        // Act
        var result = await _exerciseResultsService.GetOnlyUserAndFriendsResultForExercise(userId, exerciseId);

        // Assert
        result.Should().BeEquivalentTo(results);
    }

    [Fact]
    public async Task GetOnlyUserAndFriendsResultForExercise_ShouldReturnOrderedResultsByOwnerName_WhenOrderByOwnerNameIsApplied()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var results = _fixture.CreateMany<ExerciseResult>().ToList();
        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.ExerciseResults.OwnerName,
            OrderOption = OrderOptionNames.Shared.Ascending
        };

        _exerciseResultsRepositoryMock.Setup(repo => repo.GetOnlyUserAndFriendsResultForExercise(userId, exerciseId, null)).ReturnsAsync(results);

        // Act
        var result = await _exerciseResultsService.GetOnlyUserAndFriendsResultForExercise(userId, exerciseId, orderModel);

        // Assert
        result.Should().BeInAscendingOrder(e => e.Owner.UserName);
    }

    #endregion

    #region CreateResult

    [Fact]
    public async Task CreateResult_ShouldReturnSuccess_WhenResultIsCreatedSuccessfully()
    {
        // Arrange
        var exerciseResult = _fixture.Create<ExerciseResult>();
        var operationResult = new DefaultOperationResult(true);
        _exerciseResultsRepositoryMock.Setup(repo => repo.Create(It.IsAny<ExerciseResult>())).ReturnsAsync(operationResult);

        // Act
        var result = await _exerciseResultsService.Create(exerciseResult);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task CreateResult_ShouldThrowArgumentNullException_WhenResultIsNull()
    {
        // Act
        Func<Task> action = async () => await _exerciseResultsService.Create(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region UpdateResult

    [Fact]
    public async Task UpdateResult_ShouldReturnSuccess_WhenResultIsUpdatedSuccessfully()
    {
        // Arrange
        var exerciseResult = _fixture.Create<ExerciseResult>();
        var operationResult = new DefaultOperationResult(true);
        _exerciseResultsRepositoryMock.Setup(repo => repo.Update(It.IsAny<ExerciseResult>())).ReturnsAsync(operationResult);

        // Act
        var result = await _exerciseResultsService.Update(exerciseResult);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task UpdateResult_ShouldThrowArgumentNullException_WhenResultIsNull()
    {
        // Act
        Func<Task> action = async () => await _exerciseResultsService.Update(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateResult_ShouldReturnFailure_WhenWeightOrCountIsNegative()
    {
        // Arrange
        var exerciseResult = _fixture.Build<ExerciseResult>()
            .With(r => r.ApproachInfos, new List<Approach> { new Approach(-1, -1, "Test") })
            .Create();
        var operationResult = DefaultOperationResult.FromException(new InvalidOperationException("Weight and count cannot be less than 0"));
        
        // Act
        var result = await _exerciseResultsService.Update(exerciseResult);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task UpdateResult_ShouldReturnFailure_WhenCommentContainsSpecialSeparator()
    {
        // Arrange
        var exerciseResult = _fixture.Build<ExerciseResult>()
            .With(r => r.ApproachInfos, new List<Approach> { new Approach(1, 1, $"{SpecialConstants.DefaultSeparator}") })
            .Create();
        var operationResult = DefaultOperationResult.FromException(new InvalidOperationException($"Comments cannot contain symbol {SpecialConstants.DefaultSeparator}"));
        
        // Act
        var result = await _exerciseResultsService.Update(exerciseResult);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    #endregion

    #region DeleteResult

    [Fact]
    public async Task DeleteResult_ShouldReturnSuccess_WhenResultIsDeletedSuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var operationResult = new DefaultOperationResult(true);
        _exerciseResultsRepositoryMock.Setup(repo => repo.Delete(ownerId, exerciseId)).ReturnsAsync(operationResult);

        // Act
        var result = await _exerciseResultsService.Delete(ownerId, exerciseId);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task DeleteResult_ShouldReturnFailure_WhenResultDoesNotExist()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var operationResult = new DefaultOperationResult(true);
        _exerciseResultsRepositoryMock.Setup(repo => repo.Delete(ownerId, exerciseId)).ReturnsAsync(operationResult);

        // Act
        var result = await _exerciseResultsService.Delete(ownerId, exerciseId);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    #endregion

    #region Get

    [Fact]
    public async Task Get_ShouldReturnResult_WhenResultExists()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var exerciseResult = _fixture.Create<ExerciseResult>();
        _exerciseResultsRepositoryMock.Setup(repo => repo.Get(ownerId, exerciseId)).ReturnsAsync(exerciseResult);

        // Act
        var result = await _exerciseResultsService.Get(ownerId, exerciseId);

        // Assert
        result.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task Get_ShouldReturnNull_WhenResultDoesNotExist()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        _exerciseResultsRepositoryMock.Setup(repo => repo.Get(ownerId, exerciseId)).ReturnsAsync((ExerciseResult)null);

        // Act
        var result = await _exerciseResultsService.Get(ownerId, exerciseId);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}