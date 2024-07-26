using System.Globalization;
using Application.Constants;
using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Models.Exercise;
using WebUI.Models.ExerciseResult;
using WebUI.Models.Shared;

namespace WebUI.Controllers;

[Controller]
[AuthorizeVerifiedRoles]
[Route("exercises/results")]
public class ExerciseResultsController : Controller
{
    private readonly IExerciseResultsService _exerciseResultsService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFriendsService _friendsService;
    private readonly IMapper _mapper;

    public ExerciseResultsController(IExerciseResultsService exerciseResultsService, UserManager<ApplicationUser> userManager, IFriendsService friendsService, IMapper mapper)
    {
        _exerciseResultsService = exerciseResultsService;
        _userManager = userManager;
        _friendsService = friendsService;
        _mapper = mapper;
    }

    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetUserResults(FilterViewModel? filterModel, OrderViewModel? orderModel, PageViewModel? pageModel)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        pageModel ??= new PageViewModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            int defaultPageSize = 9;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        }

        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.ExerciseResults.OwnerId] = user.Id.ToString();
        
        var results = await _exerciseResultsService.GetAll(filterModel, orderModel, pageModel);
        ViewBag.ExerciseResultsCount = await _exerciseResultsService.Count(filterModel);

        return View(results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }
    
    [HttpGet("{userName}")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetUserResults(string userName, FilterViewModel? filterModel, OrderViewModel? orderModel, PageModel? pageModel)
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
            return this.NotFoundRedirect(["User was not found"]);
        
        pageModel ??= new PageModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            int defaultPageSize = 9;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        }

        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.ExerciseResults.OwnerId] = searchableUser.Id.ToString();
        
        var results = await _exerciseResultsService.GetAll(filterModel, orderModel, pageModel);
        
        ViewBag.ExerciseResultsCount = await _exerciseResultsService.Count(filterModel);
        ViewBag.UserName = userName;
        
        return View(results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }
    
    [HttpGet("as-exel")]
    public async Task<IActionResult> GetUserResultsAsExcel([FromServices] IExerciseResultsToExсelExporter exсelExporter)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var filterModel = new FilterModel
        {
            {FilterOptionNames.ExerciseResults.OwnerId, user.Id.ToString()}
        };
        var results = await _exerciseResultsService.GetAll(filterModel);
        var stream = await exсelExporter.ToExсel(results);
        
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "results.xlsx");
    }
    
    [HttpGet("for-exercise/{exerciseId:guid}")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetFriendsResultsForExercise([FromRoute] Guid exerciseId, 
        FilterViewModel? filterModel, OrderViewModel? orderModel, PageViewModel? pageModel, 
        [FromServices] IExercisesService exercisesService)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = $"/exercises/results/for-exercise/{exerciseId}"});
        
        var exercise = await exercisesService.GetById(exerciseId);

        if (exercise is null)
            return this.NotFoundRedirect(["Exercise was not found"]);

        ViewBag.Exercise = _mapper.Map<ExerciseViewModel>(exercise);
        
        pageModel ??= new PageViewModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            int defaultPageSize = 9;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        } 
        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.ExerciseResults.ExerciseId] = exerciseId.ToString();
        
        var results = await _exerciseResultsService.GetOnlyUserAndFriendsResultForExercise(user.Id, exerciseId, filterModel, orderModel, pageModel);
        ViewBag.ExerciseResultsCount = await _exerciseResultsService.Count(filterModel);
        
        return View(results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }  

    [HttpGet("{exerciseId:guid}/create")]
    public async Task<IActionResult> Create([FromRoute] Guid exerciseId, [FromQuery] string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var result = await _exerciseResultsService.Create(new ExerciseResult
            { Owner = user, Exercise = new Exercise { Id = exerciseId } });

        if (result.IsSuccessful)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) 
                return LocalRedirect(returnUrl);
            return RedirectToAction("GetUserResults");
        }

        if (result.Exception is AlreadyExistsException)
            return this.BadRequestRedirect(result.Errors);

        return this.ErrorRedirect(500, result.Errors);
    }
    
    [HttpGet("{exerciseId:guid}/delete")]
    public async Task<IActionResult> Delete(Guid exerciseId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var result = await _exerciseResultsService.Delete(user.Id, exerciseId);

        if (result.IsSuccessful)
            return RedirectToAction("GetUserResults");

        if (result.Exception is NotFoundException)
            return this.NotFoundRedirect(result.Errors);
        
        return  this.ErrorRedirect(500, result.Errors);
    }
    
    [HttpGet("{exerciseId:guid}/update")]
    public async Task<IActionResult> Update(Guid exerciseId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = $"/exercises/results/update/{exerciseId}"});

        var result = await _exerciseResultsService.GetById(user.Id, exerciseId);
        if (result is null) return this.NotFoundRedirect(["Result was not found"]);

        return View(_mapper.Map<ExerciseResultViewModel>(result));
    }
    
    [HttpPost("{exerciseId:guid}/update")]
    public async Task<IActionResult> Update(Guid exerciseId, [FromForm] UpdateResultsModel updateResultsModel)
    {
        // todo custom model binder and validator for update results model
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
        
        var exerciseResult = new ExerciseResult
        {
            Owner = user,
            Exercise = new Exercise { Id = exerciseId },
            ApproachInfos = updateResultsModel.ApproachInfos.Select(ai => new Approach(ai.Weight, ai.Count, ai.Comment)).ToList()
        };
        
        var result = await _exerciseResultsService.Update(exerciseResult);

        if (result.IsSuccessful)
            return RedirectToAction("GetUserResults");

        if (result.Exception is NotFoundException)
            return this.NotFoundRedirect(result.Errors);

        if (result.Exception is AlreadyExistsException)
            return this.BadRequestRedirect(result.Errors);
        
        return this.ErrorRedirect(StatusCodes.Status500InternalServerError, result.Errors);
    }
}