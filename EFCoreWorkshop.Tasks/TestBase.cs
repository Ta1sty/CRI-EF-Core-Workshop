using Bogus;
using Bogus.DataSets;
using EFCoreWorkshop.Helper;
using EFCoreWorkshop.Model;
using EFCoreWorkshop.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreWorkshop.Tasks;

public class TestBase
{
    private Services<WorkshopContext> _services = null!;
    protected WorkshopContext Context => _services.Context;
    

    [SetUp]
    public async Task Setup()
    {
        _services = await Services<WorkshopContext>.Create();
        await _services.Context.Database.EnsureDeletedAsync();
        await _services.Context.Database.EnsureCreatedAsync();
        await GenerateTestData();
    }

    [TearDown]
    public async Task TearDown() => await _services.DisposeAsync();
    
    private async Task GenerateTestData()
    {
        const int Seed = 0;
        var startDate = new DateTime(2020, 1, 1);
        var random = new Random(Seed);
        var entities = Enumerable.Range(0, random.Next(1000,2000))
            .Select(_ =>
            {
                var person = new Person("de", random.Next(0, 100));

                var hired = startDate.AddDays(random.Next(0, 1000));
                var worker = new WorkerEntity
                {
                    WorkerId = Guid.NewGuid(),
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Hired = hired,
                    EndOfContract = random.Next(0, 10) == 0 ? null : hired.AddDays(random.Next(0, 1000)),
                };
                var tasks = Enumerable.Range(0, random.Next(0, 20))
                    .Select(_ =>
                    {
                        var customer = new Person("de", random.Next(0, 2000));
                        var firstName = customer.FirstName;
                        var lastName = customer.LastName;
                        var company = new Company("de")
                        {
                            Random = new Randomizer(random.Next(0, 2000))
                        }.CompanyName();
                        var (name,desc) = random.Next(0, 4) switch
                        {
                            0 => ($"Deliver to {company}", $"Deliver Order {random.Next(0,1000000)} to {company}"),
                            1 => ($"Help {firstName} {lastName}", $"Help {firstName} {lastName} finish up their tasks"),
                            2 => ($"Sync your times", "Sync your times, make sure you are up to date, and report back to your boss"),
                            3 => ($"Support ticket: #{random.Next(0, 1000000)}", $"Customer reported issue in usage of product {random.Next(100000)}"),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        return new TaskEntity
                        {
                            TaskId = Guid.NewGuid(),
                            Name = name,
                            Description = desc,
                            Created = startDate.AddDays(random.Next(0, 1000)),
                            Length = TimeSpan.FromHours(random.Next(0, 20)),
                            WorkerId = worker.WorkerId,
                        };
                    }).ToList();

                return new { worker, tasks};
            }).ToList();
        await Context.BulkInsertAsync(entities.Select(x => x.worker));
        await Context.BulkInsertAsync(entities.SelectMany(x => x.tasks));
    }
}