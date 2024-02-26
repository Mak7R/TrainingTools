using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthorizer;
using TrainingTools.Models;

namespace TrainingTools.Controllers;

[Route("/workspaces/{workspaceId:guid}/exercises")]
public class ExerciseController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExerciseController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index(
        [Required][FromRoute] Guid workspaceId,
        FilterModel filter, 
        SortBindingModel sorter)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "User");

        IEnumerable<Exercise> exercises;
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.GetUserAsync(u => u.Id == userId);
            
            if (user == null) return View("Error", (404, "User was not found"));

            var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);

            exercises = await exercisesService.GetExercisesAsync();
        }
        
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
                (nameof(WorkspaceViewModel.Name), "A-Z") => 
                    exerciseViewModels.OrderBy(w => w.Name),
                (nameof(WorkspaceViewModel.Name), "Z-A") => 
                    exerciseViewModels.OrderBy(w => w.Name).Reverse(),
                (nameof(WorkspaceViewModel.Id), "ASCENDING") => 
                    exerciseViewModels.OrderBy(w => w.Id),
                (nameof(WorkspaceViewModel.Id), "DESCENDING") => 
                    exerciseViewModels.OrderBy(w => w.Id).Reverse(),
                _ => exerciseViewModels
            };
        }
        
        return View(exerciseViewModels);
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult Add()
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "User");
        
        return View();
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Add([Required][FromRoute] Guid workspaceId, [FromForm] AddExerciseModel exerciseModel)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "User");

        using (var scope = _scopeFactory.CreateScope())
        {
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.GetUserAsync(u => u.Id == userId);
            
            if (user == null) return View("Error", (404, "User was not found"));

            var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);

            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = exerciseModel.Name,
                WorkspaceId = workspaceId
            };

            await exercisesService.AddAsync(exercise);
        }

        return RedirectToAction("Index", "Exercise", new {workspaceId});
    }

    [HttpGet]
    [Route("{exerciseId:guid}")]
    public async Task<IActionResult> Get([Required] Guid exerciseId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "User");

        Exercise? exercise;
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.GetUserAsync(u => u.Id == userId);
            
            if (user == null) return View("Error", (404, "User was not found"));

            var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);
            
            exercise = await exercisesService.GetExerciseAsync(e => e.Id == exerciseId);
        }

        if (exercise == null) return View("Error", (404, "Workspace was not found"));
        
        return View(new ExerciseViewModel(exercise));
    }
    
    [HttpGet]
    [Route("[action]")]
    public IActionResult Delete()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    [Route("[action]")]
    public IActionResult Delete(int model)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult Edit()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult Edit(int model)
    {
        throw new NotImplementedException();
    }
}