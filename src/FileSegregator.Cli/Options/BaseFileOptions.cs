using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Options;

public abstract class BaseFileOptions(DirectoryInfo[]? Sources, DirectoryInfo? Destination, Operation Operation, OutputFormat OutputFormat, bool Overwrite, bool DryRun, bool Quiet)
{
  public DirectoryInfo[]? Sources { get; init; } = Sources;
  public DirectoryInfo? Destination { get; init; } = Destination;
  public Operation Operation { get; init; } = Operation;
  public OutputFormat OutputFormat { get; init; } = OutputFormat;
  public bool Overwrite { get; init; } = Overwrite;
  public bool DryRun { get; init; } = DryRun;
  public bool Quiet { get; init; } = Quiet;
}