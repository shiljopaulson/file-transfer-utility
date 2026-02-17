using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Commands;

public abstract class BaseCommand<TParsedOption, TResult>(string name, string? description) : Command(name, description) where TParsedOption : class
{
  public bool Quiet { get; internal set; }
  public OutputFormat OutputFormat { get; internal set; }

  internal TParsedOption? ParsedOptions { get; set; }
  internal TResult? Result { get; set; }

  public abstract Command Build();
}
