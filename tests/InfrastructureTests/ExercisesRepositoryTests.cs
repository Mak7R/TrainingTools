using Application.Constants;
using Application.Interfaces.RepositoryInterfaces;
using Application.Models.Shared;
using AutoFixture;
using Domain.Exceptions;
using Domain.Models;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Mappers;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace InfrastructureTests;

public class ExercisesRepositoryTests
{
    private readonly Mock<ILogger<ExercisesRepository>> _loggerMock;
    private readonly DbContextMock<ApplicationDbContext> _dbContextMock;
    private readonly DbSetMock<GroupEntity> _groupsDbSetMock; 
    private readonly DbSetMock<ExerciseEntity> _exercisesDbSetMock; 
    
    private readonly IFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IExercisesRepository _exercisesRepository;
    private readonly ApplicationDbContext _dbContext;
    
    public ExercisesRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _fixture = new Fixture();
        
        _dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
        _dbContext = _dbContextMock.Object;

        _loggerMock = new Mock<ILogger<ExercisesRepository>>();
        var logger = _loggerMock.Object;
        
        _groupsDbSetMock = _dbContextMock.CreateDbSetMock(temp => temp.Groups, new List<GroupEntity>());
        _exercisesDbSetMock = _dbContextMock.CreateDbSetMock(temp => temp.Exercises, new List<ExerciseEntity>());
        
