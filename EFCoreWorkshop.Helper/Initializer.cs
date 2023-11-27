using System.Diagnostics;
using System.IO.Pipes;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace EFCoreWorkshop.Helper;

public sealed class Services<TContext> : IAsyncDisposable where TContext : DbContext
{
    private static int _nextDb = 0;
    
    private readonly IContainer _container;
    private readonly IHost _host;
    private readonly Process? _consoleProcess;
    private readonly Stream? _consolePipe;
    private readonly StreamWriter? _consoleWriter;
    
    public readonly AsyncServiceScope Scope;
    public readonly IServiceProvider ServiceProvider;
    public readonly TContext Context;

    private Services(IHost host, IContainer container, Process? consoleProcess, Stream? consolePipe, StreamWriter? consoleWriter)
    {
        _host = host;
        _container = container;
        _consolePipe = consolePipe;
        _consoleWriter = consoleWriter;
        _consoleProcess = consoleProcess;
        Scope = _host.Services.CreateAsyncScope();
        ServiceProvider = Scope.ServiceProvider;
        Context = ServiceProvider.GetRequiredService<TContext>();
    }

    private static (Process?,Stream?,StreamWriter?) CreateConsoleWindow()
    {
#if DEBUG
        var pipeName = Guid.NewGuid().ToString();
        var consolePipe = new NamedPipeServerStream(pipeName, PipeDirection.Out);
        var startInfo = new ProcessStartInfo
        {
            FileName = "EFCoreWorkshop.Console.exe",
            UseShellExecute = true,
            CreateNoWindow = false,
            Arguments = pipeName
        };

        var consoleProcess = new Process { StartInfo = startInfo };
        if (!consoleProcess.Start()) throw new Exception("Failed to open console window");
        consolePipe.WaitForConnection();
        var consoleWriter = new StreamWriter(consolePipe, leaveOpen:true)
        {
            AutoFlush = true
        };
        consoleWriter.WriteLine("Connected to DebugConsole");

        Console.SetOut(consoleWriter);
        Console.SetError(consoleWriter);
        
        return (consoleProcess, consolePipe, consoleWriter);
#else
        return (null, null, null);
#endif
    }

    public async Task WaitForShutdownAsync() => await _host.WaitForShutdownAsync();

    public static async Task<Services<TContext>> Create()
    {
        var debugConsole = CreateConsoleWindow();
        const int port = 23564;
        var container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .WithName("TEST-Container-" + Guid.NewGuid())
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

        Console.WriteLine(connectionString);
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
                    .LogTo((x, _) => x == RelationalEventId.CommandExecuting || x == RelationalEventId.CommandExecuted, Console.WriteLine);
            }, 16);
        }).ConfigureLogging(options =>
        {
            options.ClearProviders()
                .AddConsole()
                .AddDebug();
        });
        var host = builder.Build();
        await host.StartAsync();
        return new Services<TContext>(host, container, debugConsole.Item1, debugConsole.Item2, debugConsole.Item3);
    }

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync();
        await _host.StopAsync();
        _host.Dispose();
        await _container.StopAsync();
        await _container.DisposeAsync();

#if DEBUG
        if (_consoleWriter is not null) await _consoleWriter.DisposeAsync();
        if (_consolePipe is not null) await _consolePipe.DisposeAsync();
#endif
    }
}