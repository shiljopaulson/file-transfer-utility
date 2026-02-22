using System.Collections.Immutable;

namespace Sphere.FileTransfer.Models;

public sealed class PatternOptions(ImmutableArray<DirectoryInfo> Sources, DirectoryInfo Destination, Operation Mode, bool Overwrite, bool DryRun, string SearchPattern) : BaseFileOptions(Sources, Destination, Mode, Overwrite, DryRun)
{
  public string SearchPattern { get; init; } = SearchPattern;
}