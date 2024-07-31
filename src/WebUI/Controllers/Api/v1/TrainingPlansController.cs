using Application.Constants;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Domain.Models.TrainingPlan;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.ModelBinding.ModelBinders;
using WebUI.Models.Shared;
using WebUI.Models.TrainingPlan;

namespace WebUI.Controllers.Api.v1;


[Route("api/v{version:apiVersion}/training-plans")]
[AuthorizeVerifiedRoles]
public class TrainingPlansController : ApiController
{
    private readonly ITrainingPlansService _trainingPlansService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public TrainingPlansController(ITrainingPlansService trainingPlansService, UserManager<ApplicationUser> userManager, IMapper mapper)
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
        var plans = await _trainingPlansService.GetAll(filterModel, orderModel, pageModel);
        return Ok(plans.Select(p => _mapper.Map<TrainingPlanViewModel>(p)));
    }
    
    [HttpGet("count")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> Count(FilterViewModel? filterModel)
    {
        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.TrainingPlan.PublicOnly] = "true";
        return Ok(await _trainingPlansService.Count(filterModel));
    }
    
    [HttpGet("my/count")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> UserPlansCount(FilterViewModel? filterModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);
        
        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.TrainingPlan.AuthorId] = user.Id.ToString();
        return Ok(await _trainingPlansService.Count(filterModel));
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

        var plans = await _trainingPlansService.GetAll(filterModel, orderModel, pageModel);
        return Ok(plans.Select(p => _mapper.Map<TrainingPlanViewModel>(p)));
    }

    [HttpGet("{planId:guid}")]
    public async Task<IActionResult> GetTrainingPlan(Guid planId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = $"/training-plans/{planId}" });
        
        var trainingPlan = await _trainingPlansService.GetById(planId);
            
        if (trainingPlan is null  || (!trainingPlan.IsPublic && trainingPlan.Author.Id != user.Id))
            return Problem(detail:"Training plan was not found", statusCode:404, title:"Not found");
        
        return Ok(_mapper.Map<TrainingPlanViewModel>(trainingPlan));
    }
    
    [HttpPost("")]
    public async Task<IActionResult> Create([FromForm] CreateTrainingPlanModel creationModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/training-plans/create" });

        var trainingPlan = new TrainingPlan
        {
            Title = creationModel.Title,
            Author = user,
            IsPublic = creationModel.IsPublic,
            TrainingPlanBlocks = []
        };
        
        var result = await _trainingPlansService.Create(trainingPlan);

        if (result.IsSuccessful)
        {
            
        }
        
        return RedirectToAction("GetUserTrainingPlans", "TrainingPlans");
    }

    [HttpPut("{planId:guid}")]
    public async Task<IActionResult> Update(Guid planId, [UpdateTrainingPlanModelBinder] UpdateTrainingPlanModel updateTrainingPlanModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid model",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Model state is not valid",
                Extensions = new Dictionary<string, object?>
                {
                    {"errors", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)}
                }
            });
        }
        
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = $"/training-plans/{planId}/update" });

        var trainingPlan = await _trainingPlansService.GetById(planId);

        if (trainingPlan is null)
            return Problem(detail:"Training plan was not found", statusCode:404, title:"Not found");

        if (user.Id != trainingPlan.Author.Id)
            return Problem("Only author can edit training plan", statusCode: StatusCodes.Status403Forbidden, title: "Forbidden");
        
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
                    Group = new Group{Id = e.GroupId}
                }).ToList()
            }).ToList()
        };
        
        var result = await _trainingPlansService.Update(newTrainingPlan);
        if (result.IsSuccessful)
        {
            if (result.ResultObject is TrainingPlan plan)
                return Ok(plan);
            return NoContent();
        }
        
        if (result.Exception is AlreadyExistsException)
            return Problem("Training plan with this name already exists", statusCode:400, title:"Training plan already exists");
        
        if (result.Exception is NotFoundException)
            return Problem("Training plan was not found in database", statusCode:404, title:"Training plan was not found");
        
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
    
    [HttpDelete("{planId:guid}")]
    public async Task<IActionResult> Delete(Guid planId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = $"/training-plans/{planId}/update" });

        var plan = await _trainingPlansService.GetById(planId);

        if (plan is null)
            return Problem(detail:"Training plan was not found", statusCode:404, title:"Not found");

        if (plan.Author.Id != user.Id)
            return Problem("Only author can delete training plan", statusCode: StatusCodes.Status403Forbidden, title: "Forbidden");

        var result = await _trainingPlansService.Delete(plan.Id);

        if (result.IsSuccessful)
            return Ok(plan);
        
        if (result.ResultObject is NotFoundException)
            return Problem(detail:"Training plan was not found", statusCode:404, title:"Not found");

        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
}