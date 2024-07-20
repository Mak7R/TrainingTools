using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Application.Constants;
using Application.Interfaces.Repositories;
using Application.Models.Shared;
using AutoMapper;
using Domain.Defaults;
using Domain.Exceptions;
using Domain.Models;
using Domain.Models.Friendship;
using Infrastructure.Data;
using Infrastructure.Entities.Friendship;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class FriendshipsRepository : IRepository<Friendship, (Guid FirstFriendId, Guid SecondFriendId)>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FriendshipsRepository> _logger;
    private readonly IMapper _mapper;
    
    public FriendshipsRepository(ApplicationDbContext dbContext, ILogger<FriendshipsRepository> logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }
    
    private static readonly
        ReadOnlyDictionary<string, Func<string, Expression<Func<FriendshipEntity, bool>>>> FriendshipFilters = new(
            new Dictionary<string, Func<string, Expression<Func<FriendshipEntity, bool>>>>
            {
                { FilterOptionNames.Relationships.Friendship.FriendId, value => Guid.TryParse(value, out var friendId) ? f => f.FirstFriendId == friendId || f.SecondFriendId == friendId : _ => false },
            });
    
    private static readonly
        ReadOnlyDictionary<OrderModel, Func<IQueryable<FriendshipEntity>, IQueryable<FriendshipEntity>>> FriendshipOrders =
            new(new Dictionary<OrderModel, Func<IQueryable<FriendshipEntity>, IQueryable<FriendshipEntity>>>
            {
                {new OrderModel{OrderBy = OrderOptionNames.Relationships.Friendship.FriendsFromDateTime, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(f => f.FriendsFrom)},
                {new OrderModel{OrderBy = OrderOptionNames.Relationships.Friendship.FriendsFromDateTime, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(f => f.FriendsFrom)}
            });
    
    public async Task<IEnumerable<Friendship>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        try
        {
            var query = _dbContext.Friendships
                .Include(f => f.FirstFriend)
                .Include(f => f.SecondFriend)
                .AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, FriendshipFilters);

            if (orderModel is not null)
                query = orderModel.Order(query, FriendshipOrders);

            if (pageModel is not null)
                query = pageModel.TakePage(query);
            
            return await query.Select(e => _mapper.Map<Friendship>(e)).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all friendships");
            throw new DataBaseException("Error while receiving friendships from database", e);
        }
    }

    public async Task<Friendship?> GetById((Guid FirstFriendId, Guid SecondFriendId) id)
    {
        try
        {
            var friendshipEntity = await _dbContext.Friendships
                .AsNoTracking()
                .Include(f => f.FirstFriend)
                .Include(f => f.SecondFriend)
                
                .FirstOrDefaultAsync(f =>
                    (f.FirstFriendId == id.FirstFriendId && f.FirstFriendId == id.SecondFriendId) ||
                    (f.SecondFriendId == id.SecondFriendId && f.SecondFriendId == id.FirstFriendId));
            
            return friendshipEntity == null ? null : _mapper.Map<Friendship>(friendshipEntity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving friendship by id '{id}' from database", id);
            throw new DataBaseException("Error while receiving friendship from database", e);
        }
    }

    public async Task<int> Count(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.Friendships.AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, FriendshipFilters);

            return await query.CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving friendships count by filter '{filter}' from database", filterModel);
            throw new DataBaseException("Error while receiving friendships count from database", e);
        }
    }

    public async Task<OperationResult> Create(Friendship friendship)
    {
        ArgumentNullException.ThrowIfNull(friendship);
        ArgumentNullException.ThrowIfNull(friendship.FirstFriend);
        ArgumentNullException.ThrowIfNull(friendship.SecondFriend);
        if (friendship.FirstFriend.Id == friendship.SecondFriend.Id)
            throw new ArgumentException("Friends id cannot be ths same");
        
        var friendshipEntity = _mapper.Map<FriendshipEntity>(friendship);
        try
        {
            var existFriendship = await _dbContext.Friendships
                .AsNoTracking()
                .FirstOrDefaultAsync(fr =>
                (fr.FirstFriendId == friendship.FirstFriend.Id && fr.SecondFriendId == friendship.SecondFriend.Id) ||
                (fr.FirstFriendId == friendship.SecondFriend.Id && fr.SecondFriendId == friendship.FirstFriend.Id));

            if (existFriendship is not null)
                throw new AlreadyExistsException("These users already are friends");

            await _dbContext.Friendships.AddAsync(friendshipEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogError("Users '{invitor}' and '{target}' already are friends", friendship.FirstFriend.Id, friendship.SecondFriend.Id);
            return DefaultOperationResult.FromException(alreadyExistsException); // 
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new friendship (Invitor: '{friend1}' Target: {friend2}) to database", friendship.FirstFriend.Id, friendship.SecondFriend.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding new friendship", e));
        }

        return new DefaultOperationResult(friendship);
    }

    public async Task<OperationResult> Update(Friendship friendship)
    {
        try
        {
            var friendshipEntity = await _dbContext.Friendships
                .FirstOrDefaultAsync(fi => 
                    (fi.FirstFriendId == friendship.FirstFriend.Id && fi.SecondFriendId == friendship.SecondFriend.Id) || 
                    (fi.FirstFriendId == friendship.SecondFriend.Id && fi.SecondFriendId == friendship.FirstFriend.Id));
            
            if (friendshipEntity is null)
            {
                _logger.LogError("Friendship ({friend1}; {friend2}) was not found", friendship.FirstFriend.Id, friendship.SecondFriend.Id);
                return DefaultOperationResult.FromException(new NotFoundException("Friendship was not found"));
            }

            _mapper.Map(friendship, friendshipEntity);
            
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating friendship (Invitor:'{friend1}' Target:{friend2}) to database", friendship.FirstFriend.Id, friendship.SecondFriend.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while updating friend invitation", e));
        }

        return new DefaultOperationResult(friendship);
    }

    public async Task<OperationResult> Delete((Guid FirstFriendId, Guid SecondFriendId) id)
    {
        Friendship? friendship;
        try
        {
            var friendshipEntity = await _dbContext.Friendships
                .FirstOrDefaultAsync(fr =>
                (fr.FirstFriendId == id.FirstFriendId && fr.SecondFriendId == id.SecondFriendId) ||
                (fr.FirstFriendId == id.SecondFriendId && fr.SecondFriendId == id.FirstFriendId));

            if (friendshipEntity is null)
                throw new NotFoundException("Friendship was not found");

            friendship = _mapper.Map<Friendship>(friendshipEntity);

            _dbContext.Friendships.Remove(friendshipEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogInformation("Users '{user1}' and '{user2}' friendship was not found", id.FirstFriendId, id.SecondFriendId);
            return DefaultOperationResult.FromException(notFoundException);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while removing friendship for users with id '{firstFriend}' and '{secondFriend}' from database", id.FirstFriendId, id.SecondFriendId);
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding removing friendship", e));
        }

        return new DefaultOperationResult(friendship);
    }
}