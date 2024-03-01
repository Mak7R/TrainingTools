using Contracts.Models;
using Microsoft.EntityFrameworkCore;

namespace Services.DbContexts;

public class TrainingToolsDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    
    public DbSet<Group> Groups { get; set; }
    
    public TrainingToolsDbContext(DbContextOptions options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        base.OnModelCreating(modelBuilder);
    }
}