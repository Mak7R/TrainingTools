using Application.Constants;
using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Models.Exercise;
using WebUI.Models.Shared;

namespace WebUI.Controllers;

[Controller]
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
    [QueryValuesReader<DefaultOrderOptions>]
    [AddAvailableGroups]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        FilterViewModel? filterModel,
        OrderViewModel? orderModel,
        PageViewModel? pageModel,
        [FromServices] IExerciseResultsService resultsService,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        pageModel ??= new PageViewModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            var defaultPageSize = 10;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        }

        ViewBag.ExercisesCount = await _exercisesService.Count(filterModel);

        var exercises = await _exercisesService.GetAll(filterModel, orderModel, pageModel);

        var user = await userManager.GetUserAsync(User);

        if (user is not null)
            ViewBag.UserResults = await resultsService.GetAll(new FilterModel
                { { FilterOptionNames.ExerciseResults.OwnerId, user.Id.ToString() } }); // todo not optimize

        return View(_mapper.Map<List<ExerciseViewModel>>(exercises));
    }

    [HttpGet("{exerciseId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid exerciseId)
    {
        var exercise = await _exercisesService.GetById(exerciseId);

        if (exercise is null)
            return this.NotFoundRedirect(["Exercise was not found"]);

        return View(_mapper.Map<ExerciseViewModel>(exercise));
    }

    [HttpPost("render-about-preview")]
    [AllowAnonymous]
    public async Task<IActionResult> RenderAboutPreview(string? about,
        [FromServices] IReferencedContentProvider referencedContentProvider)
    {
        return Content(await referencedContentProvider.ParseContentAsync(about));
    }

    [HttpGet("create")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    [AddAvailableGroups]
    public IActionResult Create()
    {
        return View();
    }

    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    [HttpPost("create")]
    [AddAvailableGroups]
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

    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    [HttpGet("{exerciseId:guid}/update")]
    [AddAvailableGroups]
    public async Task<IActionResult> Update(Guid exerciseId)
    {
        var exercise = await _exercisesService.GetById(exerciseId);
        if (exercise is null)
            return this.NotFoundRedirect(["Exercise was not found"]);

        return View(_mapper.Map<UpdateExerciseModel>(exercise));
    }

    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    [HttpPost("{exerciseId:guid}/update")]
    [AddAvailableGroups]
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

    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    [HttpGet("delete")]
    public async Task<IActionResult> DeleteExercise([FromQuery] Guid exerciseId)
    {
        var result = await _exercisesService.Delete(exerciseId);

        if (result.IsSuccessful) return RedirectToAction("GetAll", "Exercises");

        if (result.ResultObject is NotFoundException exception)
            return this.NotFoundRedirect([exception.Message]);

        return this.ErrorRedirect(500, result.Errors);
    }
}