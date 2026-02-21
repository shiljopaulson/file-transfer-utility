using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Services;

public abstract class BaseFileService<T>
{
  internal readonly ILogger<T> _logger;

  public BaseFileService(ILogger<T> logger)
  {
    _logger = logger;
  }

  internal (FileStatus, string) CopyOrMove(string sourceFilePath, string destinationFilePath, Operation operation, bool overwrite, CancellationToken cancellationToken)
  {
    _logger.LogTrace("Entering IDelimitedService => Process");
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!File.Exists(sourceFilePath))
      {
        return (FileStatus.Error, $"Could not find file '{sourceFilePath}' at source.");
      }

      if (!overwrite && File.Exists(destinationFilePath))
      {
        return (FileStatus.Error, $"The file '{destinationFilePath}' already exists at destination.");
      }

      switch (operation)
      {
        case Operation.Move:
          File.Move(sourceFilePath, destinationFilePath, overwrite);
          break;
        default:
          File.Copy(sourceFilePath, destinationFilePath, overwrite);
          break;
      }
      return (FileStatus.Processed, $"Source: '{sourceFilePath}'");
    }
    catch (OperationCanceledException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Canceled, exception.Message);
    }
    catch (PathTooLongException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
    catch (DirectoryNotFoundException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
    catch (FileNotFoundException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
    catch (IOException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
    catch (ArgumentNullException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
    catch (ArgumentException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
    catch (UnauthorizedAccessException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
    catch (NotSupportedException exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception.Message);
      return (FileStatus.Error, exception.Message);
    }
  }
}
