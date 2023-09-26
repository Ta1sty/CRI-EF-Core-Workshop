using EFCoreWorkshop.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreWorkshop.Model;

public class WorkshopContext : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<WorkerEntity> Worker { get; set; }
    
    public WorkshopContext(DbContextOptions<WorkshopContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkerEntity>(entity =>
        {
            entity.HasKey(x => x.WorkerId);
            entity.Property(x => x.WorkerId).ValueGeneratedNever();
        });

        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(x => x.TaskId);
            entity.Property(x => x.TaskId).ValueGeneratedNever();
        });
    }
}