using System.CommandLine;

namespace FileSegregator.Cli.Commands;

public abstract class BaseCommand<TParsedOption, TResult>(string name, string? description) : Command(name, description)
{
  internal TParsedOption? ParsedOptions { get; set; }
  internal TResult? Result { get; set; }

  public abstract Command Create();

  public int Execute(CancellationToken cancellationToken)
  {
    var exitCode = Process(cancellationToken);
    Print(cancellationToken);
    return exitCode;
  }

  internal abstract int Process(CancellationToken cancellationToken);
  internal abstract void Print(CancellationToken cancellationToken);
}
