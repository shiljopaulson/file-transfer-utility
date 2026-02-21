using System.CommandLine;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Cli.Handlers;
using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Commands;

/// <summary>
/// Assembles the <see cref="RootCommand"/> with all sub-commands.
/// </summary>
public sealed class CliBuilder
{
  private readonly RootHandler _rootHandler;
  private readonly DelimitedCommand _delimitedCommand;
  private readonly PatternCommand _patternCommand;
  private readonly ILogger<CliBuilder> _logger;

  public CliBuilder(
    RootHandler rootHandler,
    DelimitedCommand delimitedCommand,
    PatternCommand patternCommand,
    ILogger<CliBuilder> logger)
  {
    _rootHandler = rootHandler;
    _delimitedCommand = delimitedCommand;
    _patternCommand = patternCommand;
    _logger = logger;
  }

  /// <summary>
  /// Builds and returns the configured <see cref="RootCommand"/>.
  /// </summary>
  public RootCommand Build()
  {
    _logger.LogTrace("Entering CliBuilder => Build");
    var delimiters = Enum.GetNames<Delimiter>().Select(x => x.ToLowerInvariant());
    var rootDescription = $"A data driven file transfer utility which copies or moves files from one or multiple source directories to the destination directory based on the `delimited file` ({string.Join(",", delimiters)}) or directory `search pattern` (Example: *.png, *.*).";

    var rootCommand = new RootCommand(rootDescription);
    rootCommand.Subcommands.Add(_delimitedCommand.Build());
    rootCommand.Subcommands.Add(_patternCommand.Build());

    rootCommand.SetAction(_rootHandler.Handle);

    return rootCommand;
  }
}