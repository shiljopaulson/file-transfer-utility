namespace Sphere.FileTransfer.Services.Models;

public enum FileStatus
{
  Unprocessed,
  Processed,
  Canceled,
  Error,
  Duplicate
}