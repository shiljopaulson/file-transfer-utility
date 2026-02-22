using System.Collections.Immutable;

namespace Sphere.FileTransfer.Services.Models;

public sealed class DelimitedFile
{
  public required string FileFullName { get; init; }
  public char Delimiter { get; set; } = ',';
  public bool HasHeader { get; set; }
  public FileStatus Status { get; set; } = FileStatus.Unprocessed;
  public string? Message { get; set; }
  public ImmutableArray<DelimitedFileLine> Lines { get; set; }
}
