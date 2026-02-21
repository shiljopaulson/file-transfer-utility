using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Templates;
using Serilog.Settings.Configuration;
using Sphere.FileTransfer.Cli.Commands;
using Sphere.FileTransfer.Cli.Handlers;
using Sphere.FileTransfer.Cli.Mappers;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Cli.Writer;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;
using Sphere.FileTransfer.Services.Validators;

namespace Sphere.FileTransfer.Cli;

class Program
{
    private const string APP_NAME = "Sphere.FileTransfer.Cli";
    private const string APP_SETTINGS_FILE = "appsettings.json";

    async static Task<int> Main(string[] args)
    {
        var exitCode = 0;
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };
        try
        {
            var host = CreateHost(args);

            var cliBuilder = host.Services.GetRequiredService<CliBuilder>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var rootCommand = cliBuilder.Build();

            Log.Information("{APP_NAME} starting up", APP_NAME);
            return await rootCommand.Parse(args).InvokeAsync();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation canceled.");
            exitCode = ExitCodes.Canceled;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Console.ResetColor();
            Log.Error("Exception {ex.Message}", ex.Message);
            return ExitCodes.Error;
        }
        finally
        {
            Log.Information("Exiting with code {exitCode}", exitCode);
            await Log.CloseAndFlushAsync();
        }
        return exitCode;
    }

    static IHost CreateHost(string[] args)
    {
        var options = new ConfigurationReaderOptions(
            typeof(ConsoleLoggerConfigurationExtensions).Assembly
        );
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(APP_SETTINGS_FILE, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        var applicationBuilder = Host.CreateApplicationBuilder();
        applicationBuilder.Environment.ApplicationName = APP_NAME;
        applicationBuilder.Services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConfiguration(configuration);
            builder.AddSerilog(Log.Logger, dispose: true);
        });
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration, options)
            .CreateLogger();

        // Services
        applicationBuilder.Services.AddSingleton<IDelimitedService, DelimitedService>();
        applicationBuilder.Services.AddSingleton<IPatternService, PatternService>();

        // Readers
        applicationBuilder.Services.AddSingleton<IDelimitedReader, DelimitedReader>();
        applicationBuilder.Services.AddSingleton<IDirectoryReader, DirectoryReader>();

        // Validators
        applicationBuilder.Services.AddSingleton<AbstractValidator<DelimitedOptions>, DelimitedOptionsValidator>();
        applicationBuilder.Services.AddSingleton<AbstractValidator<PatternOptions>, PatternOptionsValidator>();

        // Sub-Commands
        applicationBuilder.Services.AddSingleton<DelimitedCommand>();
        applicationBuilder.Services.AddSingleton<PatternCommand>();

        // Handlers
        applicationBuilder.Services.AddSingleton<RootHandler>();
        applicationBuilder.Services.AddSingleton<DelimitedHandler>();
        applicationBuilder.Services.AddSingleton<PatternHandler>();

        // Mappers
        applicationBuilder.Services.AddSingleton<IMap<Delimiter, char>, DelimiterToChar>();
        applicationBuilder.Services.AddSingleton<IMap<char, Delimiter>, CharToDelimiter>();
        applicationBuilder.Services.AddSingleton<IOptionsMapper<DelimitedOptions>, DelimitedOptionsMapper>();
        applicationBuilder.Services.AddSingleton<IOptionsMapper<PatternOptions>, PatternOptionsMapper>();

        // Result Writers
        applicationBuilder.Services.AddSingleton<IResultWriter<DelimitedFile>, DelimitedResultWriter>();

        // Root CLI builder
        applicationBuilder.Services.AddSingleton<CliBuilder>();

        return applicationBuilder.Build();
    }
}
