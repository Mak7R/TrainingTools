﻿using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthorizer;
using TrainingTools.Models;

namespace TrainingTools.Controllers;

[Route("workspaces/[controller]/{exerciseId:guid}")]
public class ExercisesController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExercisesController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet]
    [Route("/workspaces/{workspaceId:guid}/[controller]")]
    public async Task<IActionResult> Index(
        [Required, FromRoute] Guid workspaceId,
        FilterModel filter, 
        SortBindingModel sorter)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);

        var exercises = await exercisesService.GetAll();

        var exerciseViewModels = exercises
            .Where(e => e.Workspace.Id == workspaceId)
            .Select(e => new ExerciseViewModel(e));
        
        if (filter.HasFilters)
        {
            ViewBag.SearchBy = filter.SearchBy!;
            ViewBag.SearchValue = filter.SearchValue!;

            exerciseViewModels = exerciseViewModels.Where(filter.SearchBy switch
            {
                nameof(ExerciseViewModel.Id) => e => e.Id.ToString().Contains(filter.SearchValue!),
                nameof(ExerciseViewModel.Name) => e => e.Name.Contains(filter.SearchValue!),
                _ => _ => false
            });
        }

        if (sorter.HasSorters)
        {
            ViewBag.SortBy = sorter.SortBy!;
            ViewBag.SortingOption = sorter.SortingOption!;
            
            exerciseViewModels = (sorter.SortBy, sorter.SortingOption) switch
            {
                (nameof(ExerciseViewModel.Name), "A-Z") => 
                    exerciseViewModels.OrderBy(w => w.Name),
                (nameof(ExerciseViewModel.Name), "Z-A") => 
                    exerciseViewModels.OrderBy(w => w.Name).Reverse(),
                (nameof(ExerciseViewModel.Id), "ASCENDING") => 
                    exerciseViewModels.OrderBy(w => w.Id),
                (nameof(ExerciseViewModel.Id), "DESCENDING") => 
                    exerciseViewModels.OrderBy(w => w.Id).Reverse(),
                _ => exerciseViewModels
            };
        }
        
        return View(exerciseViewModels);
    }

    [HttpGet]
    [Route("/workspaces/{workspaceId:guid}/[controller]/[action]")]
    public async Task<IActionResult> Add([Required, FromRoute] Guid workspaceId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");
        
        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return View("Error", (404, "User was not found"));

        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);
        var groups = (await groupsService.GetAll()).Where(g => g.Workspace.Id == workspaceId);
        
        return View(new AddExerciseModel {Groups = groups.Select(g => new GroupViewModel(g))});
    }

    [HttpPost]
    [Route("/workspaces/{workspaceId:guid}/[controller]/[action]")]
    public async Task<IActionResult> Add([Required, FromRoute] Guid workspaceId, [FromForm] AddExerciseModel exerciseModel)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = exerciseModel.Name,
            GroupId = exerciseModel.GroupId,
            WorkspaceId = workspaceId
        };

        await exercisesService.Add(exercise);
        await usersCollectionService.SaveChanges();

        return RedirectToAction("Index", new {workspaceId});
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Get([FromRoute, Required] Guid exerciseId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);
        var exercise = await exercisesService.Get(e => e.Id == exerciseId);
        if (exercise == null) return View("Error", (404, "Group was not found"));

        var resultsService = usersCollectionService.GetServiceForUser<IExerciseResultsService>(user);
        var results = await resultsService.Get(r => r.Exercise.Id == exercise.Id);
        
        return View(new FullExerciseViewModel(exercise, results));
    }
    
    [HttpGet, HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Delete([FromRoute, Required] Guid exerciseId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return View("Error", (404, "User was not found"));

        var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);
        var exercise = await exercisesService.Get(e => e.Id == exerciseId);
        try
        {
            await exercisesService.Remove(exerciseId);
            await usersCollectionService.SaveChanges();
        }
        catch(Exception e)
        {
            return View("Error", (500, e.Message));
        }

        return RedirectToAction("Index", new {workspaceId = exercise!.Workspace.Id});
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> Edit([FromRoute, Required] Guid exerciseId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return View("Error", (404, "User was not found"));

        var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);
        var exercise = await exercisesService.Get(e => e.Id == exerciseId);
        if (exercise == null) return View("Error", (404, "Exercise was not found"));
        
        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);
        var groups = (await groupsService.GetAll()).Where(g => g.Workspace.Id == exercise.Workspace.Id);
        
        return View(new EditExerciseModel{Name = exercise.Name, GroupId = exercise.GroupId, Groups = groups.Select(g => new GroupViewModel(g))});
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Edit([FromRoute, Required] Guid exerciseId, [FromForm, Required] EditExerciseModel model)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return View("Error", (404, "User was not found"));

        var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);
        
        await exercisesService.Update(exerciseId, e =>
        {
            e.Name = model.Name;
            e.GroupId = model.GroupId;
        });
        await usersCollectionService.SaveChanges();
        
        var exercise = await exercisesService.Get(e => e.Id == exerciseId);
        if (exercise == null) return View("Error", (404, "Exercise was not found"));

        return RedirectToAction("Index", new {workspaceId = exercise.Workspace.Id});
    }
}