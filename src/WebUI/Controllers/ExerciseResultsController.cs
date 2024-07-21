using System.Globalization;
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
using Microsoft.EntityFrameworkCore;
using WebUI.Extensions;
using WebUI.Filters;
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
    public async Task<IActionResult> GetUserResults(OrderModel? orderModel, FilterModel? filterModel, PageModel? pageModel)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        pageModel ??= new PageModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            int defaultPageSize = 9;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        }

        filterModel ??= new FilterModel();
        filterModel[FilterOptionNames.ExerciseResults.OwnerNameEquals] = user.UserName;
        ViewBag.ExerciseResultsCount = await _exerciseResultsService.Count(filterModel);
        
        var results = await _exerciseResultsService.GetForUser(user.UserName ?? string.Empty, filterModel, orderModel, pageModel);
        
        return View(results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }
    
    [HttpGet("as-exel")]
    public async Task<IActionResult> GetUserResultsAsExcel([FromServices] IExerciseResultsToExсelExporter exсelExporter)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var results = await _exerciseResultsService.GetForUser(user.UserName ?? string.Empty);
        var stream = await exсelExporter.ToExсel(results);
        
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "results.xlsx");
    }
    
    [HttpGet("for-exercise/{exerciseId:guid}")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetFriendsResultsForExercise(Guid exerciseId, [FromServices] IExercisesService exercisesService,
        FilterModel? filterModel, OrderModel? orderModel, PageModel? pageModel)
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
        
        pageModel ??= new PageModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            int defaultPageSize = 9;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        } 
        filterModel ??= new FilterModel();
        filterModel[FilterOptionNames.ExerciseResults.FullNameEquals] = $"{exercise.Group.Name}/{exercise.Name}";
        ViewBag.ExerciseResultsCount = await _exerciseResultsService.Count(filterModel);
        
        var results = await _exerciseResultsService.GetOnlyUserAndFriendsResultForExercise(user, exercise.Group.Name ?? string.Empty, exercise.Name ?? string.Empty, filterModel, orderModel, pageModel);
        return View(results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }  
    
    [HttpGet("for-user/{userName}")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetResultsForUser(string userName, FilterModel? filterModel, OrderModel? orderModel, PageModel? pageModel)
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
        
        pageModel ??= new PageModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            int defaultPageSize = 9;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        }

        filterModel ??= new FilterModel();
        filterModel[FilterOptionNames.ExerciseResults.OwnerNameEquals] = searchableUser.UserName;
        ViewBag.ExerciseResultsCount = await _exerciseResultsService.Count(filterModel);
        
        var results = await _exerciseResultsService.GetForUser(searchableUser.UserName ?? string.Empty, filterModel, orderModel, pageModel);
        ViewBag.UserName = userName;
        return View("GetUserResults", results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }

    [HttpGet("add/{exerciseId:guid}")]
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
            return this.BadRequestView(result.Errors);

        return this.ErrorView(500, result.Errors);
    }
    
    [HttpGet("delete/{exerciseId:guid}")]
    public async Task<IActionResult> Delete(Guid exerciseId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = "/exercises/results"});
        
        var result = await _exerciseResultsService.Delete(user.Id, exerciseId);

        if (result.IsSuccessful)
            return RedirectToAction("GetUserResults");

        if (result.Exception is NotFoundException)
            return this.NotFoundView(result.Errors);
        
        return  this.ErrorView(500, result.Errors);
    }
    
    [HttpGet("update/{exerciseId:guid}")]
    public async Task<IActionResult> Update(Guid exerciseId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {returnUrl = $"/exercises/results/update/{exerciseId}"});

        var result = await _exerciseResultsService.GetById(user.Id, exerciseId);
        if (result is null) return this.NotFoundView("Result was not found");

        return View(_mapper.Map<ExerciseResultViewModel>(result));
    }
    
    [HttpPost("update/{exerciseId:guid}")]
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
            return this.NotFoundView(result.Errors);

        if (result.Exception is AlreadyExistsException)
            return this.BadRequestView(result.Errors);
        
        return this.ErrorView(StatusCodes.Status500InternalServerError, result.Errors);
    }
}