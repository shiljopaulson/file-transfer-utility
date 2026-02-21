using System.CommandLine;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Cli.Handlers;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Cli.Options;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Commands;

public sealed class DelimitedCommand : BaseCommand<DelimitedOptions, DelimitedFile>
{
  private static readonly string _description;
  private readonly DelimitedHandler _delimitedHandler;
  private readonly ILogger<DelimitedCommand> _logger;

  static DelimitedCommand()
  {
    var delimiters = Enum.GetNames<Delimiter>().Select(x => x.ToLowerInvariant());
    _description = $"Copies or Movies files based on the file name entries found in the delimited file ({string.Join(" / ", delimiters)} files)";
  }

  public DelimitedCommand(DelimitedHandler delimitedHandler, ILogger<DelimitedCommand> logger, string name = "delimited") : base(name, "")
  {
    Description = _description;
    _delimitedHandler = delimitedHandler;
    _logger = logger;
  }

  public override Command Build()
  {
    _logger.LogTrace("Entering DelimitedCommand => Build");

    Options.Add(new SourcesOption());
    Options.Add(new DestinationOption());
    Options.Add(new FileOption());
    Options.Add(new ColumnOption());
    Options.Add(new DelimiterOption());
    Options.Add(new OperationOption());
    Options.Add(new NoHeaderOption());
    Options.Add(new OutputFormatOption());
    Options.Add(new OverwriteOption());
    Options.Add(new DryRunOption());
    Options.Add(new QuietOption());

    SetAction(_delimitedHandler.Handle);
    return this;
  }
}
