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
    public async Task QueryBuilding()
    {
        // Select the source by calling the Context.xxx or Context.Set<TEntity>
        var source = _services.Context.Workers;
        TestContext.WriteLine(source.ToString());

        // var where = source.Where(x => x.LastName.StartsWith("Ga"));
        // TestContext.WriteLine(where.ToString());
        //
        // var select = where.Select(x => x.WorkerId);
        // TestContext.WriteLine(select.ToList());

        // var result1 = await select.ToListAsync();


        var groupJoinQuery = _services.Context.Workers
            .Include(x => x.TaskEntities)
            .Select(x => new
            {
                x.WorkerId,
                Tasks = x.TaskEntities.AsQueryable().GroupJoin(_services.Context.Tasks,
                    x => x.WorkerId, x => x.WorkerId,
                    (x, y) => new { x, y }).ToList()
            });
        var result = await groupJoinQuery.ToListAsync();

        // var groupResult = await source
        //     .Include(x => x.TaskEntities)
        //     .Select(x => new
        //     {
        //         Tasks = x.TaskEntities.GroupBy(t => t.Created)
        //             .Select(g => g
        //                 .First()
        //                 .Name
        //             ).AsQueryable().OrderDescending().ToList()
        //     }).ToListAsync();
    }


    [Test]
    public async Task ChangeTracking()
    {
        var worker = _services.Context.Add(new WorkerEntity
        {
            WorkerId = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann",
            Hired = DateTime.UtcNow,
            EndOfContract = DateTime.UtcNow.AddYears(1)
        }).Entity;

        AssertTrackingState<WorkerEntity>(worker.WorkerId, EntityState.Added);

        await _services.Context.SaveChangesAsync();

        AssertTrackingState<WorkerEntity>(worker.WorkerId, EntityState.Unchanged);

        var task = new TaskEntity
        {
            TaskId = Guid.NewGuid(),
            Name = "Task1",
            Description = "Do this and that",
            Created = DateTime.UtcNow,
            Length = TimeSpan.FromHours(1),
            WorkerId = worker.WorkerId,
        };
        worker.TaskEntities.Add(task);

        AssertTrackingState<TaskEntity>(task.TaskId, EntityState.Detached);

        _services.Context.ChangeTracker.DetectChanges();

        AssertTrackingState<TaskEntity>(task.TaskId, EntityState.Added);
        // This calls detect changes if configured to do so
        await _services.Context.SaveChangesAsync();

        AssertTrackingState<TaskEntity>(task.TaskId, EntityState.Unchanged);
    }

    [Test]
    public async Task Concurrency()
    {
        var worker = _services.Context.Workers.Add(new WorkerEntity
        {
            WorkerId = Guid.NewGuid(),
            FirstName = "Gustav",
            LastName = "Gans",
            Hired = DateTime.UtcNow,
            EndOfContract = DateTime.UtcNow.AddYears(1)
        }).Entity;
        await _services.Context.SaveChangesAsync();

        await _services.Context.Workers.ExecuteUpdateAsync(x => x
            .SetProperty(e => e.EndOfContract, DateTime.UtcNow)
        );
        // Will fail because the concurrency token has changed by the update above
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            worker.LastName = "Müller";
            await _services.Context.SaveChangesAsync();
        });
        // For the update to work, you need top update the tracked entity

        // We need to clear change-tracker, otherwise find will return the tracked entity that has the wrong row-version
        _services.Context.ChangeTracker.Clear();
        worker = (await _services.Context.Workers.FindAsync(worker.WorkerId))!;

        worker.LastName = "Müller";
        await _services.Context.SaveChangesAsync();
    }

    [Test]
    public async Task BulkExtensions()
    {
        // Insert the worker with the name Gustav Gans or update it if it already exists
        await _services.Context.Workers.BulkMergeAsync(
            new[]
            {
                new WorkerEntity
                {
                    WorkerId = Guid.NewGuid(),
                    FirstName = "Gustav",
                    LastName = "Gans",
                    Hired = DateTime.UtcNow,
                    EndOfContract = DateTime.UtcNow.AddYears(1)
                }
            },
            opt =>
            {
                opt.MergeKeepIdentity = true; // Prevents the primary key from being updated
                opt.ColumnPrimaryKeyExpression = x => new { x.FirstName, x.LastName };
                opt.IgnoreOnMergeUpdateExpression = x => new { x.Hired };
            }
        );

        // Insert a task for all workers
        await _services.Context.Workers.InsertFromQueryAsync(x => new TaskEntity
        {
            TaskId = Guid.NewGuid(),
            Name = "Zeiten",
            Description = "Bitte die zeiten eintragen",
            Created = DateTime.UtcNow,
            Length = TimeSpan.FromMinutes(5),
            WorkerId = x.WorkerId,
        });
        
        // Extend all contracts by one month
        await _services.Context.Workers
            .Where(x => x.EndOfContract != null)
            .UpdateFromQueryAsync(x => new WorkerEntity
            {
                EndOfContract = x.EndOfContract!.Value.AddMonths(1),
            });

        // Delete via Include
        await _services.Context.Workers
            .Include(x => x.TaskEntities)
            .SelectMany(x => x.TaskEntities)
            .DeleteFromQueryAsync();
    }

    private void AssertTrackingState<TEntity>(Guid id, EntityState state) where TEntity : class
    {
        var primaryKeyProperty = _services.Context.Set<TEntity>().EntityType.FindPrimaryKey()!.Properties.Single()
            .PropertyInfo!;
        Guid PrimaryKey(TEntity x) => (Guid)primaryKeyProperty.GetValue(x)!;
        var oldAutoDetect = _services.Context.ChangeTracker.AutoDetectChangesEnabled;
        _services.Context.ChangeTracker.AutoDetectChangesEnabled = false;
        var entry = _services.Context.ChangeTracker.Entries<TEntity>().FirstOrDefault(x => PrimaryKey(x.Entity) == id);
        _services.Context.ChangeTracker.AutoDetectChangesEnabled = oldAutoDetect;
        if (state == EntityState.Detached)
        {
            Assert.That(entry is null || entry.State == state);
        }
        else
        {
            Assert.That(entry is not null && entry.State == state);
        }
    }
}