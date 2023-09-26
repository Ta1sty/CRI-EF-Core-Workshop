namespace EFCoreWorkshop.Model.Entities;

public class WorkerEntity
{
    public Guid WorkerId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Hired { get; set; }
    
    public List<TaskEntity> TaskEntities { get; set; }
}