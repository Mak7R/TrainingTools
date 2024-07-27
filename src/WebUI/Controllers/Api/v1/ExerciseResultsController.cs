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
using WebUI.Filters;
using WebUI.Models.ExerciseResult;
using WebUI.Models.Shared;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
[AuthorizeVerifiedRoles]
[Route("api/v{version:apiVersion}/exercises/results")]
public class ExerciseResultsController : ApiController
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
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.ExerciseResults.OwnerId] = user.Id.ToString();
        
        var results = await _exerciseResultsService.GetAll(filterModel, orderModel, pageModel);

        return Ok(results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }
    
    [HttpGet("{userName}")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetUserResults(string userName, FilterViewModel? filterModel, OrderViewModel? orderModel, PageModel? pageModel)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

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
            return Problem(detail:"User was not found", statusCode:404, title:"Not found");

        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.ExerciseResults.OwnerId] = searchableUser.Id.ToString();
        
        var results = await _exerciseResultsService.GetAll(filterModel, orderModel, pageModel);
        
        return Ok(results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }
    
    [HttpGet("as-exel")]
    public async Task<IActionResult> GetUserResultsAsExcel([FromServices] IExerciseResultsToExсelExporter exсelExporter)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);
        
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
        FilterViewModel? filterModel, OrderViewModel? orderModel, PageViewModel? pageModel)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);
        
        filterModel ??= new FilterViewModel();
        filterModel[FilterOptionNames.ExerciseResults.ExerciseId] = exerciseId.ToString();
        
        var results = await _exerciseResultsService.GetOnlyUserAndFriendsResultForExercise(user.Id, exerciseId, filterModel, orderModel, pageModel);
        
        return Ok(results.Select(r => _mapper.Map<ExerciseResultViewModel>(r)));
    }  

    [HttpPost("{exerciseId:guid}")]
    public async Task<IActionResult> Create([FromRoute] Guid exerciseId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);
        
        var result = await _exerciseResultsService.Create(new ExerciseResult
            { Owner = user, Exercise = new Exercise { Id = exerciseId } });

        if (result.IsSuccessful)
        {
            if (result.ResultObject is ExerciseResult exerciseResult)
            {
                return CreatedAtAction("GetUserResults", "ExerciseResults", new RouteValueDictionary {{FilterOptionNames.ExerciseResults.ExerciseId,  exerciseResult.Exercise.Id}}, exerciseResult);
            }

            return Created();
        }

        if (result.Exception is AlreadyExistsException)
            return Problem("Exercise result already exists", statusCode: StatusCodes.Status400BadRequest,
                title: "Already exists");

        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
    
    [HttpDelete("{exerciseId:guid}")]
    public async Task<IActionResult> Delete(Guid exerciseId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);
        
        var result = await _exerciseResultsService.Delete(user.Id, exerciseId);

        if (result.IsSuccessful)
            Ok();

        if (result.ResultObject is NotFoundException)
            return Problem(detail:"Exercise result was not found", statusCode:404, title:"Not found");
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
    
    [HttpPut("{exerciseId:guid}")]
    public async Task<IActionResult> Update(Guid exerciseId, UpdateResultsModel updateResultsModel)
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
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);
        
        var exerciseResult = new ExerciseResult
        {
            Owner = user,
            Exercise = new Exercise { Id = exerciseId },
            ApproachInfos = updateResultsModel.ApproachInfos.Select(ai => new Approach(ai.Weight, ai.Count, ai.Comment)).ToList()
        };
        
        var result = await _exerciseResultsService.Update(exerciseResult);

        if (result.IsSuccessful)
            return RedirectToAction("GetUserResults");

        if (result.ResultObject is NotFoundException)
            return Problem(detail:"Exercise result was not found", statusCode:404, title:"Not found");

        if (result.Exception is AlreadyExistsException)
            return Problem("Exercise result already exists", statusCode: StatusCodes.Status400BadRequest,
                title: "Already exists");
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
}