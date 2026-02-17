namespace Sphere.FileTransfer.Services.Models;

public sealed class SegregatedFile
{
  public Guid Id { get; init; } = Guid.NewGuid();
  public required FileInfo File { get; set; }
  public FileStatus Status { get; set; } = FileStatus.Unprocessed;
  public string? Message { get; set; }
}