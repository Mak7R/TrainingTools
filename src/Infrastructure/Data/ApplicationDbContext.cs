using Domain.Identity;
using Infrastructure.Entities;
using Infrastructure.Entities.Friendship;
using Infrastructure.Entities.TrainingPlan;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<GroupEntity> Groups { get; set; }
    public virtual DbSet<ExerciseEntity> Exercises { get; set; }
    public virtual DbSet<ExerciseResultEntity> ExerciseResults { get; set; }
    public virtual DbSet<FriendInvitationEntity> FriendInvitations { get; set; }
    public virtual DbSet<FriendshipEntity> Friendships { get; set; }


    public virtual DbSet<TrainingPlanEntity> TrainingPlans { get; set; }
    public virtual DbSet<TrainingPlanBlockEntity> TrainingPlanBlocks { get; set; }
    public virtual DbSet<TrainingPlanBlockEntryEntity> TrainingPlanBlockEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}