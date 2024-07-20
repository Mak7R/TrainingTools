using Domain.Identity;
using Infrastructure.Entities;
using Infrastructure.Entities.Friendship;
using Infrastructure.Entities.TrainingPlan;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public virtual DbSet<GroupEntity> Groups { get; set; }
    public virtual DbSet<ExerciseEntity> Exercises { get; set; }
    public virtual DbSet<ExerciseResultEntity> ExerciseResults { get; set; }
    public virtual DbSet<FriendInvitationEntity> FriendInvitations { get; set; }
    public virtual DbSet<FriendshipEntity> Friendships { get; set; }
    
    
    public virtual DbSet<TrainingPlanEntity> TrainingPlans { get; set; }
    public virtual DbSet<TrainingPlanBlockEntity> TrainingPlanBlocks { get; set; }
    public virtual DbSet<TrainingPlanBlockEntryEntity> TrainingPlanBlockEntries { get; set; }
    
    
    
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // -------------------- Basic Entities -------------------- //
        builder.Entity<GroupEntity>()
            .ToTable("Group")
            .HasIndex(g => g.Name)
            .IsUnique();

        builder.Entity<ExerciseEntity>()
            .ToTable("Exercise")
            .HasIndex(e => e.Name);

        builder.Entity<ExerciseEntity>()
            .HasIndex(e => new {e.Name, e.GroupId})
            .IsUnique();

        builder.Entity<ExerciseResultEntity>()
            .ToTable("ExerciseResult")
            .HasKey(er => new { UserId = er.OwnerId, er.ExerciseId });

        
        // -------------------- Friendship -------------------- //
        
        builder.Entity<FriendInvitationEntity>()
            .ToTable("FriendInvitation")
            .HasKey(fi => new { fi.InvitorId, TargetId = fi.InvitedId });
        

        builder.Entity<FriendInvitationEntity>()
            .HasOne(fi => fi.Invitor)
            .WithMany()
            .HasForeignKey(fi => fi.InvitorId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Entity<FriendInvitationEntity>()
            .HasOne(fi => fi.Invited)
            .WithMany()
            .HasForeignKey(fi => fi.InvitedId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<FriendshipEntity>()
            .ToTable("Friendship")
            .HasKey(fr => new { fr.FirstFriendId, fr.SecondFriendId });
        
        builder.Entity<FriendshipEntity>()
            .HasOne(fr => fr.FirstFriend)
            .WithMany()
            .HasForeignKey(fr => fr.FirstFriendId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Entity<FriendshipEntity>()
            .HasOne(fr => fr.SecondFriend)
            .WithMany()
            .HasForeignKey(fr => fr.SecondFriendId)
            .OnDelete(DeleteBehavior.NoAction);

        
        // -------------------- Training Plan -------------------- //
        
        builder.Entity<TrainingPlanEntity>()
            .ToTable("TrainingPlan")
            .HasKey(plan => plan.Id);

        builder.Entity<TrainingPlanEntity>()
            .HasIndex(plan => new { plan.AuthorId, Name = plan.Title })
            .IsUnique();

        builder.Entity<TrainingPlanEntity>()
            .HasIndex(plan => plan.Title);
        
        builder.Entity<TrainingPlanEntity>()
            .HasMany(p => p.TrainingPlanBlocks)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<TrainingPlanBlockEntity>()
            .ToTable("TrainingPlanBlock")
            .Property("TrainingPlanEntityId")
            .IsRequired();
        

        builder.Entity<TrainingPlanBlockEntity>()
            .HasMany(block => block.TrainingPlanBlockEntries)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<TrainingPlanBlockEntryEntity>()
            .ToTable("TrainingPlanBlockEntry");
    }
}