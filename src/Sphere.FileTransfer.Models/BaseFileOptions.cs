namespace Sphere.FileTransfer.Models;

public abstract class BaseFileOptions(DirectoryInfo[] Sources, DirectoryInfo Destination, Operation Operation, bool Overwrite, bool DryRun)
{
  public DirectoryInfo[] Sources { get; init; } = Sources;
  public DirectoryInfo Destination { get; init; } = Destination;
  public Operation Operation { get; init; } = Operation;
  public bool Overwrite { get; init; } = Overwrite;
  public bool DryRun { get; init; } = DryRun;
}