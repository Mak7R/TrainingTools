using Application.Constants;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Mappers;
using WebUI.ModelBinding.CustomModelBinders;

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
        [ModelBinder(typeof(FilterModelBinder))]FilterModel? filterModel)
    {
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

    [HttpGet("{author}/{name}")]
    public async Task<IActionResult> GetTrainingPlan(string author, string name)
    {
        var plan = await _trainingPlansService.GetByName(author, name);

        if (plan is null)
            return this.NotFoundView(new []{"Training plan was not found"});
        
        return View(plan.ToTrainingPlanViewModel());
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
    public async Task<IActionResult> Create(string creationModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = "/training-plans/create" });

        if (!ModelState.IsValid)
        {
            return View(creationModel);
        }

        var result = await _trainingPlansService.Create(null!); // todo

        if (!result.IsSuccessful)
        {
            return this.ErrorView(500, new []{result.Exception?.Message ?? string.Empty });
        }
        
        return RedirectToAction("GetUserTrainingPlans", "TrainingPlans");
    }

    [HttpGet("{author}/{name}/update")]
    public async Task<IActionResult> Update(string author, string name)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = $"/training-plans/{author}/{name}/update" });

        var trainingPlan = await _trainingPlansService.GetByName(author, name);
        
        // trainingPlan.ToViewModel();

        return View();
    }
    
    [HttpPost("{author}/{name}/update")]
    public async Task<IActionResult> Update(string author, string name, string updateModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = $"/training-plans/{author}/{name}/update" });

        var result = await _trainingPlansService.Update(null!); // todo

        if (!result.IsSuccessful)
        {
            return this.ErrorView(500, new []{result.Exception?.Message ?? string.Empty });
        }
        
        return RedirectToAction("GetTrainingPlan", "TrainingPlans", new { author, name });
    }

    [HttpPost("{author}/{name}/delete")]
    public async Task<IActionResult> Delete(string author, string name)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Accounts", new { returnUrl = $"/training-plans/{author}/{name}/update" });

        var plan = await _trainingPlansService.GetByName(author, name);

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
    
    [HttpGet("is-name-free")]
    public async Task<IActionResult> IsNameFree(string name)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Json(false);

        var plans = await _trainingPlansService.GetAll(filterModel: new FilterModel
        {
            { FilterOptionNames.TrainingPlan.AuthorId, user.Id.ToString() },
            { FilterOptionNames.TrainingPlan.NameEquals, name }
        });

        return Json(!plans.Any());
    }
}