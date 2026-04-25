using System.Collections.Immutable;

using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
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

internal sealed class Program
{
  private Program()
  {
  }

  private const string APP_NAME = "Sphere.FileTransfer.Cli";
  private const string APP_SETTINGS_FILE = "ftu.appsettings.json";


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
      using var host = CreateHost();

      var cliBuilder = host.Services.GetRequiredService<CliBuilder>();
      var rootCommand = cliBuilder.Build();

      Log.Information("Starting {AppName} at {StartTime}", APP_NAME, DateTime.Now.ToString("o"));
      return await rootCommand.Parse(args).InvokeAsync().ConfigureAwait(true);
    }
    catch (OperationCanceledException)
    {
      Utility.WriteLine("Operation canceled.", ConsoleColor.Cyan);
      exitCode = ExitCodes.Canceled;
    }
    finally
    {
      Log.Information("Exiting with code {ExitCode}", exitCode);
      await Log.CloseAndFlushAsync().ConfigureAwait(true);
    }
    return exitCode;
  }

  static IHost CreateHost()
  {
    var readerOptions = new ConfigurationReaderOptions(
        typeof(Serilog.ConsoleLoggerConfigurationExtensions).Assembly,
        typeof(Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme).Assembly, // Serilog.Sinks.Console
        typeof(Serilog.Sinks.File.FileSink).Assembly // Serilog.Sinks.File
    );
    IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(APP_SETTINGS_FILE, optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration, readerOptions)
        .CreateLogger();
    var applicationBuilder = Host.CreateApplicationBuilder();
    applicationBuilder.Environment.ApplicationName = APP_NAME;
    applicationBuilder.Services.AddLogging(builder =>
    {
      builder.ClearProviders();
      builder.AddConfiguration(configuration);
      builder.AddSerilog(Log.Logger, dispose: true);
    });

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
    applicationBuilder.Services.AddSingleton<IResultWriter<ImmutableArray<SegregatedDirectory>>, PatternResultWriter>();

    // Root CLI builder
    applicationBuilder.Services.AddSingleton<CliBuilder>();

    return applicationBuilder.Build();
  }
}