namespace Sphere.FileTransfer.Services.Models;

public enum LineStatus
{
  Unprocessed,
  Processed,
  Skipped,
  Duplicate,
  Canceled,
  Error,
}