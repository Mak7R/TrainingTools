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

public class FriendInvitationsRepository : IRepository<FriendInvitation, (Guid InvitorId, Guid InvitedId)>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FriendInvitationsRepository> _logger;
    private readonly IMapper _mapper;
    
    public FriendInvitationsRepository(ApplicationDbContext dbContext, ILogger<FriendInvitationsRepository> logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    private static readonly
        ReadOnlyDictionary<string, Func<string, Expression<Func<FriendInvitationEntity, bool>>>> FriendInvitationFilters = new(
            new Dictionary<string, Func<string, Expression<Func<FriendInvitationEntity, bool>>>>
            {
                { FilterOptionNames.Relationships.FriendInvitation.Invited, value => Guid.TryParse(value, out var invitedId) ? e => e.InvitedId == invitedId : _ => false },
                { FilterOptionNames.Relationships.FriendInvitation.Invitor, value => Guid.TryParse(value, out var invitorId) ? e => e.InvitorId == invitorId : _ => false }
            });
    
    private static readonly
        ReadOnlyDictionary<OrderModel, Func<IQueryable<FriendInvitationEntity>, IQueryable<FriendInvitationEntity>>> FriendInvitationOrders =
            new(new Dictionary<OrderModel, Func<IQueryable<FriendInvitationEntity>, IQueryable<FriendInvitationEntity>>>
            {
                {new OrderModel{OrderBy = OrderOptionNames.Relationships.FriendInvitation.InvitationDateTime, OrderOption = OrderOptionNames.Shared.Ascending}, query => query.OrderBy(f => f.InvitationDateTime)},
                {new OrderModel{OrderBy = OrderOptionNames.Relationships.FriendInvitation.InvitationDateTime, OrderOption = OrderOptionNames.Shared.Descending}, query => query.OrderByDescending(f => f.InvitationDateTime)}
            });
    
    public async Task<IEnumerable<FriendInvitation>> GetAll(FilterModel? filterModel = null, OrderModel? orderModel = null, PageModel? pageModel = null)
    {
        try
        {
            var query = _dbContext.FriendInvitations
                .Include(i => i.Invited)
                .Include(i => i.Invitor)
                .AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, FriendInvitationFilters);

            if (orderModel is not null)
                query = orderModel.Order(query, FriendInvitationOrders);

            if (pageModel is not null)
                query = pageModel.TakePage(query);
            
            return await query.Select(e => _mapper.Map<FriendInvitation>(e)).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving all friend invitations");
            throw new DataBaseException("Error while receiving friend invitations from database", e);
        }
    }

    public async Task<FriendInvitation?> GetById((Guid InvitorId, Guid InvitedId) id)
    {
        try
        {
            var invitation = await _dbContext.FriendInvitations
                .AsNoTracking()
                .Include(friendInvitationEntity => friendInvitationEntity.Invitor)
                .Include(friendInvitationEntity => friendInvitationEntity.Invited)
                
                .FirstOrDefaultAsync(fi =>
                    (fi.InvitorId == id.InvitorId && fi.InvitedId == id.InvitedId) ||
                    (fi.InvitorId == id.InvitedId && fi.InvitedId == id.InvitorId));
            
            return invitation == null ? null : _mapper.Map<FriendInvitation>(invitation);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving friend invitations by id '{id}' from database", id);
            throw new DataBaseException("Error while receiving friend invitations from database", e);
        }
    }

    public async Task<int> Count(FilterModel? filterModel = null)
    {
        try
        {
            var query = _dbContext.FriendInvitations.AsNoTracking();

            if (filterModel is not null)
                query = filterModel.Filter(query, FriendInvitationFilters);

            return await query.CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while receiving friend invitations count by filter '{filter}' from database", filterModel);
            throw new DataBaseException("Error while receiving friend invitations count from database", e);
        }
    }

    public async Task<OperationResult> Create(FriendInvitation invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        ArgumentNullException.ThrowIfNull(invitation.Invitor);
        ArgumentNullException.ThrowIfNull(invitation.Invited);
        if (invitation.Invitor.Id == invitation.Invited.Id)
            throw new ArgumentException("Invitor and invited id are the same");
        
        var invitationEntity = _mapper.Map<FriendInvitationEntity>(invitation);
        try
        {
            var existFriendInvitation = await _dbContext.FriendInvitations
                .AsNoTracking()
                .Include(friendInvitationEntity => friendInvitationEntity.Invitor)
                .Include(friendInvitationEntity => friendInvitationEntity.Invited)
                .FirstOrDefaultAsync(fi =>
                    (fi.InvitorId == invitation.Invitor.Id && fi.InvitedId == invitation.Invited.Id) ||
                    (fi.InvitorId == invitation.Invited.Id && fi.InvitedId == invitation.Invitor.Id));

            if (existFriendInvitation is not null)
                throw new AlreadyExistsException("User was already invited");
            

            await _dbContext.FriendInvitations.AddAsync(invitationEntity);
            await _dbContext.SaveChangesAsync();
        }
        catch (AlreadyExistsException alreadyExistsException)
        {
            _logger.LogError("Users '{invitor}' and '{target}' already was invited", invitation.Invitor.Id, invitation.Invited.Id);
            return DefaultOperationResult.FromException(alreadyExistsException); // 
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while adding new friend invitation (Invitor: '{invitor}' Target: {target}) to database", invitation.Invitor.Id, invitation.Invited.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while adding new friend invitation", e));
        }

        return new DefaultOperationResult(invitation);
    }

    public async Task<OperationResult> Update(FriendInvitation friendInvitation)
    {
        try
        {
            var invitationEntity = await _dbContext.FriendInvitations
                .FirstOrDefaultAsync(fi => fi.InvitorId == friendInvitation.Invitor.Id && fi.InvitedId == friendInvitation.Invited.Id);
            
            if (invitationEntity is null)
            {
                _logger.LogError("Invitation ({invitor}; {target}) was not found", friendInvitation.Invitor.Id, friendInvitation.Invited.Id);
                return DefaultOperationResult.FromException(new NotFoundException("Invitation was not found"));
            }

            _mapper.Map(friendInvitation, invitationEntity);
            
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while updating friend invitation (Invitor:'{invitor}' Target:{target}) to database", friendInvitation.Invitor.Id, friendInvitation.Invited.Id);
            return DefaultOperationResult.FromException(new DataBaseException("Error while updating friend invitation", e));
        }

        return new DefaultOperationResult(friendInvitation);
    }

    public async Task<OperationResult> Delete((Guid InvitorId, Guid InvitedId) id)
    {
        FriendInvitation? friendInvitation;
        try
        {
            var invitation = await _dbContext.FriendInvitations.FirstOrDefaultAsync(fi => fi.InvitorId == id.InvitorId && fi.InvitedId == id.InvitedId);
            
            if (invitation == null)
            {
                _logger.LogError(
                    "Invitation ({invitor}; {target}) was not found", id.InvitorId, id.InvitedId);
                return DefaultOperationResult.FromException(new NotFoundException("Invitation was not found"));
            }
            
            friendInvitation = _mapper.Map<FriendInvitation>(invitation);
            
            _dbContext.FriendInvitations.Remove(invitation);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception was thrown while removing invitation ({invitor}; {target}) from database", id.InvitorId, id.InvitedId);
            return DefaultOperationResult.FromException(new DataBaseException("Error while removing invitation", e));
        }

        return new DefaultOperationResult(friendInvitation);
    }
}