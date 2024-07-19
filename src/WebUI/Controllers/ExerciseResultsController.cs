using System.Globalization;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Enums;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Mapping.Mappers;
using WebUI.ModelBinding.ModelBinders;
using WebUI.Models.Exercise;
using WebUI.Models.ExerciseResult;
using WebUI.Models.Group;
using WebUI.Models.Shared;

namespace WebUI.Controllers;

[Controller]
[Authorize]
[Route("exercises/results")]
public class ExerciseResultsController : Controller
{
    private readonly IExerciseResultsService _exerciseResultsService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFriendsService _friendsService;

    public ExerciseResultsController(IExerciseResultsService exerciseResultsService, UserManager<ApplicationUser> userManager, IFriendsService friendsService)
    {
        _exerciseResultsService = exerciseResultsService;
        _userManager = userManager;
        _friendsService = friendsService;
    }

    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetUserResults(OrderModel? orderModel,[FilterModelBinder] FilterModel? filterModel)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var results = await _exerciseResultsService.GetForUser(user.Id, orderModel, filterModel);
        
        return View(results.Select(r => r.ToExerciseResultViewModel()));
    }
    
    [HttpGet("as-exel")]
    public async Task<IActionResult> GetUserResultsAsExcel([FromServices] IExerciseResultsToExсelExporter exсelExporter)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var results = await _exerciseResultsService.GetForUser(user.Id);
        var stream = await exсelExporter.ToExсel(results);
        
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "results.xlsx");
    }
    
    [HttpGet("for-exercise/{exerciseId:guid}")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetFriendsResultsForExercise(Guid exerciseId, [FromServices] IExercisesService exercisesService,
        OrderModel? orderModel,[FilterModelBinder] FilterModel? filterModel)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = $"/exercises/results/for-exercise/{exerciseId}"});

        var exercise = await exercisesService.GetById(exerciseId);

        if (exercise is null)
            return this.NotFoundView("Exercise was not found");

        ViewBag.Exercise = new ExerciseViewModel
        {
            Id = exercise.Id, Name = exercise.Name,
            Group = new GroupViewModel { Id = exercise.Group.Id, Name = exercise.Group.Name }
        };
        
        var results = await _exerciseResultsService.GetOnlyUserAndFriendsResultForExercise(user.Id, exerciseId, orderModel, filterModel);
        return View(results.Select(r => r.ToExerciseResultViewModel()));
    }  
    
    [HttpGet("for-user/{userName}")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetResultsForUser(string userName, OrderModel? orderModel,[FilterModelBinder] FilterModel? filterModel)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});

        if (user.UserName == userName)
            return RedirectToAction("GetUserResults");
        
        var isAdminOrRoot = await _userManager.IsInRoleAsync(user, nameof(Role.Admin)) ||
                            await _userManager.IsInRoleAsync(user, nameof(Role.Root));
        
        ApplicationUser? searchableUser;
        if (isAdminOrRoot)
        {
            searchableUser =
                await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }
        else
        {
            var friendsIds =  (await _friendsService.GetFriendsFor(user)).Select(u => u.Id);
            searchableUser = await _userManager.Users.FirstOrDefaultAsync(u =>
                u.UserName == userName && (u.IsPublic || friendsIds.Contains(u.Id)));
        }

        if (searchableUser is null)
            return this.NotFoundView("User was not found");
        
        var results = await _exerciseResultsService.GetForUser(searchableUser.Id, orderModel, filterModel);
        ViewBag.UserName = userName;
        return View("GetUserResults", results.Select(r => r.ToExerciseResultViewModel()));
    }

    [HttpGet("add/{exerciseId:guid}")]
    public async Task<IActionResult> Create([FromRoute] Guid exerciseId, [FromQuery] string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var result = await _exerciseResultsService.Create(new ExerciseResult
            { Owner = user, Exercise = new Exercise { Id = exerciseId } });
        
        if (!result.IsSuccessful) return this.BadRequestView(result.Errors);
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) 
            return LocalRedirect(returnUrl);
        return RedirectToAction("GetUserResults");
    }
    
    [HttpGet("delete/{exerciseId:guid}")]
    public async Task<IActionResult> Delete(Guid exerciseId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var result = await _exerciseResultsService.Delete(user.Id, exerciseId);

        return result.IsSuccessful ? RedirectToAction("GetUserResults") : this.BadRequestView(result.Errors);
    }
    
    [HttpGet("update/{exerciseId:guid}")]
    public async Task<IActionResult> Update(Guid exerciseId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = $"/exercises/results/update/{exerciseId}"});

        var result = await _exerciseResultsService.Get(user.Id, exerciseId);
        if (result is null) return this.NotFoundView("Result was not found");

        return View(result.ToExerciseResultViewModel());
    }
    
    [HttpPost("update/{exerciseId:guid}")]
    public async Task<IActionResult> Update(Guid exerciseId, [FromForm] UpdateResultsModel updateResultsModel)
    {
        int i = 0;
        foreach (var approach in updateResultsModel.ApproachInfos)
        {
            if (approach.Weight == 0)
            {
                if (Request.Form.TryGetValue($"ApproachInfos[{i}].Weight", out var value))
                {
                    if (decimal.TryParse(value.First() ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out var weightInv))
                    {
                        approach.Weight = weightInv;
                    }
                    else if (decimal.TryParse(value.First() ?? "0", NumberStyles.Float, CultureInfo.CurrentCulture, out var weightCur))
                    {
                        approach.Weight = weightCur;
                    }
                }
            }
            
            i++;
        }
        
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = $"/exercises/results/update/{exerciseId}"});
        
        var result = new ExerciseResult
        {
            Owner = user,
            Exercise = new Exercise { Id = exerciseId },
            ApproachInfos = updateResultsModel.ApproachInfos.Select(ai => new Approach(ai.Weight, ai.Count, ai.Comment)).ToList()
        };
        
        var operationResult = await _exerciseResultsService.Update(result);

        if (operationResult.IsSuccessful)
        {
            return RedirectToAction("GetUserResults");
        }
        else
        {
            return this.ErrorView(StatusCodes.Status500InternalServerError, operationResult.Errors);
        }
    }
}