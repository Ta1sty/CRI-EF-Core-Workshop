using EFCoreWorkshop.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreWorkshop.Model;

public class WorkshopModel : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; } 
    public DbSet<WorkerEntity> Worker { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        
    }
}