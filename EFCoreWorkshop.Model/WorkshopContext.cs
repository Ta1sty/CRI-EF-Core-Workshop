using EFCoreWorkshop.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EFCoreWorkshop.Model;

public class WorkshopContext : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<WorkerEntity> Workers { get; set; }
    
    public WorkshopContext(DbContextOptions<WorkshopContext> options) : base(options)
    {
    }

    #region Overrides of DbContext

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkerEntity>(entity =>
        {
            entity.HasKey(x => x.WorkerId);
            entity.Property(x => x.WorkerId).ValueGeneratedNever();
            entity.HasIndex(x => x.Hired);
            entity.HasIndex(x => x.EndOfContract).IsUnique();
            entity.Property(x => x.Version).IsRowVersion();
        });
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(x => x.TaskId);
            entity.Property(x => x.TaskId).ValueGeneratedNever();
            entity.Property(x => x.Version).IsRowVersion();
        });
    }   
}