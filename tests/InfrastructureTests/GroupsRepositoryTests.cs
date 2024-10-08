using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Models.Shared;
using AutoFixture;
using AutoMapper;
using Domain.Exceptions;
using Domain.Models;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Mapping.Profiles;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace InfrastructureTests;

public class GroupsRepositoryTests
{
    private readonly DbContextMock<ApplicationDbContext> _dbContextMock;
    private readonly DbSetMock<GroupEntity> _groupsDbSetMock; 
    
    private readonly IFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IRepository<Group, Guid> _groupsRepository;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GroupsRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _fixture = new Fixture();
        
        _dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
        _dbContext = _dbContextMock.Object;

        var loggerMock = new Mock<ILogger<GroupsRepository>>();
        var logger = loggerMock.Object;

        _mapper = new Mapper(new MapperConfiguration(expression =>
        {
            expression.AddProfile<InfrastructureApplicationMappingProfile>();
        }));
        
        _groupsDbSetMock = _dbContextMock.CreateDbSetMock(temp => temp.Groups, new List<GroupEntity>());
        
        _groupsRepository = new GroupsRepository(_dbContext, logger, _mapper);
    }
    
    #region GetAll

    [Fact]
    public async Task GetAll_ShouldReturnEmptyEnumerable_WhenNoGroupsExist()
    {
        // Arrange
        _dbContext.Groups.RemoveRange(_dbContext.Groups);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupsRepository.GetAll();

        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnAllGroups_WhenNoArgumentsIsApplied()
    {
        // Arrange
        var groups = _fixture.CreateMany<GroupEntity>().ToList();
        await _dbContext.Groups.AddRangeAsync(groups);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupsRepository.GetAll();
        
        // Assert
        result.Select(r => r.Id).Should().BeEquivalentTo(groups.Select(g => g.Id));
    }

    [Fact]
    public async Task GetAll_ShouldReturnFilteredGroups_WhenFilterIsApplied()
    {
        // Arrange
        List<string> names = ["Group", "Group1", "NotGroup", "Exercise", "Exercise1"];
        var enumerator = names.GetEnumerator();
        var groups = _fixture.Build<GroupEntity>()
            .With(g => g.Name, () =>
            {
                enumerator.MoveNext();
                return enumerator.Current;
            })
            .CreateMany(names.Count).ToList();
        await _dbContext.Groups.AddRangeAsync(groups);
        await _dbContext.SaveChangesAsync();

        var containableName = "Group";

        var filterModel = new FilterModel
        {
            { FilterOptionNames.Group.Name, containableName }
        };

        // Act
        var result = await _groupsRepository.GetAll(filterModel);

        // Assert
        var filteredGroupIds = groups.Where(g => g.Name.Contains(containableName)).Select(g => g.Id).ToList();
        result.Select(g => g.Id).Should().BeEquivalentTo(filteredGroupIds);
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnFilteredGroups_WhenOrderIsApplied()
    {
        // Arrange
        List<string> names = ["Group", "Group1", "NotGroup", "Exercise", "Exercise1"];
        var enumerator = names.GetEnumerator();
        var groups = _fixture.Build<GroupEntity>()
            .With(g => g.Name, () =>
            {
                enumerator.MoveNext();
                return enumerator.Current;
            })
            .CreateMany(names.Count).ToList();
        await _dbContext.Groups.AddRangeAsync(groups);
        await _dbContext.SaveChangesAsync();

        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.Group.Name,
            OrderOption = OrderOptionNames.Shared.Descending
        };

        // Act
        var result = await _groupsRepository.GetAll(orderModel:orderModel);

        // Assert
        result.Should().BeInDescendingOrder(g => g.Name);
        result.Select(g => g.Id).Should().BeEquivalentTo(groups.Select(g => g.Id));
        
    }

    [Fact]
    public async Task GetAll_ShouldReturnFilteredGroups_WhenPageIsApplied()
    {
        // Arrange
        List<string> names = ["Group", "Group1", "NotGroup", "Exercise", "Exercise1"];
        var enumerator = names.GetEnumerator();
        var groups = _fixture.Build<GroupEntity>()
            .With(g => g.Name, () =>
            {
                enumerator.MoveNext();
                return enumerator.Current;
            })
            .CreateMany(names.Count).ToList();
        await _dbContext.Groups.AddRangeAsync(groups);
        await _dbContext.SaveChangesAsync();

        var orderModel = new OrderModel
        {
            OrderBy = OrderOptionNames.Group.Name,
            OrderOption = OrderOptionNames.Shared.Ascending
        };

        int page = 1, pageSize = 2;
        var pageModel = new PageModel
        {
            CurrentPage = page,
            PageSize = pageSize
        };

        // Act
        var result = await _groupsRepository.GetAll(orderModel:orderModel, pageModel:pageModel);

        // Assert
        result.Count().Should().Be(pageSize);
        result.Should().BeInAscendingOrder(g => g.Name);

        var pagedGroups = groups.OrderBy(g => g.Name).Skip(page*pageSize).Take(pageSize).Select(g => g.Id);
        result.Select(g => g.Id).Should().BeEquivalentTo(pagedGroups);
        
    }
    
    [Fact]
    public async Task GetAll_ThrowsDataBaseException_WhenExceptionOccurs()
    {
        _dbContextMock.Setup(db => db.Groups).Throws(new Exception("Database error"));

        Func<Task> action = async () => await _groupsRepository.GetAll();

        await action.Should().ThrowAsync<DataBaseException>();
    }

    #endregion

    #region Count

    [Fact]
    public async Task Count_ShouldReturnGroupsCount_WhenNoFilterIsApplied()
    {
        // Arrange
        var groups = _fixture.Build<GroupEntity>().CreateMany().ToList();
        await _dbContext.Groups.AddRangeAsync(groups);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupsRepository.Count();

        // Assert
        result.Should().Be(groups.Count);
    }

    [Fact]
    public async Task Count_ShouldReturnZero_WhenBadFilterIsApplied()
    {
        // Arrange
        var groups = _fixture.Build<GroupEntity>().With(g => g.Name, "SomeGroup").Create();
        await _dbContext.Groups.AddAsync(groups);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupsRepository.Count(new FilterModel { { FilterOptionNames.Group.NameEquals, "UnexistsGroup" } });

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Count_ThrowsDataBaseException_WhenExceptionOccurs()
    {
        // Arrange
        _dbContextMock.Setup(db => db.Groups).Throws(new Exception("Database error"));

        // Act
        Func<Task> action = async () => await _groupsRepository.Count();

        // Assert
        await action.Should().ThrowAsync<DataBaseException>();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ShouldReturnGroup_WhenGroupIdExists()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var groupEntity = _fixture.Build<GroupEntity>().With(g => g.Id, groupId).Create();
        await _dbContext.Groups.AddAsync(groupEntity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupsRepository.GetById(groupId);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(groupId);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenGroupIdDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var groupEntity = _fixture.Build<GroupEntity>().With(g => g.Id, Guid.Empty).Create();
        await _dbContext.Groups.AddAsync(groupEntity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupsRepository.GetById(groupId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ThrowsDataBaseException_WhenExceptionOccurs()
    {
        // Arrange
        _dbContextMock.Setup(db => db.Groups).Throws(new Exception("Database error"));

        // Act
        Func<Task> action = async () => await _groupsRepository.GetById(Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<DataBaseException>();
    }

    #endregion

    #region CreateGroup

    [Fact]
    public async Task CreateGroup_ShouldReturnSuccess_WhenGroupIsAddedSuccessfully()
    {
        // Arrange
        var group = _fixture.Create<Group>();

        // Act
        var result = await _groupsRepository.Create(group);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Exception.Should().BeNull();
        result.Errors.Should().BeEmpty();
        var containableGroup = _groupsDbSetMock.Object.FirstOrDefault(g => g.Id == group.Id);
        containableGroup.Should().NotBeNull();
        _mapper.Map<Group>(containableGroup).Should().BeEquivalentTo(group);
    }

    [Fact]
    public async Task CreateGroup_ShouldThrowArgumentNullException_WhenGroupIsNull()
    {
        // Act
        Func<Task> action = async () => await _groupsRepository.Create(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateGroup_ShouldThrowArgumentException_WhenGroupNameIsNullOrWhiteSpace()
    {
        // Arrange
        var group = _fixture.Build<Group>().Without(g => g.Name).Create();

        // Act
        Func<Task> action = async () => await _groupsRepository.Create(group);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateGroup_ShouldReturnFailure_WhenGroupWithSameNameExists()
    {
        // Arrange
        var groupName = "GroupName";
        var existingGroup = _fixture.Build<GroupEntity>().With(g => g.Name, groupName).Create();
        
        await _dbContext.Groups.AddAsync(existingGroup);
        await _dbContext.SaveChangesAsync();

        var newGroup = _fixture.Build<Group>().With(g => g.Name, groupName).Create();

        // Act
        var result = await _groupsRepository.Create(newGroup);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<AlreadyExistsException>();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateGroup_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var group = _fixture.Create<Group>();
        group.Id = Guid.NewGuid();
        group.Name = "NewGroup";

        _dbContextMock.Setup(db => db.SaveChangesAsync(default)).Throws(new Exception("Database error"));

        // Act
        var result = await _groupsRepository.Create(group);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<DataBaseException>();
        result.Errors.Should().NotBeEmpty();
    }

    #endregion
    
    #region UpdateGroup
    
    [Fact]
    public async Task UpdateGroup_ShouldReturnSuccess_WhenGroupIsUpdatedSuccessfully()
    {
        // Arrange
        var existingGroup = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddAsync(existingGroup);
        await _dbContext.SaveChangesAsync();

        var updatedGroup = _fixture.Build<Group>().With(g =>g.Id, existingGroup.Id).Create();

        // Act
        var result = await _groupsRepository.Update(updatedGroup);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Exception.Should().BeNull();
        result.Errors.Should().BeEmpty();

        var updatedEntity = await _groupsDbSetMock.Object.FirstOrDefaultAsync(g => g.Id == updatedGroup.Id);
        updatedEntity.Should().NotBeNull();
        updatedEntity.Should().BeEquivalentTo(updatedGroup);
    }

    [Fact]
    public async Task UpdateGroup_ShouldThrowArgumentNullException_WhenGroupIsNull()
    {
        // Act
        Func<Task> action = async () => await _groupsRepository.Update(null);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateGroup_ShouldThrowArgumentException_WhenGroupNameIsNullOrWhiteSpace()
    {
        // Arrange
        var group = _fixture.Build<Group>().Without(g => g.Name).Create();

        // Act
        Func<Task> action = async () => await _groupsRepository.Update(group);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateGroup_ShouldReturnFailure_WhenGroupWithSameNameExists()
    {
        // Arrange
        var existingGroup1 = _fixture.Build<GroupEntity>().With(g => g.Name, "ExistingGroup").Create();
        var existingGroup2 = _fixture.Build<GroupEntity>().With(g => g.Name, "UpdatingGroup").Create();

        await _dbContext.Groups.AddRangeAsync(existingGroup1, existingGroup2);
        await _dbContext.SaveChangesAsync();

        var updatedGroup = new Group
        {
            Id = existingGroup2.Id,
            Name = existingGroup1.Name // Trying to update with a name that already exists
        };

        // Act
        var result = await _groupsRepository.Update(updatedGroup);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<AlreadyExistsException>();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateGroup_ShouldReturnFailure_WhenGroupNotFound()
    {
        // Arrange
        var updatedGroup = _fixture.Create<Group>();

        // Act
        var result = await _groupsRepository.Update(updatedGroup);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<NotFoundException>();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateGroup_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var existingGroup = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddAsync(existingGroup);
        await _dbContext.SaveChangesAsync();

        var updatedGroup = new Group
        {
            Id = existingGroup.Id,
            Name = "UpdatedGroup"
        };

        _dbContextMock.Setup(db => db.SaveChangesAsync(default)).Throws(new Exception("Database error"));

        // Act
        var result = await _groupsRepository.Update(updatedGroup);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<DataBaseException>();
        result.Errors.Should().NotBeEmpty();
    }

    #endregion
    
    #region DeleteGroup

    [Fact]
    public async Task DeleteGroup_ShouldReturnSuccess_WhenGroupIsDeletedSuccessfully()
    {
        // Arrange
        var existingGroup = _fixture.Create<GroupEntity>();
        await _dbContext.Groups.AddAsync(existingGroup);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupsRepository.Delete(existingGroup.Id);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Exception.Should().BeNull();
        result.Errors.Should().BeEmpty();

        var deletedEntity = await _groupsDbSetMock.Object.FirstOrDefaultAsync(g => g.Id == existingGroup.Id);
        deletedEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGroup_ShouldThrowNotFoundException_WhenGroupNotFound()
    {
        // Act
        var result = await _groupsRepository.Delete(Guid.NewGuid());

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<NotFoundException>();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteGroup_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var existingGroup = _fixture.Build<GroupEntity>().With(g => g.Id, Guid.NewGuid()).Create();
        await _dbContext.Groups.AddAsync(existingGroup);
        await _dbContext.SaveChangesAsync();

        _dbContextMock.Setup(db => db.SaveChangesAsync(default)).Throws(new Exception("Database error"));

        // Act
        var result = await _groupsRepository.Delete(existingGroup.Id);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Exception.Should().BeAssignableTo<DataBaseException>();
        result.Errors.Should().NotBeEmpty();
    }

    #endregion
}