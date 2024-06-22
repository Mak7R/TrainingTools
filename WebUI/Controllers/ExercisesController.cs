using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Exceptions;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Mappers;
using WebUI.ModelBinding.CustomModelBinders;
using WebUI.Models.ExerciseModels;
using WebUI.Models.SharedModels;

namespace WebUI.Controllers;

[Controller]
[AllowAnonymous]
[Route("exercises")]
public class ExercisesController : Controller
{
    private readonly IExercisesService _exercisesService;
    private readonly IGroupsService _groupsService;

    public ExercisesController(IExercisesService exercisesService, IGroupsService groupsService)
    {
        _exercisesService = exercisesService;
        _groupsService = groupsService;
    }
    
    [HttpGet("")]
    [TypeFilter(typeof(QueryValuesProvidingActionFilter), Arguments = new object[] { typeof(DefaultOrderOptions) })]
    public async Task<IActionResult> GetAllExercises(
        [FromQuery] OrderModel? orderModel,
        [ModelBinder(typeof(FilterModelBinder))]FilterModel? filterModel, 
        
        [FromServices] IExerciseResultsService resultsService, 
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        ViewBag.AvailableGroups = (await _groupsService.GetAll(new OrderModel{Order = "ASC", OrderBy = "name"}))
            .Select(g => g.ToGroupViewModel());
        
        var exercises = await _exercisesService.GetAll(orderModel, filterModel);
        var exerciseViewModels = exercises.Select(e => e.ToExerciseViewMode());

        var user = await userManager.GetUserAsync(User);

        if (user is not null)
            ViewBag.UserResults = await resultsService.GetForUser(user.Id);
        
        return View(exerciseViewModels);
    }

    [HttpGet("{exerciseId:guid}")]
    public async Task<IActionResult> GetExercise(Guid exerciseId)
    {
        var exercise = await _exercisesService.GetById(exerciseId);

        if (exercise is null) return this.NotFoundView("Exercise was not found");
        
        return View(exercise.ToExerciseViewMode());
    }

    [HttpGet("add-exercise")]
    [Authorize(Roles = "Admin,Root")]
    public async Task<IActionResult> AddExercise()
    {
        ViewBag.AvailableGroups = (await _groupsService.GetAll(new OrderModel{Order = "ASC", OrderBy = "name"}))
            .Select(g => g.ToGroupViewModel());
        return View();
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpPost("add-exercise")]
    public async Task<IActionResult> AddExercise([FromForm] AddExerciseModel addExerciseModel)
    {
        if (!ModelState.IsValid)
            return View(addExerciseModel);
        
        var exercise = addExerciseModel.ToExercise();
        var result = await _exercisesService.CreateExercise(exercise);
        
        if (result.IsSuccessful) 
            return RedirectToAction("GetAllExercises", "Exercises");
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(UpdateExerciseModel), error);
            }
            
            return View(addExerciseModel);
        }
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("{exerciseId:guid}/update")]
    public async Task<IActionResult> UpdateExercise(Guid exerciseId)
    {
        var exercise = await _exercisesService.GetById(exerciseId);
        if (exercise is null) return this.NotFoundView("Exercise was not found");
        
        ViewBag.AvailableGroups = (await _groupsService.GetAll(new OrderModel{Order = "ASC", OrderBy = "name"}))
                .Select(g => g.ToGroupViewModel());
        return View(new UpdateExerciseModel{Name = exercise.Name, GroupId = exercise.Group.Id, About = exercise.About});
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpPost("{exerciseId:guid}/update")]
    public async Task<IActionResult> UpdateExercise(Guid exerciseId, [FromForm] UpdateExerciseModel updateExerciseModel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AvailableGroups = (await _groupsService.GetAll(new OrderModel{Order = "ASC", OrderBy = "name"}))
                .Select(g => g.ToGroupViewModel());
            return View(updateExerciseModel);
        }
            
        
        var exercise = updateExerciseModel.ToExercise();
        exercise.Id = exerciseId;
        var result = await _exercisesService.UpdateExercise(exercise);
        
        if (result.IsSuccessful) return RedirectToAction("GetExercise", "Exercises", new {exerciseId});
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(UpdateExerciseModel), error);
            }
            
            ViewBag.AvailableGroups = (await _groupsService.GetAll(new OrderModel{Order = "ASC", OrderBy = "name"}))
                .Select(g => g.ToGroupViewModel());
            return View(updateExerciseModel);
        }
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("delete-exercise")]
    public async Task<IActionResult> DeleteExercise([FromQuery] Guid exerciseId)
    {
        var result = await _exercisesService.DeleteExercise(exerciseId);

        if (result.IsSuccessful) return RedirectToAction("GetAllExercises", "Exercises");

        if (result.ResultObject is NotFoundException exception)
        {
            return this.NotFoundView(exception.Message);
        }
        else
        {
            return this.ServerErrorView(500, result.Errors);
        }
    }
}

