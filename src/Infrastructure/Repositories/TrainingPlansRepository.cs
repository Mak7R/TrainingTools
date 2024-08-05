using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Models.Shared;
using AutoMapper;
using Domain.Defaults;
using Domain.Exceptions;
using Domain.Models;
using Domain.Models.TrainingPlan;
using Infrastructure.Data;
using Infrastructure.Entities.TrainingPlan;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Repositories;

public class TrainingPlansRepository : IRepository<TrainingPlan, Guid>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TrainingPlansRepository> _logger;
    private readonly IMapper _mapper;

    public TrainingPlansRepository(ApplicationDbContext dbContext, ILogger<TrainingPlansRepository> logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    private static readonly ReadOnlyDictionary<string, Func<string, Expression<Func<TrainingPlanEntity, bool>>>> TrainingPlanFilters =
        new(new Dictionary<string, Func<string, Expression<Func<TrainingPlanEntity, bool>>>>
        {
            { FilterOptionNames.TrainingPlan.Title, value => p => p.Title.Contains(value) },
            { FilterOptionNames.TrainingPlan.TitleEquals, value => p => p.Title == value},
            {
                FilterOptionNames.TrainingPlan.AuthorName,
                value => p => p.Author.UserName != null && p.Author.UserName.Contains(value)
            },
            {
                FilterOptionNames.TrainingPlan.AuthorNameEquals,
                value => p => p.Author.UserName != null && p.Author.UserName == value
            },
            { FilterOptionNames.TrainingPlan.AuthorId, value => Guid.TryParse(value, out var authorId) ? p => p.AuthorId == authorId : _ => false },
            { FilterOptionNames.TrainingPlan.PublicOnly, value => value == "true" ? p => p.IsPublic : p => true }
        });
    
    private static readonly
        ReadOnlyDictionary<OrderModel, Func<IQueryable<TrainingPlanEntity>, IQueryable<TrainingPlanEntity>>> TrainingPlanOrders =
            new(new Dictionary<OrderModel, Func<IQueryable<TrainingPlanEntity>, IQueryable<TrainingPlanEntity>>>
            {
                {new OrderModel{OrderBy = OrderOptionNames.TrainingPlan.Title, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(t => t.Title)},
                {new OrderModel{OrderBy = OrderOptionNames.TrainingPlan.Title, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(t => t.Title)},
                {new OrderModel{OrderBy = OrderOptionNames.TrainingPlan.AuthorName, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(t => t.Author.UserName).ThenBy(t => t.Title)},
                {new OrderModel{OrderBy = OrderOptionNames.TrainingPlan.AuthorName, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(t => t.Author.UserName).ThenBy(t => t.Title)}
            });
    
    public async Task<IEnumerable<TrainingPlan>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        try
        {
            var query = _dbContext.TrainingPlans
                .Include(plan => plan.Author)
                .Include(plan => plan.TrainingPlanBlocks)
                .ThenInclude(block => block.TrainingPlanBlockEntries)
                .ThenInclude(e => e.Group)
                .AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, TrainingPlanFilters);
            
            if (orderModel is not null)
                query = orderModel.Order(query, TrainingPlanOrders);

            if (pageModel is not null)
                query = pageModel.TakePage(query);
            
            return (await query.ToListAsync()).Select(entity => _mapper.Map<TrainingPlan>(entity));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all training plans");
            throw new DataBaseException("Error while receiving training plans from database", e);
        }
    }

    public async Task<TrainingPlan?> GetById(Guid id)
    {
        try
        {
            var plan = await _dbContext.TrainingPlans
                .AsNoTracking()
                .Include(plan => plan.Author)
                .Include(plan => plan.TrainingPlanBlocks)
                .ThenInclude(block => block.TrainingPlanBlockEntries)
                .ThenInclude(e => e.Group)
                
                .FirstOrDefaultAsync(plan => plan.Id == id);

            return plan == null ? null : _mapper.Map<TrainingPlan>(plan);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving training plan by id '{id}'", id);
            throw new DataBaseException("Error while receiving training plan from database", e);
        }
    }

    public async Task<int> Count(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.TrainingPlans.AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, TrainingPlanFilters);

            return await query.CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving training plans count by filter '{filter}' from database", filterModel);
            throw new DataBaseException("Error while receiving training plans count from database", e);
        }
    }

    public async Task<OperationResult> Create(TrainingPlan trainingPlan)
    {
        ArgumentNullException.ThrowIfNull(trainingPlan);
        
        var trainingPlanEntity = _mapper.Map<TrainingPlanEntity>(trainingPlan);

        try
        {
            var existsPlan = await _dbContext.TrainingPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Title == trainingPlan.Title && e.Author.Id == trainingPlan.Author.Id);

            if (existsPlan is not null)
                throw new AlreadyExistsException("Plan with this name already exists");

            await _dbContext.TrainingPlans.AddAsync(trainingPlanEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Training plan with name '{trainingPlanName}' already exist in database", trainingPlan.Title);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new training plan '{plan}' to database", trainingPlan.Title);
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding training plan to database", e));
        }

        return new DefaultOperationResult(true, trainingPlan);
    }

    public async Task<OperationResult> Update(TrainingPlan updatedPlan)
    {
        ArgumentNullException.ThrowIfNull(updatedPlan);

        try
        {
            var trainingPlanEntity = await _dbContext.TrainingPlans
                .Include(p => p.TrainingPlanBlocks)
                .ThenInclude(b => b.TrainingPlanBlockEntries)
                .FirstOrDefaultAsync(e => e.Id == updatedPlan.Id);

            if (trainingPlanEntity is null)
                throw new NotFoundException("Training plan was not found");
            
            var sameName = await _dbContext.TrainingPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id != trainingPlanEntity.Id && p.AuthorId == updatedPlan.Author.Id && p.Title == updatedPlan.Title);

            if (sameName is not null)
                throw new AlreadyExistsException("Plan with this name already exists");
            
            _mapper.Map(updatedPlan, trainingPlanEntity); // todo optimization with execute update
            
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogInformation(notFoundException, "Training plan with id '{trainingPlanId}' was not found", updatedPlan.Id);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogInformation(alreadyExistsException, "Training plan with name '{trainingPlan}' already exist in database", updatedPlan.Title);
            return DefaultOperationResult.FromException(alreadyExistsException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new training plan '{trainingPlanId}' to database", updatedPlan.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while updating training plan in database", e));
        }

        return new DefaultOperationResult(true, updatedPlan);
    }

    public async Task<OperationResult> Delete(Guid trainingPlanId)
    {
        TrainingPlan? trainingPlan;
        try
        {
            var trainingPlanEntity = await _dbContext.TrainingPlans.FirstOrDefaultAsync(plan => plan.Id == trainingPlanId);

            if (trainingPlanEntity is null)
                throw new NotFoundException("Training plan was not found");

            trainingPlan = _mapper.Map<TrainingPlan>(trainingPlanEntity);
            
            _dbContext.TrainingPlans.Remove(trainingPlanEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogInformation(notFoundException, "Training plan with id '{trainingPlanId} was not found'",
                trainingPlanId);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (Exception)
        {
            _logger.LogError("Error when deleting training plan with id '{trainingPlanId}'", trainingPlanId);
            return DefaultOperationResult.FromException(new DataBaseException("Error while deleting training plan"));
        }

        return new DefaultOperationResult(trainingPlan);
    }
}