using Application.Interfaces.Services;
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
    
    /// <summary>
    /// Retrieves a list of all exercises, with optional filtering, ordering, and pagination.
    /// </summary>
    /// <param name="filterModel">Supported filters: f_name, f_name-equals, f_group</param>
    /// <param name="orderModel">Supported orders: name, group-name</param>
    /// <param name="pageModel">Paging params: page, page-size</param>
    /// <returns>List of exercises matching the criteria</returns>
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
    
    /// <summary>
    /// Counts the total number of exercises based on optional filter criteria.
    /// </summary>
    /// <param name="filterModel">Supported filters: f_name, f_name-equals, f_group</param>
    /// <returns>Total count of exercises matching the criteria</returns>
    [HttpGet("count")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AllowAnonymous]
    public async Task<IActionResult> Count(FilterViewModel? filterModel)
    {
        return Ok(await _exercisesService.Count(filterModel));
    }

    /// <summary>
    /// Retrieves a specific exercise by its ID.
    /// </summary>
    /// <param name="exerciseId">ID of the exercise</param>
    /// <returns>The exercise with the specified ID</returns>
    [HttpGet("{exerciseId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid exerciseId)
    {
        var exercise = await _exercisesService.GetById(exerciseId);

        if (exercise is null) 
            return Problem(detail:"Exercise was not found", statusCode:404, title:"Not found");
        
        return Ok(_mapper.Map<ExerciseViewModel>(exercise));
    }

    /// <summary>
    /// Renders a preview of the "about" content, parsed for references.
    /// </summary>
    /// <param name="about">Content to be parsed</param>
    /// <param name="referencedContentProvider">Provider for parsing referenced content</param>
    /// <returns>Rendered content as plain text</returns>
    [HttpGet("render-about-preview")]
    [AllowAnonymous]
    public async Task<IActionResult> RenderAboutPreview([FromBody] string? about, [FromServices] IReferencedContentProvider referencedContentProvider)
    {
        return Content(await referencedContentProvider.ParseContentAsync(about));
    }

    /// <summary>
    /// Creates a new exercise with the specified details.
    /// </summary>
    /// <param name="createExerciseModel">Model containing details of the new exercise</param>
    /// <returns>Created exercise or an error response</returns>
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

    /// <summary>
    /// Updates the details of an existing exercise.
    /// </summary>
    /// <param name="exerciseId">ID of the exercise to update</param>
    /// <param name="updateExerciseModel">Model containing updated details of the exercise</param>
    /// <returns>Updated exercise or an error response</returns>
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

    /// <summary>
    /// Deletes an existing exercise by its ID.
    /// </summary>
    /// <param name="exerciseId">ID of the exercise to delete</param>
    /// <returns>No content or an error response</returns>
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
