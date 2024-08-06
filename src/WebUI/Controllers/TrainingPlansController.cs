using Application.Constants;
using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Domain.Models.TrainingPlan;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.ModelBinding.ModelBinders;
using WebUI.Models.Shared;
using WebUI.Models.TrainingPlan;

namespace WebUI.Controllers;

[Controller]
[Route("training-plans")]
[AuthorizeVerifiedRoles]
public class TrainingPlansController : Controller
{
    private readonly IMapper _mapper;
    private readonly ITrainingPlansService _trainingPlansService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TrainingPlansController(ITrainingPlansService trainingPlansService, UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _trainingPlansService = trainingPlansService;
        _userManager = userManager;
        _mapper = mapper;
    }

    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetAll(
        FilterViewModel? filterModel,
        OrderViewModel? orderModel,
        PageViewModel? pageModel)
    {
        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.TrainingPlan.PublicOnly] = "true";

        pageModel ??= new PageViewModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            var defaultPageSize = 10;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        }

        ViewBag.TrainingPlansCount = await _trainingPlansService.Count(filterModel);

        var plans = await _trainingPlansService.GetAll(filterModel, orderModel, pageModel);
        return View(plans.Select(p => _mapper.Map<TrainingPlanViewModel>(p)));
    }

    [HttpGet("for-user")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetUserTrainingPlans(
        FilterViewModel? filterModel,
        OrderViewModel? orderModel,
        PageViewModel? pageModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/training-plans/for-user" });

        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.TrainingPlan.AuthorName] = user.UserName;

        pageModel ??= new PageViewModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            var defaultPageSize = 10;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        }

        ViewBag.TrainingPlansCount = await _trainingPlansService.Count(filterModel);

        var plans = await _trainingPlansService.GetAll(filterModel, orderModel, pageModel);
        return View(plans.Select(p => _mapper.Map<TrainingPlanViewModel>(p)));
    }

    [HttpGet("{planId:guid}")]
    public async Task<IActionResult> GetTrainingPlan(Guid planId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = $"/training-plans/{planId}" });

        var trainingPlan = await _trainingPlansService.GetById(planId);

        if (trainingPlan is null || (!trainingPlan.IsPublic && trainingPlan.Author.Id != user.Id))
            return this.NotFoundRedirect(new[] { "Training plan was not found" });

        return View(_mapper.Map<TrainingPlanViewModel>(trainingPlan));
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/training-plans/create" });

        return View();
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateTrainingPlanModel creationModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/training-plans/create" });

        if (!ModelState.IsValid) return View(creationModel);

        var trainingPlan = new TrainingPlan
        {
            Title = creationModel.Title,
            Author = user,
            IsPublic = creationModel.IsPublic,
            TrainingPlanBlocks = []
        };

        var result = await _trainingPlansService.Create(trainingPlan);

        if (!result.IsSuccessful) return this.ErrorRedirect(500, new[] { result.Exception?.Message ?? string.Empty });

        return RedirectToAction("GetUserTrainingPlans", "TrainingPlans");
    }

    [HttpGet("{planId:guid}/update")]
    [AddAvailableGroups]
    public async Task<IActionResult> Update(Guid planId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account",
                new { returnUrl = $"/training-plans/{planId}/update" });

        var trainingPlan = await _trainingPlansService.GetById(planId);

        if (trainingPlan is null)
            return this.NotFoundRedirect(new[] { "Training plan was not found" });

        if (trainingPlan.Author.Id != user.Id)
            return this.ErrorRedirect(StatusCodes.Status403Forbidden,
                new[] { "Only owner can update his/her training plan" });

        return View(_mapper.Map<UpdateTrainingPlanModel>(trainingPlan));
    }

    [HttpPost("{planId:guid}/update")]
    public async Task<IActionResult> Update(Guid planId,
        [UpdateTrainingPlanModelBinder] UpdateTrainingPlanModel updateTrainingPlanModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid model",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Model state is not valid",
                Extensions = new Dictionary<string, object?>
                {
                    { "errors", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                }
            });

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = $"/training-plans/{planId}/update" });

        var trainingPlan = await _trainingPlansService.GetById(planId);

        if (trainingPlan is null)
            return Problem("Training plan was not found in database", statusCode: StatusCodes.Status404NotFound);

        if (user.Id != trainingPlan.Author.Id)
            return this.ErrorRedirect(StatusCodes.Status403Forbidden, new[] { "Only author can edit training plan" });

        var newTrainingPlan = new TrainingPlan
        {
            Id = trainingPlan.Id,
            Title = updateTrainingPlanModel.NewTitle,
            Author = user,
            IsPublic = updateTrainingPlanModel.IsPublic,
            TrainingPlanBlocks = updateTrainingPlanModel.Blocks.Select(b => new TrainingPlanBlock
            {
                Title = b.Title,
                TrainingPlanBlockEntries = b.Entries.Select(e => new TrainingPlanBlockEntry
                {
                    Description = e.Description,
                    Group = new Group { Id = e.GroupId }
                }).ToList()
            }).ToList()
        };
        var result = await _trainingPlansService.Update(newTrainingPlan);

        if (result.IsSuccessful)
            return Ok(newTrainingPlan);

        if (result.Exception is AlreadyExistsException)
            return Problem("Training plan with this name already exists", statusCode: StatusCodes.Status400BadRequest);

        if (result.Exception is NotFoundException)
            return Problem("Training plan was not found in database", statusCode: StatusCodes.Status404NotFound);

        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Detail = "An internal server error occurred while processing the request",
            Status = StatusCodes.Status500InternalServerError,
            Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }
        });
    }

    [HttpGet("{planId:guid}/delete")]
    public async Task<IActionResult> Delete(Guid planId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = $"/training-plans/{planId}/update" });

        var plan = await _trainingPlansService.GetById(planId);

        if (plan is null)
            return this.NotFoundRedirect(new[] { "Plan was not found" });

        if (plan.Author.Id != user.Id)
            return this.ErrorRedirect(StatusCodes.Status403Forbidden,
                new[] { "Only owner can delete his/her training plan" });

        var result = await _trainingPlansService.Delete(plan.Id);

        if (!result.IsSuccessful) return this.ErrorRedirect(500, new[] { result.Exception?.Message ?? string.Empty });

        return RedirectToAction("GetUserTrainingPlans");
    }
}