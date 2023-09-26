using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace EFCoreWorkshop.Helper;

public sealed class Services<TContext> : IAsyncDisposable where TContext : DbContext
{
    private static int _nextDb = 0;
    
    private readonly IContainer _container;
    private readonly IHost _host;

    public readonly AsyncServiceScope Scope;
    public readonly IServiceProvider ServiceProvider;
    public readonly TContext Context;

    private Services(IHost host, IContainer container)
    {
        _host = host;
        _container = container;
        Scope = _host.Services.CreateAsyncScope();
        ServiceProvider = Scope.ServiceProvider;
        Context = ServiceProvider.GetRequiredService<TContext>();
    }

    public async Task WaitForShutdownAsync() => await _host.WaitForShutdownAsync();

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

        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(collection =>
        {
            collection.AddDbContextPool<TContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsAssembly("EFCoreWorkshop.Migrations");
                        sqlServerOptions.MigrationsHistoryTable("__MigrationsHistory");
                        sqlServerOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
                    })
                    .EnableSensitiveDataLogging()
                    .LogTo((x, _) => x == RelationalEventId.CommandExecuting, Console.WriteLine)
                    .UseLoggerFactory(new LoggerFactory(new[] { new DebugLoggerProvider() }));
            }, 16);
        });
        var host = builder.Build();
        await host.StartAsync();
        return new Services<TContext>(host, container);
    }

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync();
        await _host.StopAsync();
        _host.Dispose();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}