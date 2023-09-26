using EFCoreWorkshop.Helper;
using EFCoreWorkshop.Model;
using EFCoreWorkshop.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreWorkshop.Tasks;

public class Tests
{
    private Services<WorkshopContext> _services = null!;

    [SetUp]
    public async Task Setup()
    {
        _services = await Services<WorkshopContext>.Create();
        await _services.Context.Database.EnsureDeletedAsync();
        await _services.Context.Database.EnsureCreatedAsync();
    }
    [TearDown]
    public async Task TearDown() => await _services.DisposeAsync();

    [Test]
    public async Task ChangeTracking()
    {
        var worker = _services.Context.Add(new WorkerEntity
        {
            WorkerId = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann",
            Hired = DateTime.UtcNow,
        }).Entity;
        
        AssertTrackingState<WorkerEntity>(worker.WorkerId, EntityState.Added);

        await _services.Context.SaveChangesAsync();
        
        AssertTrackingState<WorkerEntity>(worker.WorkerId, EntityState.Unchanged);
        
        worker.TaskEntities.Add(new TaskEntity
        {
            TaskId = Guid.NewGuid(),
            Name = "Task1",
            Description = "Do this and that",
            Created = DateTime.UtcNow,
            Length = TimeSpan.FromHours(1),
            WorkerId = worker.WorkerId,
        });

        await _services.Context.SaveChangesAsync();
    }

    private void AssertTrackingState<TEntity>(Guid id, EntityState state) where TEntity : class
    {
        var primaryKeyProperty = _services.Context.Set<TEntity>().EntityType.FindPrimaryKey()!.Properties.Single().PropertyInfo!;
        Guid PrimaryKey(TEntity x) => (Guid)primaryKeyProperty.GetValue(x)!;
        var entry = _services.Context.ChangeTracker.Entries<TEntity>().FirstOrDefault(x => PrimaryKey(x.Entity) == id);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.State, Is.EqualTo(state));
    }
}