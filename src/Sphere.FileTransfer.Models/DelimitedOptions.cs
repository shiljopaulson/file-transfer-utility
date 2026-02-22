using System.Collections.Immutable;

namespace Sphere.FileTransfer.Models;

public sealed class DelimitedOptions(ImmutableArray<DirectoryInfo> Sources, DirectoryInfo Destination, Operation Operation, bool Overwrite, bool DryRun, FileInfo File, byte Column, bool NoHeader, char Delimiter) : BaseFileOptions(Sources, Destination, Operation, Overwrite, DryRun)
{
  public FileInfo File { get; init; } = File;
  public byte Column { get; init; } = Column;
  public bool NoHeader { get; init; } = NoHeader;
  public char Delimiter { get; init; } = Delimiter;
}