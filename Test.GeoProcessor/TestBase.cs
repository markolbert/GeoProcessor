using System;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.GeoProcessor;
using J4JSoftware.GeoProcessor.RouteBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Test.GeoProcessor;

public class TestBase
{
    protected TestBase()
    {
        Services = CreateHost();
        Logger = Services.GetService<ILoggerFactory>()?.CreateLogger( GetType() );

        var config = Services.GetService<TestConfig>();
        config.Should().NotBeNull();
        Config = config!;
    }

    protected ILogger? Logger { get; }
    protected IServiceProvider Services { get; }
    protected TestConfig Config { get; }

    private IServiceProvider CreateHost() =>
        new HostBuilder()
           .ConfigureAppConfiguration(ConfigureApp)
           .ConfigureServices( CreateServices )
                         .Build()
                         .Services;

    private void ConfigureApp( HostBuilderContext hbc, IConfigurationBuilder builder )
    {
        builder.AddUserSecrets<TestBase>();
    }

    private void CreateServices( HostBuilderContext hbc, IServiceCollection services )
    {
        services.AddSingleton<TestConfig>( hbc.Configuration.Get<TestConfig>() ?? new TestConfig() );

        var seriLogger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Debug()
                        .CreateLogger();

        services.AddSingleton( new LoggerFactory().AddSerilog( seriLogger ) );
        
        services.AddTransient<RouteBuilder>( s => new RouteBuilder( s.GetService<ILoggerFactory>() ) );

    }

    protected Task LogMessage( ProcessingMessage mesg )
    {
        Logger?.LogInformation("{phase} {mesg}", mesg.Phase, mesg.Message );

        return Task.CompletedTask;
    }

    protected Task LogStatus( StatusInformation info )
    {
        if( info.TotalToProcess <= 0 )
            Logger?.LogInformation( "{phase} processed {items} items", info.Phase, info.Processed );
        else
            Logger?.LogInformation( "{phase} processed {items} of {total} items",
                                    info.Phase,
                                    info.Processed,
                                    info.TotalToProcess );

        return Task.CompletedTask;
    }
}
