
using System.Text.Json;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using TrainingTools.Extensions;
using TrainingTools.Models;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Route("api/v1/workspaces/exercises")]
public class ExerciseResultsController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExerciseResultsController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [HttpPost("{exerciseId:guid}/results")]
    public async Task<IActionResult> Add([FromRoute] Guid exerciseId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var exerciseResultsService = scope.ServiceProvider.GetRequiredService<IExerciseResultsService>();
        var results = new ExerciseResults
        {
            Id = Guid.NewGuid(), 
            ExerciseId = exerciseId, 
            ResultsJson = JsonSerializer.Serialize(new ExerciseResultsObject())
        };
        await exerciseResultsService.Add(results);
        await authorizedUser.SaveChanges();
        return Ok();
    }
    
    [HttpDelete("results/{resultsId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid resultsId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var exerciseResultsService = scope.ServiceProvider.GetRequiredService<IExerciseResultsService>();
        await exerciseResultsService.Remove(resultsId);
        await authorizedUser.SaveChanges();
        return Ok();
    }
    
    [HttpPut("results/{resultsId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid resultsId, [FromBody] UpdateExerciseResultsModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
            
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var exerciseResultsService = scope.ServiceProvider.GetRequiredService<IExerciseResultsService>();

        await exerciseResultsService.Update(
            resultsId, er =>
            {
                er.ResultsJson = JsonSerializer.Serialize(model.ExerciseResultsModel);
            });
        await authorizedUser.SaveChanges();

        return Ok();
    }
}