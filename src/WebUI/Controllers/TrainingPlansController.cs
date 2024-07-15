using Application.Constants;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Domain.Models.TrainingPlan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Mappers;
using WebUI.ModelBinding.CustomModelBinders;
using WebUI.Models.ResponseModels;
using WebUI.Models.TrainingPlanModels;

namespace WebUI.Controllers;


[Controller]
[Route("training-plans")]
[Authorize]
public class TrainingPlansController : Controller
{
    private readonly ITrainingPlansService _trainingPlansService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TrainingPlansController(ITrainingPlansService trainingPlansService, UserManager<ApplicationUser> userManager)
    {
        _trainingPlansService = trainingPlansService;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAll(
        [FromQuery] OrderModel? orderModel,
        [ModelBinder(typeof(FilterModelBinder))] FilterModel? filterModel)
    {
        filterModel ??= new FilterModel();
        filterModel[FilterOptionNames.TrainingPlan.PublicOnly] = "true";
        var plans = await _trainingPlansService.GetAll(orderModel, filterModel);
        return View(plans.Select(p => p.ToTrainingPlanViewModel()));
    }

    [HttpGet("for-user")]
    public async Task<IActionResult> GetUserTrainingPlans(
        [FromQuery] OrderModel? orderModel,
        [ModelBinder(typeof(FilterModelBinder))]FilterModel? filterModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = "/training-plans/for-user" });
        
        if (filterModel is null)
        {
            filterModel = new FilterModel
            {
                { FilterOptionNames.TrainingPlan.Author, user.UserName }
            };
        }
        else
            filterModel[FilterOptionNames.TrainingPlan.Author] = user.UserName;

        var plans = await _trainingPlansService.GetAll(orderModel, filterModel);
        return View(plans.Select(p => p.ToTrainingPlanViewModel()));
    }

    [HttpGet("{author}/{title}")]
    public async Task<IActionResult> GetTrainingPlan(string author, string title)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = $"/training-plans/{author}/{title}" });
        
        var trainingPlan = await _trainingPlansService.GetByName(author, title);

        if (trainingPlan is null  || (!trainingPlan.IsPublic && trainingPlan.Author.Id != user.Id))
            return this.NotFoundView(new []{"Training plan was not found"});
        
        return View(trainingPlan.ToTrainingPlanViewModel());
    }

    [HttpGet("{author}/{title}/as-pdf")]
    public async Task<IActionResult> GetTrainingPlanAsPdf(string author, string title)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = $"/training-plans/{author}/{title}" });
        
        var trainingPlan = await _trainingPlansService.GetByName(author, title);

        if (trainingPlan is null  || (!trainingPlan.IsPublic && trainingPlan.Author.Id != user.Id))
            return this.NotFoundView(new []{"Training plan was not found"});

        return new ViewAsPdf("GetTrainingPlanAsPDF", trainingPlan.ToTrainingPlanViewModel(), ViewData)
        {
            PageMargins = new Margins(10, 5, 20, 5),
            PageSize = Size.A4,
            IsLowQuality = false,
        };
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = "/training-plans/create" });

        return View();
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateTrainingPlanModel creationModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = "/training-plans/create" });

        if (!ModelState.IsValid)
        {
            return View(creationModel);
        }

        var trainingPlan = new TrainingPlan
        {
            Title = creationModel.Title,
            Author = user,
            IsPublic = creationModel.IsPublic,
            TrainingPlanBlocks = []
        };
        
        var result = await _trainingPlansService.Create(trainingPlan);

        if (!result.IsSuccessful)
        {
            return this.ErrorView(500, new []{result.Exception?.Message ?? string.Empty });
        }
        
        return RedirectToAction("GetUserTrainingPlans", "TrainingPlans");
    }

    [HttpGet("{author}/{title}/update")]
    [TypeFilter(typeof(AddAvailableGroupsActionFilter))]
    public async Task<IActionResult> Update(string author, string title, [FromServices] IGroupsService groupsService)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts",
                new { returnUrl = $"/training-plans/{author}/{title}/update" });
        
        var trainingPlan = await _trainingPlansService.GetByName(author, title);
        
        if (trainingPlan is null)
            return this.NotFoundView(new[] { "Training plan was not found" });

        if (trainingPlan.Author.Id != user.Id)
            return this.ErrorView(StatusCodes.Status403Forbidden,
                new[] { "Only owner can update his/her training plan" });
        
        return View(new UpdateTrainingPlanModel
        {
            Id = trainingPlan.Id,
            NewTitle = trainingPlan.Title,
            IsPublic = trainingPlan.IsPublic,
            Blocks = trainingPlan.TrainingPlanBlocks.Select(b => new UpdateTrainingPlanBlockModel
            {
                Name = b.Name,
                Entries = b.TrainingPlanBlockEntries.Select(e => new UpdateTrainingPlanBlockEntryModel
                {
                    Description = e.Description,
                    GroupId = e.Group.Id
                }).ToList()
            }).ToList()
        });
    }

    [HttpPost("{author}/{title}/update")]
    public async Task<IActionResult> Update(string author, string title, [ModelBinder(typeof(UpdateTrainingPlanModelBinder))] UpdateTrainingPlanModel updateTrainingPlanModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = $"/training-plans/{author}/{title}/update" });

        var trainingPlan = await _trainingPlansService.GetByName(author, title);

        if (trainingPlan is null)
            return NotFound(ResponseModel.BadResponse(new[] { "Training plan was not found" }));
        
        if (user.Id != trainingPlan.Author.Id)
            return this.ErrorView(StatusCodes.Status403Forbidden, new[] { "Only author can edit training plan" });

        if (!ModelState.IsValid)
        {
            return Json(ResponseModel.BadResponse(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        var newTrainingPlan = new TrainingPlan
        {
            Id = trainingPlan.Id,
            Title = updateTrainingPlanModel.NewTitle,
            Author = user,
            IsPublic = updateTrainingPlanModel.IsPublic,
            TrainingPlanBlocks = updateTrainingPlanModel.Blocks.Select(b => new TrainingPlanBlock
            {
                Name = b.Name,
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
            return Ok(ResponseModel.GoodResponse(null));
        }
        
        if (result.Exception is AlreadyExistsException)
        {
            return BadRequest(ResponseModel.BadResponse(result.Errors));
        }
        
        if (result.Exception is NotFoundException)
        {
            return NotFound(ResponseModel.BadResponse(result.Errors));
        }

        return StatusCode(StatusCodes.Status500InternalServerError, ResponseModel.BadResponse(result.Errors));
    }
    
    [HttpGet("{author}/{title}/delete")]
    public async Task<IActionResult> Delete(string author, string title)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = $"/training-plans/{author}/{title}/update" });

        var plan = await _trainingPlansService.GetByName(author, title);

        if (plan is null)
            return this.NotFoundView(new[] { "Plan was not found" });

        if (plan.Author.Id != user.Id)
            return this.ErrorView(StatusCodes.Status403Forbidden,
                new[] { "Only owner can delete his/her training plan" });

        var result = await _trainingPlansService.Delete(plan.Id);

        if (!result.IsSuccessful)
        {
            return this.ErrorView(500, new []{result.Exception?.Message ?? string.Empty });
        }

        return RedirectToAction("GetUserTrainingPlans");
    }
}