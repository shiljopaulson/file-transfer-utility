namespace Sphere.FileTransfer.Services.Models;

public enum DirectoryStatus
{
  Unprocessed,
  Processed,
  Canceled,
  Error,
  NoMatchingFiles
}