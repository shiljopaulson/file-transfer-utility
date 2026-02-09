namespace FileSegregator.Cli.Models;

public abstract class BaseFileOptions(DirectoryInfo? Source, DirectoryInfo? Destination, Mode Mode, OutputFormat OutputFormat, bool Overwrite, bool DryRun, bool Quiet)
{
  public DirectoryInfo? Source { get; init; } = Source;
  public DirectoryInfo? Destination { get; init; } = Destination;
  public Mode Mode { get; init; } = Mode;
  public OutputFormat OutputFormat { get; init; } = OutputFormat;
  public bool Overwrite { get; init; } = Overwrite;
  public bool DryRun { get; init; } = DryRun;
  public bool Quiet { get; init; } = Quiet;

  public override string ToString()
  {
    return $"Source:{Source}, Destination:{Destination}, Mode:{Mode}, OutputFormat:{OutputFormat}, Overwrite:{Overwrite}, DryRun:{DryRun}, Quiet:{Quiet}";
  }
}