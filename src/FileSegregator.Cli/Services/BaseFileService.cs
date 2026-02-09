using System.Diagnostics;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Services;

public interface IBaseFileService
{
  void Process(CancellationToken cancellationToken);
}

public abstract class BaseFileService<TParsedOptions, TResult> : IBaseFileService where TParsedOptions : BaseFileOptions where TResult : class
{
  public BaseFileService(TParsedOptions parsedOptions)
  {
    ArgumentNullException.ThrowIfNull(parsedOptions);
    ParsedOptions = parsedOptions;
  }
  internal TResult? Result;
  public readonly TParsedOptions ParsedOptions;
  public abstract void Process(CancellationToken cancellationToken);

  internal (Status, string?) CopyOrMove(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      var fileInfo = new FileInfo(sourceFilePath);
      if (!fileInfo.Exists && ParsedOptions.DryRun)
      {
        return (Status.FileNotFound, $"Could not find file '{sourceFilePath}'.");
      }

      fileInfo = new FileInfo(destinationFilePath);
      if (fileInfo.Exists && (ParsedOptions.DryRun || !ParsedOptions.Overwrite))
      {
        return (Status.IO, $"The file '{destinationFilePath}' already exists.");
      }
      switch (ParsedOptions.Mode)
      {
        case Mode.Move:
          File.Move(sourceFilePath, destinationFilePath, ParsedOptions.Overwrite);
          return (Status.Moved, null);
        default:
          File.Copy(sourceFilePath, destinationFilePath, ParsedOptions.Overwrite);
          return (Status.Copied, null);
      }
    }
    catch (OperationCanceledException operationCanceledException)
    {
      Trace.TraceInformation(operationCanceledException.Message);
      return (Status.OperationCanceled, operationCanceledException.Message);
    }
    catch (PathTooLongException pathTooLongException)
    {
      return (Status.PathTooLong, pathTooLongException.Message);
    }
    catch (DirectoryNotFoundException directoryNotFoundException)
    {
      return (Status.DirectoryNotFound, directoryNotFoundException.Message);
    }
    catch (FileNotFoundException fileNotFoundException)
    {
      return (Status.FileNotFound, fileNotFoundException.Message);
    }
    catch (IOException ioException)
    {
      return (Status.IO, ioException.Message);
    }
    catch (ArgumentNullException argumentNullException)
    {
      return (Status.ArgumentNull, argumentNullException.Message);
    }
    catch (ArgumentException argumentException)
    {
      return (Status.Argument, argumentException.Message);
    }
    catch (UnauthorizedAccessException unauthorizedAccessException)
    {
      return (Status.UnauthorizedAccess, unauthorizedAccessException.Message);
    }
    catch (NotSupportedException notSupportedException)
    {
      return (Status.NotSupported, notSupportedException.Message);
    }
    catch (Exception exception)
    {
      return (Status.Error, exception.Message);
    }
  }
}
