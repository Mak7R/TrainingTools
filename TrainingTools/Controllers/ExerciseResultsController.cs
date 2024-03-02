
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
            var userId = HttpContext.GetIdFromSession();
            if (userId == null) return RedirectToAction("Login", "Users");

            using var scope = _scopeFactory.CreateScope();
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.Get(u => u.Id == userId);
            if (user == null) return View("Error", (404, "User was not found"));

            var exerciseResultsService = usersCollectionService.GetServiceForUser<IExerciseResultsService>(user);
            var results = new ExerciseResults { Id = Guid.NewGuid(), ExerciseId = exerciseId };
            await exerciseResultsService.Add(results);
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
        try
        {
            var userId = HttpContext.GetIdFromSession();
            if (userId == null) return RedirectToAction("Login", "Users");

            using var scope = _scopeFactory.CreateScope();
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.Get(u => u.Id == userId);
            if (user == null) return View("Error", (404, "User was not found"));

            var exerciseResultsService = usersCollectionService.GetServiceForUser<IExerciseResultsService>(user);
            var results = await exerciseResultsService.Get(r => r.Id == resultsId);
            if (results == null) return StatusCode(500);
            await exerciseResultsService.Remove(results);
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
            var userId = HttpContext.GetIdFromSession();
            if (userId == null) return RedirectToAction("Login", "Users");

            using var scope = _scopeFactory.CreateScope();
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.Get(u => u.Id == userId);
            if (user == null) return View("Error", (404, "User was not found"));

            var exerciseResultsService = usersCollectionService.GetServiceForUser<IExerciseResultsService>(user);
            await exerciseResultsService.Update(model.ExerciseResultsId, r =>
            {
                var currentResults = r.Results;
                var newResults = model.ExerciseResultsEntries;
            
                for (int i = 0; i < Math.Min(currentResults.Count, newResults.Count); i++)
                {
                    currentResults[i].Weight = newResults[i].Weight;
                    currentResults[i].Count = newResults[i].Count;
                }

                if (currentResults.Count > newResults.Count)
                {
                    currentResults.RemoveRange(newResults.Count, currentResults.Count - newResults.Count);
                }
                else if (currentResults.Count < newResults.Count)
                {
                    for (int i = currentResults.Count; i < newResults.Count; i++)
                    {
                        var result = new ExerciseResultEntry
                        {
                            Id = Guid.NewGuid(),
                            Weight = newResults[i].Weight,
                            Count = newResults[i].Count
                        };
                        currentResults.Add(result);
                    }
                }
            });
        }
        catch
        {
            return StatusCode(500);
        }

        return Ok();
    }
}