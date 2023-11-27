using System.Linq.Expressions;
using EFCoreWorkshop.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreWorkshop.Tasks;

public class Queries : TestBase
{
    [Test]
    public async Task ExpressionsAndAsQueryable()
    {
        // Instead of writing a query inline you can create an expression that can be reused multiple times to build dynaimc queries
        
        // The expression that determines if a task is overdue
        Expression<Func<TaskEntity, bool>> taskOverdueExpr = 
            task => task.Created + task.Length > DateTime.UtcNow;
        // The expression that determines if a worker is currently employed
        Expression<Func<WorkerEntity, bool>> workerActiveExpr =
            worker => worker.Hired > DateTime.UtcNow && (worker.EndOfContract == null || worker.EndOfContract < DateTime.UtcNow);

        // Can now pass it as argument to a query where clause
        

        // Can also pass this as an inner argument
        

        // You can also dynamically build search expressions such as name matching for example Richard Jonas Schmitt should match Richard Schmitt and Jonas Schmitt
        // and that for any number or name parts, on building such expressions, is a matter for another time
    }
    
    [Test]
    public async Task Include()
    {
        // You can navigate between entities using include and select, you may also reduce the number by specifying a where condition in the include
    }
    
    [Test]
    public async Task JoinAndGroupJoin()
    {
        // Manually join the table tasks to workers, using the key WorkerId, Hired/Created,
        // i.e. the set of all tasks with their worker where the task was created on the same day the worker was hired
        // var join = Context.Workers.Join();
    }
    
    [Test]
    public async Task Ordering()
    {
        // Order each worker by their LastName and then by their FirstName
        // var ordering = Context.Workers.OrderBy();
    }

    [Test]
    public async Task GroupBy()
    {
        // Grouping and to dictionary is a classic combination when building mappings
    }

    [Test]
    public async Task AggregateOps()
    {
        // All operations that project a sequence of objects onto a singular value can be called aggregate operations
        // These are First(OrDefault),Single(OrDefault),Last(OrDefault),Min,Max,Avg etc
        
    }

    [Test]
    public async Task AsyncEnumerable()
    {
        // You can use IAsyncEnumerable to avoid retrieving too many entities into RAM
        // especially useful if you need to retrieve data, and only in a few cases have to execute an update
        // you can use await foreach() to iterate over such an enumerable
        
    }
}