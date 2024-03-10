using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using TrainingTools.Extensions;
using TrainingTools.Models;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Route("api/v1/workspaces")]
public class ExercisesController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExercisesController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet("{workspaceId:guid}/[controller]")]
    public async Task<IActionResult> Index(
        [FromRoute] Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        var exercises = (await exercisesService.GetAll()).Where(e => e.Workspace.Id == workspaceId);
        
        return Json(new ExercisesViewCollectionBuilder(exercises).Filter(filter).Order(order).Build());
    }

    [HttpPost("{workspaceId:guid}/[controller]")]
    public async Task<IActionResult> Add([FromRoute] Guid workspaceId, [FromBody] AddExerciseModel exerciseModel)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = exerciseModel.Name,
            GroupId = exerciseModel.GroupId,
            WorkspaceId = workspaceId
        };

        await exercisesService.Add(exercise);
        await authorizedUser.SaveChanges();

        return Ok();
    }

    [HttpGet("exercises/{exerciseId:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid exerciseId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        var exercise = await exercisesService.Get(e => e.Id == exerciseId);
        if (exercise == null) return NotFound(new ErrorViewModel("Exercise was not found"));

        var resultsService = scope.ServiceProvider.GetRequiredService<IExerciseResultsService>();
        var results = await resultsService.Get(r => r.Exercise.Id == exercise.Id);
        
        return Json(new FullExerciseViewModel(exercise, results));
    }
    
    [HttpDelete("exercises/{exerciseId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid exerciseId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        
        await exercisesService.Remove(exerciseId);
        await authorizedUser.SaveChanges();

        return Ok();
    }

    [HttpPatch("exercises/{exerciseId:guid}")]
    public async Task<IActionResult> Edit([FromRoute] Guid exerciseId, [FromBody] EditExerciseModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

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