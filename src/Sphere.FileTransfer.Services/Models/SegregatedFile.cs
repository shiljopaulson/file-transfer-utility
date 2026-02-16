namespace Sphere.FileTransfer.Services.Models;

public sealed class SegregatedFile(FileInfo file)
{
  public required FileInfo File { get; init; } = file;
  public FileStatus Status { get; set; } = FileStatus.Unprocessed;
  public string? Message { get; set; }
}