using Application.Interfaces.ServiceInterfaces;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Models.ExerciseModels;
using WebUI.Models.GroupModels;

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
    public async Task<IActionResult> GetAllExercises()
    {
        ViewBag.AvailableGroups =
            (await _groupsService.GetAll()).Select(g => new GroupViewModel { Id = g.Id, Name = g.Name });
        var exercises = await _exercisesService.GetAll();
        var exerciseViewModels = exercises.Select(e => new ExerciseViewModel { Id = e.Id, Name = e.Name, Group = new GroupViewModel {Id = e.Group.Id, Name = e.Group.Name}});
        return View(exerciseViewModels);
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpPost("add-exercise")]
    public async Task<IActionResult> AddExercise([FromForm] AddExerciseModel addExerciseModel)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequestView(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }
        
        var exercise = new Exercise{Name = addExerciseModel.Name, Group = new Group{Id = addExerciseModel.GroupId}};
        var result = await _exercisesService.CreateExercise(exercise);
        
        if (result.IsSuccessful) return RedirectToAction("GetAllExercises", "Exercises");
        
        if (result.ResultObject is AlreadyExistsException exception)
        {
            return this.BadRequestView([exception.Message]);
        }
        else
        {
            return this.ServerErrorView(500, result.Errors);
        }
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpPost("edit-exercise")]
    public async Task<IActionResult> EditExercise([FromForm] EditExerciseModel editExerciseModel)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequestView(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }
        var exercise = new Exercise{Id = editExerciseModel.Id, Name = editExerciseModel.Name, Group = new Group{Id = editExerciseModel.GroupId}};
        var result = await _exercisesService.UpdateExercise(exercise);
        
        if (result.IsSuccessful) return RedirectToAction("GetAllExercises", "Exercises");
        if (result.ResultObject is AlreadyExistsException alreadyExistsException)
        {
            return this.BadRequestView([alreadyExistsException.Message]);
        }
        else if (result.ResultObject is NotFoundException notFoundException)
        {
            return this.NotFoundView(notFoundException.Message);
        }
        else
        {
            return this.ServerErrorView(500, result.Errors);
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