        _exercisesRepository = new ExercisesRepository(_dbContext, logger);
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ShouldReturnEmptyEnumerable_WhenNoExercisesExist()
    {
        // Arrange
        _dbContext.Exercises.RemoveRange(_dbContext.Exercises);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _exercisesRepository.GetAll();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllExercises_WhenNoFilterIsApplied()
    {
        // Arrange
        var exercises = _fixture.CreateMany<ExerciseEntity>().ToList();
        await _dbContext.Exercises.AddRangeAsync(exercises);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _exercisesRepository.GetAll();

        // Assert
        result.Should().HaveCount(exercises.Count);
        result.Should().Contain(e => exercises
            .Select(ex => new { ex.Id, ex.Name })
            .Contains(new { e.Id, e.Name }));
    }

    [Fact]
    public async Task GetAll_ShouldReturnFilteredExercises_WhenFilterIsAppliedAndMatch()
    {
        // Arrange
        var group1 = _fixture.Create<GroupEntity>();
        var group2 = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddRangeAsync(group1, group2);
        await _dbContext.SaveChangesAsync();

        var exercises = _fixture.Build<ExerciseEntity>()
            .With(e => e.GroupId, group1.Id)
            .CreateMany(3).ToList();
        
        exercises.Add(_fixture.Build<ExerciseEntity>()
            .With(e => e.GroupId, group1.Id)
            .With(e => e.Name, "GroupName")
            .Create());
        
        await _dbContext.Exercises.AddRangeAsync(exercises);
        await _dbContext.SaveChangesAsync();

        var filterModel = new FilterModel
        {
            { FilterOptionNames.Exercise.Group, group1.Id.ToString() },
            { FilterOptionNames.Exercise.Name, "GroupName" }
        };

        // Act
        var result = await _exercisesRepository.GetAll(filterModel);

        // Assert
        var filteredExercises = exercises.Where(e => e.GroupId == group1.Id && e.Name.Contains("GroupName")).ToList();
        result.Should().HaveCount(filteredExercises.Count);
        result.Should().Contain(e => exercises
            .Select(ex => new { ex.Id, ex.Name })
            .Contains(new { e.Id, e.Name }));
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyEnumerable_WhenFilterIsAppliedAndDoesNotMatch()
    {
        // Arrange
        var group = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddAsync(group);
        await _dbContext.SaveChangesAsync();

        var exercises = _fixture.Build<ExerciseEntity>()
            .With(e => e.GroupId, group.Id)
            .CreateMany(5).ToList();
        await _dbContext.Exercises.AddRangeAsync(exercises);
        await _dbContext.SaveChangesAsync();

        var filterModel = new FilterModel
        {
            { FilterOptionNames.Exercise.Group, Guid.NewGuid().ToString() }
        };

        // Act
        var result = await _exercisesRepository.GetAll(filterModel);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ThrowsDataBaseException_WhenExceptionOccurs()
    {
        // Arrange
        _dbContextMock.Setup(db => db.Exercises).Throws(new Exception("Database error"));

        // Act
        Func<Task> action = async () => await _exercisesRepository.GetAll();

        // Assert
        await action.Should().ThrowAsync<DataBaseException>();
    }

    #endregion
    
    #region GetByName

    [Fact]
    public async Task GetByName_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = _fixture.Build<ExerciseEntity>()
            .Create();
        await _dbContext.Exercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _exercisesRepository.GetByName(exercise.Name);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(exercise.Id);
        result.Name.Should().Be(exercise.Name);
    }

    [Fact]
    public async Task GetByName_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Act
        var result = await _exercisesRepository.GetByName("NonExistentName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByName_ShouldThrowArgumentException_WhenNameIsNullOrWhiteSpace()
    {
        // Act
        Func<Task> action = async () => await _exercisesRepository.GetByName("");

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByName_ShouldThrowDataBaseException_WhenExceptionOccurs()
    {
        // Arrange
        _dbContextMock.Setup(db => db.Exercises).Throws(new Exception("Database error"));

        // Act
        Func<Task> action = async () => await _exercisesRepository.GetByName("TestName");

        // Assert
        await action.Should().ThrowAsync<DataBaseException>();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = _fixture.Build<ExerciseEntity>()
            .Create();
        await _dbContext.Exercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _exercisesRepository.GetById(exercise.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(exercise.Id);
        result.Name.Should().Be(exercise.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Act
        var result = await _exercisesRepository.GetById(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ShouldThrowDataBaseException_WhenExceptionOccurs()
    {
        // Arrange
        _dbContextMock.Setup(db => db.Exercises).Throws(new Exception("Database error"));

        // Act
        Func<Task> action = async () => await _exercisesRepository.GetById(Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<DataBaseException>();
    }

    #endregion

    #region CreateExercise
    
    [Fact]
    public async Task CreateExercise_ShouldReturnSuccess_WhenExerciseIsAddedSuccessfully()
    {
        // Arrange
        var exercise = _fixture.Build<Exercise>()
            .With(e => e.Group, _fixture.Create<Group>())
            .Create();
        
        // Act
        var result = await _exercisesRepository.Create(exercise);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Exception.Should().BeNull();
        result.Errors.Should().BeEmpty();
        var containableExercise = _exercisesDbSetMock.Object.FirstOrDefault(e => e.Id == exercise.Id);
        containableExercise.Should().NotBeNull();
        containableExercise.Name.Should().BeEquivalentTo(exercise.Name);
    }

    [Fact]
    public async Task CreateExercise_ShouldThrowArgumentNullException_WhenExerciseIsNull()
    {
        // Act
        Func<Task> action = async () => await _exercisesRepository.Create(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateExercise_ShouldThrowArgumentException_WhenExerciseNameIsNullOrWhiteSpace()
    {
        // Arrange
        var exercise = _fixture.Build<Exercise>()
            .With(e => e.Name, string.Empty)
            .With(e => e.Group, _fixture.Create<Group>())
            .Create();

        // Act
        Func<Task> action = async () => await _exercisesRepository.Create(exercise);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateExercise_ShouldThrowArgumentNullException_WhenExerciseGroupIsNull()
    {
        // Arrange
        var exercise = _fixture.Build<Exercise>().Without(e => e.Group).Create();

        // Act
        Func<Task> action = async () => await _exercisesRepository.Create(exercise);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateExercise_ShouldReturnFailure_WhenExerciseWithSameNameExists()
    {
        // Arrange
        var group = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddAsync(group);
        await _dbContext.SaveChangesAsync();

        var exerciseName = "ExistingExercise";
        var existingExercise = _fixture.Build<ExerciseEntity>()
            .With(e => e.Name, exerciseName)
            .With(e => e.GroupId, group.Id)
            .Create();
        
        await _dbContext.Exercises.AddAsync(existingExercise);
        await _dbContext.SaveChangesAsync();

        var newExercise = _fixture.Build<Exercise>()
            .With(e => e.Name, exerciseName)
            .With(e => e.Group, new Group { Id = group.Id })
            .Create();

        // Act
        var result = await _exercisesRepository.Create(newExercise);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<AlreadyExistsException>();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateExercise_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var exercise = _fixture.Create<Exercise>();

        _dbContextMock.Setup(db => db.SaveChangesAsync(default)).Throws(new Exception("Database error"));

        // Act
        var result = await _exercisesRepository.Create(exercise);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<DataBaseException>();
        result.Errors.Should().NotBeEmpty();
    }

    #endregion
    
    #region UpdateExercise

    [Fact]
    public async Task UpdateExercise_ShouldReturnSuccess_WhenExerciseIsUpdatedSuccessfully()
    {
        // Arrange
        var group = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddAsync(group);
        await _dbContext.SaveChangesAsync();
        
        var exercise = _fixture.Build<ExerciseEntity>()
            .With(e => e.GroupId, group.Id)
            .Create();
        await _dbContext.Exercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();

        var updatedExercise = new Exercise
        {
            Id = exercise.Id,
            Name = "UpdatedName",
            Group = new Group { Id = exercise.GroupId },
            About = "UpdatedAbout"
        };

        // Act
        var result = await _exercisesRepository.Update(updatedExercise);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Exception.Should().BeNull();
        result.Errors.Should().BeEmpty();

        var updatedExerciseEntity = await _dbContext.Exercises.FindAsync(exercise.Id);
        updatedExerciseEntity.Name.Should().Be(updatedExercise.Name);
        updatedExerciseEntity.About.Should().Be(updatedExercise.About);
    }

    [Fact]
    public async Task UpdateExercise_ShouldThrowArgumentNullException_WhenExerciseIsNull()
    {
        // Act
        Func<Task> action = async () => await _exercisesRepository.Update(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateExercise_ShouldThrowArgumentException_WhenExerciseNameIsNullOrWhiteSpace()
    {
        // Arrange
        var exercise = _fixture.Build<Exercise>()
            .With(e => e.Name, string.Empty)
            .With(e => e.Group, _fixture.Create<Group>())
            .Create();

        // Act
        Func<Task> action = async () => await _exercisesRepository.Update(exercise);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateExercise_ShouldThrowArgumentNullException_WhenExerciseGroupIsNull()
    {
        // Arrange
        var exercise = _fixture.Build<Exercise>()
            .Without(e => e.Group)
            .Create();

        // Act
        Func<Task> action = async () => await _exercisesRepository.Update(exercise);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturnFailure_WhenExerciseWithSameNameExists()
    {
        // Arrange
        var group = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddAsync(group);
        await _dbContext.SaveChangesAsync();

        var exercise1 = _fixture.Build<ExerciseEntity>()
            .With(e => e.GroupId, group.Id)
            .Create();

        var exercise2 = _fixture.Build<ExerciseEntity>()
            .With(e => e.Name, exercise1.Name)
            .Create();

        await _dbContext.Exercises.AddRangeAsync(exercise1, exercise2);
        await _dbContext.SaveChangesAsync();

        var updatedExercise = new Exercise
        {
            Id = exercise2.Id,
            Name = exercise1.Name,
            Group = new Group { Id = exercise2.GroupId },
            About = "UpdatedAbout"
        };

        // Act
        var result = await _exercisesRepository.Update(updatedExercise);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<AlreadyExistsException>();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturnFailure_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exercise = _fixture.Build<Exercise>()
            .Create();
        exercise.Group = _fixture.Create<Group>();

        // Act
        var result = await _exercisesRepository.Update(exercise);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<NotFoundException>();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var group = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddAsync(group);
        await _dbContext.SaveChangesAsync();
        
        var exerciseEntity = _fixture.Create<ExerciseEntity>();
        await _dbContext.Exercises.AddAsync(exerciseEntity);
        await _dbContext.SaveChangesAsync();
        
        var exercise = _fixture.Build<Exercise>()
            .With(e => e.Id, exerciseEntity.Id)
            .With(e => e.Group, new Group{Id = group.Id})
            .Create();

        _dbContextMock.Setup(db => db.SaveChangesAsync(default)).Throws(new Exception("Database error"));

        // Act
        var result = await _exercisesRepository.Update(exercise);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<DataBaseException>();
        result.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region DeleteExercise

    [Fact]
    public async Task DeleteExercise_ShouldReturnSuccess_WhenExerciseIsDeletedSuccessfully()
    {
        // Arrange
        var exercise = _fixture.Build<ExerciseEntity>()
            .Create();
        await _dbContext.Exercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _exercisesRepository.Delete(exercise.Id);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.ResultObject.Should().NotBeNull();
        result.Exception.Should().BeNull();
        result.Errors.Should().BeEmpty();

        var deletedExerciseEntity = await _dbContext.Exercises.FindAsync(exercise.Id);
        deletedExerciseEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteExercise_ShouldReturnFailure_WhenExerciseDoesNotExist()
    {
        // Act
        var result = await _exercisesRepository.Delete(Guid.NewGuid());

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<NotFoundException>();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteExercise_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var exercise = _fixture.Build<ExerciseEntity>()
            .Create();
        await _dbContext.Exercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();

        _dbContextMock.Setup(db => db.Exercises).Throws(new Exception("Database error"));

        // Act
        var result = await _exercisesRepository.Delete(exercise.Id);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<DataBaseException>();
        result.Errors.Should().NotBeEmpty();
    }

    #endregion
}