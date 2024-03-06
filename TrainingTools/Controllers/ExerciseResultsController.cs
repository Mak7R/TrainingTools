
using System.Text.Json;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthorizer;
using TrainingTools.Models;

namespace TrainingTools.Controllers;

[Route("api/workspaces/exercises/results")]
public class ExerciseResultsController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExerciseResultsController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Add([FromQuery] Guid exerciseId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
            try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
            catch (NotFoundException e) { return View("Error", (404, e.Message)); }

            var exerciseResultsService = scope.ServiceProvider.GetRequiredService<IExerciseResultsService>();
            var results = new ExerciseResults { Id = Guid.NewGuid(), ExerciseId = exerciseId, ResultsJson = JsonSerializer.Serialize(new ExerciseResultsObject())};
            await exerciseResultsService.Add(results);
            await authorizedUser.SaveChanges();
        }
        catch
        {
            return StatusCode(500);
        }
        return Ok();
    }
    
    [HttpDelete]
    [Route("")]
    public async Task<IActionResult> Delete([FromQuery] Guid resultsId)
    {
        throw new NotImplementedException();
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
            try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
            catch (NotFoundException e) { return View("Error", (404, e.Message)); }

            var exerciseResultsService = scope.ServiceProvider.GetRequiredService<IExerciseResultsService>();
            try
            {
                await exerciseResultsService.Remove(resultsId);
                await authorizedUser.SaveChanges();
            }
            catch (Exception e)
            {
                return View("Error", (500, e.Message));
            }
            
        }
        catch
        {
            return StatusCode(500);
        }
        return Ok();
    }
    
    [HttpPut]
    [Route("")]
    public async Task<IActionResult> Update([FromBody] UpdateExerciseResultsModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                    { 
                        message = string.Join
                        (
                            '\n', 
                            ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                        )
                    }
                );
            
            using var scope = _scopeFactory.CreateScope();
            var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
            try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
            catch (NotFoundException e) { return View("Error", (404, e.Message)); }

            var exerciseResultsService = scope.ServiceProvider.GetRequiredService<IExerciseResultsService>();

            await exerciseResultsService.Update(
                model.ExerciseResultsId, er =>
                {
                    er.ResultsJson = JsonSerializer.Serialize(model.ExerciseResultsModel);
                });
            await authorizedUser.SaveChanges();
        }
        catch
        {
            return StatusCode(500);
        }

        return Ok();
    }
}