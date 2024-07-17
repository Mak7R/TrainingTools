using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using AutoMapper;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.ModelBinding.CustomModelBinders;
using WebUI.Models.Exercise;
using WebUI.Models.Shared;

namespace WebUI.Controllers;

[Controller]
[AllowAnonymous]
[Route("exercises")]
public class ExercisesController : Controller
{
    private readonly IExercisesService _exercisesService;
    private readonly IMapper _mapper;

    public ExercisesController(IExercisesService exercisesService, IMapper mapper)
    {
        _exercisesService = exercisesService;
        _mapper = mapper;
    }
    
    [HttpGet("")]
    [TypeFilter(typeof(QueryValuesProvidingActionFilter), Arguments = new object[] { typeof(DefaultOrderOptions) })]
    [TypeFilter(typeof(AddAvailableGroupsActionFilter))]
    public async Task<IActionResult> GetAll(
        [FromQuery] OrderModel? orderModel,
        [ModelBinder(typeof(FilterModelBinder))] FilterModel? filterModel, 
        
        [FromServices] IExerciseResultsService resultsService, 
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var exercises = await _exercisesService.GetAll(orderModel, filterModel);

        var user = await userManager.GetUserAsync(User);

        if (user is not null)
            ViewBag.UserResults = await resultsService.GetForUser(user.Id);
        
        return View(_mapper.Map<List<ExerciseViewModel>>(exercises));
    }

    [HttpGet("{exerciseId:guid}")]
    public async Task<IActionResult> Get(Guid exerciseId)
    {
        var exercise = await _exercisesService.GetById(exerciseId);

        if (exercise is null) 
            return this.NotFoundView("Exercise was not found");
        
        return View(_mapper.Map<ExerciseViewModel>(exercise));
    }

    [HttpPost("render-about-preview")]
    public async Task<IActionResult> RenderAboutPreview(string? about, [FromServices] IReferencedContentProvider referencedContentProvider)
    {
        return Content(await referencedContentProvider.ParseContentAsync(about));
    }

    [HttpGet("create")]
    [Authorize(Roles = "Admin,Root")]
    [TypeFilter(typeof(AddAvailableGroupsActionFilter))]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpPost("create")]
    [TypeFilter(typeof(AddAvailableGroupsActionFilter))]
    public async Task<IActionResult> Create([FromForm] CreateExerciseModel createExerciseModel)
    {
        if (!ModelState.IsValid)
            return View(createExerciseModel);
        
        var result = await _exercisesService.Create(_mapper.Map<Exercise>(createExerciseModel));
        
        if (result.IsSuccessful) 
            return RedirectToAction("GetAll", "Exercises");
        
        foreach (var error in result.Errors)
            ModelState.AddModelError(nameof(UpdateExerciseModel), error);
            
        return View(createExerciseModel);
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("{exerciseId:guid}/update")]
    [TypeFilter(typeof(AddAvailableGroupsActionFilter))]
    public async Task<IActionResult> Update(Guid exerciseId)
    {
        var exercise = await _exercisesService.GetById(exerciseId);
        if (exercise is null) 
            return this.NotFoundView("Exercise was not found");
        
        return View(_mapper.Map<UpdateExerciseModel>(exercise));
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpPost("{exerciseId:guid}/update")]
    [TypeFilter(typeof(AddAvailableGroupsActionFilter))]
    public async Task<IActionResult> Update(Guid exerciseId, [FromForm] UpdateExerciseModel updateExerciseModel)
    {
        if (!ModelState.IsValid)
            return View(updateExerciseModel);
        
        var exercise = _mapper.Map<Exercise>(updateExerciseModel);
        exercise.Id = exerciseId;
        
        var result = await _exercisesService.Update(exercise);
        
        if (result.IsSuccessful) 
            return RedirectToAction("Get", "Exercises", new { exerciseId });
        
        foreach (var error in result.Errors)
            ModelState.AddModelError(nameof(UpdateExerciseModel), error);
        
        return View(updateExerciseModel);
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("delete")]
    public async Task<IActionResult> DeleteExercise([FromQuery] Guid exerciseId)
    {
        var result = await _exercisesService.Delete(exerciseId);

        if (result.IsSuccessful) return RedirectToAction("GetAll", "Exercises");

        if (result.ResultObject is NotFoundException exception)
            return this.NotFoundView(exception.Message);
        
        return this.ErrorView(500, result.Errors);
    }
}

