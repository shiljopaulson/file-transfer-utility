namespace Sphere.FileTransfer.Services.Models;

public sealed class SegregatedDirectory(string directoryPath)
{
  public required string DirectoryPath { get; init; } = directoryPath;
  public FileStatus Status { get; set; } = FileStatus.Unprocessed;
  public string? Message { get; set; }
  public SegregatedFile[]? Files { get; set; }
}
