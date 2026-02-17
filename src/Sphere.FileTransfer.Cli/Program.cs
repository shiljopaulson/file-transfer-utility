using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Cli.Commands;
using Sphere.FileTransfer.Cli.Handlers;
using Sphere.FileTransfer.Services;
using Sphere.FileTransfer.Services.Readers;
using Sphere.FileTransfer.Services.Validators;
using FluentValidation;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Cli.Results;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Cli.Mappers;

namespace Sphere.FileTransfer.Cli;

class Program
{
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
            return await rootCommand.Parse(args).InvokeAsync();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation canceled.");
            exitCode = 130;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
        return exitCode;
    }

    static IHost CreateHost(string[] args) =>
    Host.CreateDefaultBuilder(args)
        // ── Configuration ──────────────────────────────────────────────────
        /* .ConfigureAppConfiguration((hostCtx, config) =>
        {
            config
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile(
                    $"appsettings.{hostCtx.HostingEnvironment.EnvironmentName}.json",
                    optional: true, reloadOnChange: false)
                .AddEnvironmentVariables(prefix: "FILETOOL_")
                .AddCommandLine(args);    // allows --FileTool:MaxFileSizeMb 200 etc.
        }) */
        // ── Logging ────────────────────────────────────────────────────────
        /* .ConfigureLogging((hostCtx, logging) =>
        {
            logging.ClearProviders();
            logging.AddConsole(opts => opts.FormatterName = "simple");
            logging.AddConfiguration(hostCtx.Configuration.GetSection("Logging"));
        }) */
        // ── Services / DI ──────────────────────────────────────────────────
        .ConfigureServices((hostCtx, services) =>
        {
            // Strongly-typed options bound from appsettings.json "FileTool" section
            /* services.Configure<FileToolOptions>(
                hostCtx.Configuration.GetSection(FileToolOptions.SectionName)); */

            // Services
            services.AddSingleton<IDelimitedService, DelimitedService>();
            services.AddSingleton<IPatternService, PatternService>();

            // Readers
            services.AddSingleton<IDelimitedReader, DelimitedReader>();
            services.AddSingleton<IDirectoryReader, DirectoryReader>();

            // Validators
            services.AddSingleton<AbstractValidator<DelimitedOptions>, DelimitedOptionsValidator>();
            services.AddSingleton<AbstractValidator<PatternOptions>, PatternOptionsValidator>();

            // Sub-Commands
            services.AddSingleton<DelimitedCommand>();
            services.AddSingleton<PatternCommand>();

            // Handlers
            services.AddSingleton<RootHandler>();
            services.AddSingleton<DelimitedHandler>();
            services.AddSingleton<PatternHandler>();

            // Mappers
            services.AddSingleton<IOptionsMapper<DelimitedOptions>, DelimitedOptionsMapper>();
            services.AddSingleton<IOptionsMapper<PatternOptions>, PatternOptionsMapper>();

            // Result Writers
            services.AddSingleton<IResultWriter<DelimitedFile>, DelimitedResultWriter>();

            // Root CLI builder
            services.AddSingleton<CliBuilder>();
        })
        .Build();

}
