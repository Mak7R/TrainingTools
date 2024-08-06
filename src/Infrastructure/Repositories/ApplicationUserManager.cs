using Domain.Identity;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    private readonly ApplicationDbContext _context;

    public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<ApplicationUserManager> logger,
        ApplicationDbContext context)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors,
            services, logger)
    {
        _context = context;
    }

    public override Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        user.RegistrationDateTime = DateTime.UtcNow;
        return base.CreateAsync(user, password);
    }

    public override Task<IdentityResult> CreateAsync(ApplicationUser user)
    {
        user.RegistrationDateTime = DateTime.UtcNow;
        return base.CreateAsync(user);
    }

    public override async Task<IdentityResult> DeleteAsync(ApplicationUser user)
    {
        var invitations = _context.FriendInvitations.Where(fi => fi.InvitorId == user.Id || fi.InvitedId == user.Id);
        _context.FriendInvitations.RemoveRange(invitations);

        // Delete related friendships
        var friendships = _context.Friendships.Where(fr => fr.FirstFriendId == user.Id || fr.SecondFriendId == user.Id);
        _context.Friendships.RemoveRange(friendships);

        await _context.SaveChangesAsync();

        return await base.DeleteAsync(user);
    }
}