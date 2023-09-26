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
    }
}