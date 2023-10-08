using System.ComponentModel.DataAnnotations;

namespace EFCoreWorkshop.Model.Entities;

public class TaskEntity
{
    public Guid TaskId { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime Created { get; set; }
    public TimeSpan Length { get; set; }
    public byte[] Version { get; set; }

    public Guid WorkerId { get; set; }
    public WorkerEntity WorkerEntity { get; set; }
}