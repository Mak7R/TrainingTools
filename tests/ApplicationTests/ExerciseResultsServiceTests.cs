using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Shared;
using Application.Services;
using AutoFixture;
using Domain.Defaults;
using Domain.Models;
using Domain.Models.Friendship;
using Domain.Rules;
using FluentAssertions;
using Moq;

namespace ApplicationTests;

public class ExerciseResultsServiceTests
{
    private readonly Mock<IRepository<ExerciseResult, (Guid, Guid)>> _exerciseResultsRepositoryMock;
    private readonly Mock<IRepository<Friendship, (Guid, Guid)>> _friendshipRepositoryMock;
    private readonly IExerciseResultsService _exerciseResultsService;
    private readonly IFixture _fixture;

    public ExerciseResultsServiceTests()
    {
        _exerciseResultsRepositoryMock = new Mock<IRepository<ExerciseResult, (Guid, Guid)>>();
        _friendshipRepositoryMock = new Mock<IRepository<Friendship, (Guid, Guid)>>();
        _exerciseResultsService = new ExerciseResultsService(_exerciseResultsRepositoryMock.Object, _friendshipRepositoryMock.Object);
        _fixture = new Fixture();
    }

    
}