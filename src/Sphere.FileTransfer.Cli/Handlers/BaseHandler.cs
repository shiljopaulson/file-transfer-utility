using System.CommandLine;

using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Handlers;

internal abstract class BaseHandler<TOptions, TResult>
{
  internal bool Quiet { get; set; }
  internal OutputFormat OutputFormat { get; set; }
  internal TOptions? ParsedOptions { get; set; }
  internal TResult? Result { get; set; }
  internal void ParseDefaultOptions(ParseResult parseResult)
  {
    Quiet = parseResult.GetValue<bool>(OptionNames.Quiet);
    OutputFormat = parseResult.GetValue<OutputFormat>(OptionNames.OutputFormat);
  }
}