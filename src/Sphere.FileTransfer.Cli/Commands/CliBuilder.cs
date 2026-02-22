using System.CommandLine;

using Microsoft.Extensions.Logging;

using Sphere.FileTransfer.Cli.Handlers;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Cli.Options;

namespace Sphere.FileTransfer.Cli.Commands;

/// <summary>
/// Assembles the <see cref="RootCommand"/> with all sub-commands.
/// </summary>
internal sealed class CliBuilder(
  RootHandler rootHandler,
  DelimitedCommand delimitedCommand,
  PatternCommand patternCommand,
  ILogger<CliBuilder> logger)
{
  private readonly RootHandler _rootHandler = rootHandler;
  private readonly DelimitedCommand _delimitedCommand = delimitedCommand;
  private readonly PatternCommand _patternCommand = patternCommand;
  private readonly ILogger<CliBuilder> _logger = logger;

  /// <summary>
  /// Builds and returns the configured <see cref="RootCommand"/>.
  /// </summary>
  public RootCommand Build()
  {
    _logger.LogTrace("Entering CliBuilder => Build");
    var delimiters = Enum.GetNames<Delimiter>().Select(x => x.ToLowerInvariant());
    var rootDescription = $"A data driven file transfer utility which copies or moves files from one or multiple source directories to the destination directory based on the `delimited file` ({string.Join(" / ", delimiters)} files) or directory `search pattern` (Example: *.png, *.*).";

    var rootCommand = new RootCommand(rootDescription);
    rootCommand.Options.Add(new InfoOption());

    rootCommand.Subcommands.Add(_delimitedCommand.Build());
    rootCommand.Subcommands.Add(_patternCommand.Build());

    rootCommand.SetAction(_rootHandler.Handle);

    return rootCommand;
  }
}