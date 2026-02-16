using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Services;

public abstract class BaseFileService
{
  internal static (FileStatus, string) CopyOrMove(string sourceFilePath, string destinationFilePath, Operation operation, bool overwrite, CancellationToken cancellationToken)
  {
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
    catch (OperationCanceledException operationCanceledException)
    {
      return (FileStatus.Canceled, operationCanceledException.Message);
    }
    catch (PathTooLongException pathTooLongException)
    {
      return (FileStatus.Error, pathTooLongException.Message);
    }
    catch (DirectoryNotFoundException directoryNotFoundException)
    {
      return (FileStatus.Error, directoryNotFoundException.Message);
    }
    catch (FileNotFoundException fileNotFoundException)
    {
      return (FileStatus.Error, fileNotFoundException.Message);
    }
    catch (IOException ioException)
    {
      return (FileStatus.Error, ioException.Message);
    }
    catch (ArgumentNullException argumentNullException)
    {
      return (FileStatus.Error, argumentNullException.Message);
    }
    catch (ArgumentException argumentException)
    {
      return (FileStatus.Error, argumentException.Message);
    }
    catch (UnauthorizedAccessException unauthorizedAccessException)
    {
      return (FileStatus.Error, unauthorizedAccessException.Message);
    }
    catch (NotSupportedException notSupportedException)
    {
      return (FileStatus.Error, notSupportedException.Message);
    }
    catch (Exception exception)
    {
      return (FileStatus.Error, exception.Message);
    }
  }

}
