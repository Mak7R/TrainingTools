using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Application.Services;
using AutoFixture;
using Domain.Defaults;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace ApplicationTests;

public class ExercisesServiceTests
{
    private readonly Mock<IExercisesRepository> _exercisesRepositoryMock;
    private readonly IExercisesService _exercisesService;
    private readonly IFixture _fixture;

    public ExercisesServiceTests()
    {
        _exercisesRepositoryMock = new Mock<IExercisesRepository>();
        _exercisesService = new ExercisesService(_exercisesRepositoryMock.Object);
        _fixture = new Fixture();
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ShouldReturnAllExercises_WhenNoOrderAndFilterIsApplied()
    {
        // Arrange
        var exercises = _fixture.CreateMany<Exercise>().ToList();
        _exercisesRepositoryMock.Setup(repo => repo.GetAll(null)).ReturnsAsync(exercises);

        // Act
        var result = await _exercisesService.GetAll();

        // Assert
        result.Should().BeEquivalentTo(exercises);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOrderedExercisesByName_WhenOrderByNameIsApplied()
    {
        // Arrange
        var exercises = _fixture.CreateMany<Exercise>().ToList();
        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.Exercise.Name,
            OrderOption = OrderOptionNames.Shared.Ascending
        };

        _exercisesRepositoryMock.Setup(repo => repo.GetAll(null)).ReturnsAsync(exercises);

        // Act
        var result = await _exercisesService.GetAll(orderModel);

        // Assert
        result.Should().BeInAscendingOrder(e => e.Name);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOrderedExercisesByGroupName_WhenOrderByGroupNameIsApplied()
    {
        // Arrange
        var exercises = _fixture.CreateMany<Exercise>().ToList();
        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.Exercise.GroupName,
            OrderOption = OrderOptionNames.Shared.Ascending
        };

        _exercisesRepositoryMock.Setup(repo => repo.GetAll(null)).ReturnsAsync(exercises);

        // Act
        var result = await _exercisesService.GetAll(orderModel);

        // Assert
        result.Should().BeInAscendingOrder(e => e.Group.Name);
    }

    [Fact]
    public async Task GetAll_ShouldReturnFilteredExercises_WhenFilterIsApplied()
    {
        // Arrange
        var exercises = _fixture.CreateMany<Exercise>().ToList();
        var filterModel = new FilterModel
        {
            { FilterOptionNames.Exercise.Name, exercises.First().Name }
        };

        _exercisesRepositoryMock.Setup(repo => repo.GetAll(filterModel)).ReturnsAsync(exercises.Where(e => e.Name == exercises.First().Name));

        // Act
        var result = await _exercisesService.GetAll(null, filterModel);

        // Assert
        result.Should().BeEquivalentTo(exercises.Where(e => e.Name == exercises.First().Name));
    }

    #endregion

    #region GetByName

    [Fact]
    public async Task GetByName_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = _fixture.Create<Exercise>();
        _exercisesRepositoryMock.Setup(repo => repo.GetByName(exercise.Name)).ReturnsAsync(exercise);

        // Act
        var result = await _exercisesService.GetByName(exercise.Name);

        // Assert
        result.Should().BeEquivalentTo(exercise);
    }

    [Fact]
    public async Task GetByName_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Arrange
        _exercisesRepositoryMock.Setup(repo => repo.GetByName(It.IsAny<string>())).ReturnsAsync((Exercise)null);

        // Act
        var result = await _exercisesService.GetByName("NonExistentName");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = _fixture.Create<Exercise>();
        _exercisesRepositoryMock.Setup(repo => repo.GetById(exercise.Id)).ReturnsAsync(exercise);

        // Act
        var result = await _exercisesService.GetById(exercise.Id);

        // Assert
        result.Should().BeEquivalentTo(exercise);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Arrange
        _exercisesRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).ReturnsAsync((Exercise)null);

        // Act
        var result = await _exercisesService.GetById(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateExercise

    [Fact]
    public async Task CreateExercise_ShouldReturnSuccess_WhenExerciseIsCreatedSuccessfully()
    {
        // Arrange
        var exercise = _fixture.Create<Exercise>();
        var operationResult = new DefaultOperationResult(true);
        _exercisesRepositoryMock.Setup(repo => repo.Create(It.IsAny<Exercise>())).ReturnsAsync(operationResult);

        // Act
        var result = await _exercisesService.Create(exercise);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task CreateExercise_ShouldThrowArgumentNullException_WhenExerciseIsNull()
    {
        // Act
        Func<Task> action = async () => await _exercisesService.Create(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region UpdateExercise

    [Fact]
    public async Task UpdateExercise_ShouldReturnSuccess_WhenExerciseIsUpdatedSuccessfully()
    {
        // Arrange
        var exercise = _fixture.Create<Exercise>();
        var operationResult = new DefaultOperationResult(true);
        _exercisesRepositoryMock.Setup(repo => repo.Update(It.IsAny<Exercise>())).ReturnsAsync(operationResult);

        // Act
        var result = await _exercisesService.Update(exercise);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task UpdateExercise_ShouldThrowArgumentNullException_WhenExerciseIsNull()
    {
        // Act
        Func<Task> action = async () => await _exercisesService.Update(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateExercise_ShouldThrowArgumentNullException_WhenExerciseGroupIsNull()
    {
        // Arrange
        var exercise = _fixture.Build<Exercise>()
            .Without(e => e.Group)
            .Create();

        // Act
        Func<Task> action = async () => await _exercisesService.Update(exercise);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region DeleteExercise

    [Fact]
    public async Task DeleteExercise_ShouldReturnSuccess_WhenExerciseIsDeletedSuccessfully()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var operationResult = new DefaultOperationResult(true);
        _exercisesRepositoryMock.Setup(repo => repo.Delete(exerciseId)).ReturnsAsync(operationResult);

        // Act
        var result = await _exercisesService.Delete(exerciseId);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public async Task DeleteExercise_ShouldReturnFailure_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var operationResult = new DefaultOperationResult(true);
        _exercisesRepositoryMock.Setup(repo => repo.Delete(exerciseId)).ReturnsAsync(operationResult);

        // Act
        var result = await _exercisesService.Delete(exerciseId);

        // Assert
        result.Should().BeEquivalentTo(operationResult);
    }

    #endregion
}