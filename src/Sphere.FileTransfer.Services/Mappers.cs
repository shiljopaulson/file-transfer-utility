using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Services;

public class Mappers
{
  public static LineStatus Map(FileStatus fileStatus)
  {
    var lineStatus = fileStatus switch
    {
      FileStatus.Canceled => LineStatus.Canceled,
      FileStatus.Processed => LineStatus.Processed,
      FileStatus.Unprocessed => LineStatus.Unprocessed,
      _ => LineStatus.Error,
    };
    return lineStatus;
  }
}
