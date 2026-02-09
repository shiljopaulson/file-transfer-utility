namespace FileSegregator.Cli.Models;

public abstract class BaseFileOptions(DirectoryInfo[]? Sources, DirectoryInfo? Destination, Mode Mode, OutputFormat OutputFormat, bool Overwrite, bool DryRun, bool Quiet)
{
  public DirectoryInfo[]? Sources { get; init; } = Sources;
  public DirectoryInfo? Destination { get; init; } = Destination;
  public Mode Mode { get; init; } = Mode;
  public OutputFormat OutputFormat { get; init; } = OutputFormat;
  public bool Overwrite { get; init; } = Overwrite;
  public bool DryRun { get; init; } = DryRun;
  public bool Quiet { get; init; } = Quiet;

  public override string ToString()
  {
    return $"Source:{Sources}, Destination:{Destination}, Mode:{Mode}, OutputFormat:{OutputFormat}, Overwrite:{Overwrite}, DryRun:{DryRun}, Quiet:{Quiet}";
  }
}