using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.Models.Exercise;
using WebUI.Models.Shared;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
public class ExercisesController : ApiController
{
    private readonly IExercisesService _exercisesService;
    private readonly IMapper _mapper;

    public ExercisesController(IExercisesService exercisesService, IMapper mapper)
    {
        _exercisesService = exercisesService;
        _mapper = mapper;
    }
    
    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        FilterViewModel? filterModel,
        OrderViewModel? orderModel,
        PageViewModel? pageModel)
    {
        var exercises = await _exercisesService.GetAll(filterModel, orderModel, pageModel);
        return Ok(_mapper.Map<List<ExerciseViewModel>>(exercises));
    }
    
    [HttpGet("count")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AllowAnonymous]
    public async Task<IActionResult> Count(FilterViewModel? filterModel)
    {
        return Ok(await _exercisesService.Count(filterModel));
    }

    [HttpGet("{exerciseId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid exerciseId)
    {
        var exercise = await _exercisesService.GetById(exerciseId);

        if (exercise is null) 
            return Problem(detail:"Exercise was not found", statusCode:404, title:"Not found");
        
        return Ok(_mapper.Map<ExerciseViewModel>(exercise));
    }

    [HttpGet("render-about-preview")]
    [AllowAnonymous]
    public async Task<IActionResult> RenderAboutPreview([FromBody] string? about, [FromServices] IReferencedContentProvider referencedContentProvider)
    {
        return Content(await referencedContentProvider.ParseContentAsync(about));
    }

    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    [HttpPost("")]
    [AddAvailableGroups]
    public async Task<IActionResult> Create(CreateExerciseModel createExerciseModel)
    {
        var result = await _exercisesService.Create(_mapper.Map<Exercise>(createExerciseModel));

        if (result.IsSuccessful)
        {
            if (result.ResultObject is Exercise exercise)
            {
                return CreatedAtAction("Get", "Exercises", new {exerciseId = exercise.Id}, exercise);
            }

            return Created();
        }
        
        if (result.Exception is AlreadyExistsException)
            return Problem("Exercise already exists", statusCode: StatusCodes.Status400BadRequest,
                title: "Already exists");

        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }

    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    [HttpPut("{exerciseId:guid}")]
    [AddAvailableGroups]
    public async Task<IActionResult> Update(Guid exerciseId, UpdateExerciseModel updateExerciseModel)
    {
        var exercise = _mapper.Map<Exercise>(updateExerciseModel);
        exercise.Id = exerciseId;
        
        var result = await _exercisesService.Update(exercise);
        
        if (result.IsSuccessful)
            return Ok(exercise);
        
        if (result.Exception is AlreadyExistsException)
            return Problem("Exercise with this name already exists", statusCode: StatusCodes.Status400BadRequest,
                title: "Already exists");
        
        if (result.ResultObject is NotFoundException)
            return Problem(detail:"Exercise was not found", statusCode:404, title:"Not found");
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }

    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    [HttpDelete("{exerciseId:guid}")]
    public async Task<IActionResult> DeleteExercise(Guid exerciseId)
    {
        var result = await _exercisesService.Delete(exerciseId);

        if (result.IsSuccessful)
        {
            if (result.ResultObject is Exercise exercise)
                return Ok(exercise);
            return Ok();
        }

        if (result.ResultObject is NotFoundException)
            return Problem(detail:"Exercise was not found", statusCode:404, title:"Not found");
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
}
