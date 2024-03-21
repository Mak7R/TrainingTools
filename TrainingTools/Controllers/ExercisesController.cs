using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Services;
using TrainingTools.Extensions;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Route("api/v1/workspaces/{workspaceId:guid}/[controller]")]
public class ExercisesController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExercisesController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> GetAll(
        [FromRoute] Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var selected = scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>();
        await selected.Select(workspaceId);
        
        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        var exercises = await exercisesService.GetAll();
        
        return Json(new ExercisesViewCollectionBuilder(exercises.Select(e => e.ToExerciseViewModel(selected.Permission))).Filter(filter).Order(order).Build());
    }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromRoute] Guid workspaceId, [FromBody] AddExerciseModel exerciseModel)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        
        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();

        var exercise = new Exercise
        {
            Name = exerciseModel.Name,
            GroupId = exerciseModel.GroupId,
        };

        await exercisesService.Add(exercise);
        await authorizedUser.SaveChanges();

        return Ok();
    }

    [HttpGet("{exerciseId:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid exerciseId,[FromRoute] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        var selected = scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>();
        await selected.Select(workspaceId);
        
        
        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        var exercise = await exercisesService.Get(e => e.Id == exerciseId);
        if (exercise == null) return NotFound(new ErrorViewModel("Exercise was not found"));

        var resultsService = scope.ServiceProvider.GetRequiredService<IExerciseResultsService>();
        var results = await resultsService.Get(r => r.ExerciseId == exerciseId);
        var allResults = await resultsService.GetRange(r=> r.ExerciseId == exerciseId);
        
        return Json(exercise.ToFullExerciseViewModel(results, allResults, selected.Permission));
    }
    
    [HttpDelete("{exerciseId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid exerciseId,[FromRoute] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        

        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        
        await exercisesService.Remove(exerciseId);
        await authorizedUser.SaveChanges();

        return Ok();
    }

    [HttpPatch("{exerciseId:guid}")]
    public async Task<IActionResult> Edit([FromRoute] Guid exerciseId, [FromBody] EditExerciseModel model,[FromRoute] Guid workspaceId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        

        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        
        await exercisesService.Update(exerciseId, e =>
        {
            e.Name = model.Name;
            e.GroupId = model.GroupId;
        });
        await authorizedUser.SaveChanges();
        
        var exercise = await exercisesService.Get(e => e.Id == exerciseId);
        if (exercise == null) return NotFound(new ErrorViewModel("Exercise was not found"));

        return Ok();
    }
}