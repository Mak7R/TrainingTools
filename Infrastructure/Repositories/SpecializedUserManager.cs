using Application.Identity;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories;

public class SpecializedUserManager : UserManager<ApplicationUser>
{
    private readonly ApplicationDbContext _context;

    public SpecializedUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<SpecializedUserManager> logger,
        ApplicationDbContext context)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _context = context;
    }

    public override async Task<IdentityResult> DeleteAsync(ApplicationUser user)
    {
        var invitations = _context.FriendInvitations.Where(fi => fi.InvitorId == user.Id || fi.TargetId == user.Id);
        _context.FriendInvitations.RemoveRange(invitations);

        // Delete related friendships
        var friendships = _context.FriendRelationships.Where(fr => fr.FirstFriendId == user.Id || fr.SecondFriendId == user.Id);
        _context.FriendRelationships.RemoveRange(friendships);

        await _context.SaveChangesAsync();
        
        return await base.DeleteAsync(user);
    }
}