﻿using Application.Identity;
using Application.Models;
using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IFriendsService
{
    public Task<OperationResult> CreateInvitation(ApplicationUser invitor, ApplicationUser target);
    public Task<OperationResult> AcceptInvitation(ApplicationUser user, Guid invitationId);
    public Task<OperationResult> RefuseInvitation(ApplicationUser user, Guid invitationId);

    public Task<IEnumerable<FriendInvitation>> GetInvitationsFor(ApplicationUser user);
    public Task<IEnumerable<FriendInvitation>> GetInvitationsOf(ApplicationUser user);
    
    public Task<IEnumerable<ApplicationUser>> GetFriendsFor(ApplicationUser user);
    
    public Task<OperationResult> RemoveFriendship(ApplicationUser user1, ApplicationUser user2);
}