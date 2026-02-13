using System.Collections.Immutable;
using System.Diagnostics;
using FileSegregator.Cli.Models;
using FileSegregator.Cli.Options;

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
  internal static readonly ImmutableArray<Status> SuccessfulStatuses = [Status.Copied, Status.Moved];
  internal TResult? Result;
  public readonly TParsedOptions ParsedOptions;
  public abstract void Process(CancellationToken cancellationToken);

  internal (Status, string) FileExist(string filePath, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    try
    {
      var fileExists = File.Exists(filePath);
      return fileExists
        ? (Status.FileFound, $"File found at '{filePath}'")
        : (Status.FileNotFound, $"File not found at '{filePath}'");
    }
    catch (Exception exception)
    {
      return (Status.FileNotFound, exception.Message);
    }
  }

  internal (Status, string) CopyOrMove(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!File.Exists(sourceFilePath))
      {
        return (Status.FileNotFound, $"Could not find file '{sourceFilePath}'.");
      }

      if (!ParsedOptions.Overwrite && File.Exists(destinationFilePath))
      {
        return (Status.IO, $"The file '{destinationFilePath}' already exists.");
      }

      switch (ParsedOptions.Operation)
      {
        case Operation.Move:
          File.Move(sourceFilePath, destinationFilePath, ParsedOptions.Overwrite);
          return (Status.Moved, $"Source: '{sourceFilePath}'");
        default:
          File.Copy(sourceFilePath, destinationFilePath, ParsedOptions.Overwrite);
          return (Status.Copied, $"Source: '{sourceFilePath}'");
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
