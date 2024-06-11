using Application.Identity;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public virtual DbSet<GroupEntity> Groups { get; set; }
    public virtual DbSet<ExerciseEntity> Exercises { get; set; }
    public virtual DbSet<ExerciseResultEntity> ExerciseResults { get; set; }
    public virtual DbSet<FriendInvitationEntity> FriendInvitations { get; set; }
    public virtual DbSet<FriendRelationshipEntity> FriendRelationships { get; set; }
    
    
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<GroupEntity>()
            .ToTable("Group")
            .HasIndex(g => g.Name)
            .IsUnique();

        builder.Entity<ExerciseEntity>()
            .ToTable("Exercise")
            .HasIndex(e => e.Name)
            .IsUnique();

        builder.Entity<ExerciseResultEntity>()
            .ToTable("ExerciseResult")
            .HasKey(er => new { er.UserId, er.ExerciseId });

        builder.Entity<FriendInvitationEntity>()
            .ToTable("FriendInvitation")
            .HasKey(fi => new { fi.InvitorId, fi.TargetId });

        builder.Entity<FriendRelationshipEntity>()
            .ToTable("FriendRelationship")
            .HasKey(fr => new { fr.FirstFriendId, fr.SecondFriendId });
    }
}