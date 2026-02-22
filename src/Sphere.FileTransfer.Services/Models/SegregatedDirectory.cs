using System.Collections.Immutable;

namespace Sphere.FileTransfer.Services.Models;

public sealed class SegregatedDirectory
{
  public required string DirectoryPath { get; set; }
  public DirectoryStatus Status { get; set; } = DirectoryStatus.Unprocessed;
  public string? Message { get; set; }
  public ImmutableArray<SegregatedFile> Files { get; set; } = [];
}