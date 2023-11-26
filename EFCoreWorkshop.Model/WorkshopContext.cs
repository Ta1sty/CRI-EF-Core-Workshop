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
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkerEntity>(entity =>
        {
            entity.HasKey(x => x.WorkerId);
            entity.Property(x => x.WorkerId).ValueGeneratedNever();
            entity.Property(x => x.Version).IsRowVersion();
            
            entity.HasIndex(x => x.Hired);
            entity.HasIndex(x => x.EndOfContract);
        });
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(x => x.TaskId);
            entity.Property(x => x.TaskId).ValueGeneratedNever();
            entity.Property(x => x.Version).IsRowVersion();

            entity.HasIndex(x => x.Created);
            entity.HasIndex(x => new { x.Created, x.Length });

            entity.HasOne(x => x.WorkerEntity)
                .WithMany(x => x.TaskEntities)
                .HasForeignKey(x => x.WorkerId)
                .HasPrincipalKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    #endregion
}