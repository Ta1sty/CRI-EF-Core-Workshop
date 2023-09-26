using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace EFCoreWorkshop.Migration;

public sealed class Services<TContext> : IAsyncDisposable where TContext : DbContext
{
    private static int _nextDb = 0;
    
    private readonly ServiceProvider _serviceProvider;
    private readonly IContainer _container;
    
    public readonly AsyncServiceScope Scope;
    public readonly IServiceProvider ServiceProvider;
    public readonly TContext Context;

    private Services(IContainer container, ServiceProvider serviceProvider)
    {
        _container = container;
        _serviceProvider = serviceProvider;
        Scope = _serviceProvider.CreateAsyncScope();
        ServiceProvider = Scope.ServiceProvider;
        Context = ServiceProvider.GetRequiredService<TContext>();
    }

    public static async Task<Services<TContext>> Create()
    {
        const int port = 23564;
        var container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .WithName("TEST-Container")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SA_PASSWORD", "Mssql12345")
            .WithPortBinding(port, 1433)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();
        await container.StartAsync();
        
        var dbName = typeof(TContext).Name.ToLower() + "_" + Interlocked.Increment(ref _nextDb);
        var connectionString = @$"Server=localhost,{port.ToString()};Database={dbName};User Id=sa;Password=Mssql12345;Trust Server Certificate=True";
        
        await Console.Error.WriteLineAsync(connectionString);
        
        var collection = new ServiceCollection();
        collection.AddDbContextPool<TContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(typeof(Services<TContext>).Assembly.FullName);
                    sqlServerOptions.MigrationsHistoryTable("__MigrationsHistory");
                    sqlServerOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
                })
                .EnableSensitiveDataLogging()
                .LogTo((x,_) => x == RelationalEventId.CommandExecuting, Console.WriteLine)
                .UseLoggerFactory(new LoggerFactory(new []{new DebugLoggerProvider()}));
        }, 16);
        var services = collection.BuildServiceProvider();
        return new Services<TContext>(container,services);
    }

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}