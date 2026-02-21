using System.Security;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Services.Readers;

public interface IDelimitedReader
{
  DelimitedFile Read(string fileFullName, char delimiter, bool hasHeader, CancellationToken cancellationToken);
}

public sealed class DelimitedReader : IDelimitedReader
{
  private readonly ILogger<DelimitedReader> _logger;
  public DelimitedReader(ILogger<DelimitedReader> logger)
  {
    _logger = logger;
  }

  public DelimitedFile Read(string fileFullName, char delimiter, bool hasHeader, CancellationToken cancellationToken)
  {
    _logger.LogTrace("Entering IDelimitedReader => Read");
    cancellationToken.ThrowIfCancellationRequested();
    var delimitedFile = new DelimitedFile { FileFullName = fileFullName, Delimiter = delimiter, HasHeader = hasHeader };
    var lines = new List<DelimitedFileLine>();
    var lineNumber = 0;
    var skipHeader = delimitedFile.HasHeader;
    try
    {
      string line;

      using StreamReader streamReader = new(delimitedFile.FileFullName, true);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
      while ((line = streamReader.ReadLine()) != null)
      {
        cancellationToken.ThrowIfCancellationRequested();
        lineNumber++;

        var delimitedFields = string.IsNullOrWhiteSpace(line) ? [] : line.Split(delimitedFile.Delimiter);
        var delimitedFileLine = new DelimitedFileLine { Number = lineNumber, Data = line, DelimitedFields = delimitedFields };
        if (delimitedFields.Length == 0)
        {
          delimitedFileLine.Status = LineStatus.Error;
          delimitedFileLine.Message = "Empty line";
        }
        if (skipHeader)
        {
          skipHeader = false;
          delimitedFileLine.Status = delimitedFileLine.Status == LineStatus.Error ? LineStatus.Error : LineStatus.Skipped;
        }

        if (cancellationToken.IsCancellationRequested)
        {
          delimitedFileLine.Status = LineStatus.Canceled;
        }
        lines.Add(delimitedFileLine);
      }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }
    catch (OperationCanceledException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Canceled;
      delimitedFile.Message = $"{nameof(OperationCanceledException)} while reading line number {lineNumber}, {exception.Message}";
    }
    catch (PathTooLongException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = exception.Message;
    }
    catch (DirectoryNotFoundException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(DirectoryNotFoundException)} while reading line number {lineNumber}, {exception.Message}";
    }
    catch (FileNotFoundException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(FileNotFoundException)} while reading line number {lineNumber}, {exception.Message}";
    }
    catch (IOException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(IOException)} while reading line number {lineNumber}, {exception.Message}";
    }
    catch (OutOfMemoryException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(OutOfMemoryException)} while reading line number {lineNumber}, {exception.Message}";
    }
    catch (ArgumentNullException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = exception.Message;
    }
    catch (ArgumentException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = exception.Message;
    }
    catch (UnauthorizedAccessException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(UnauthorizedAccessException)} while reading line number {lineNumber}, {exception.Message}";
    }
    catch (NotSupportedException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(NotSupportedException)} while reading line number {lineNumber}, {exception.Message}";
    }
    catch (SecurityException exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(SecurityException)} while reading line number {lineNumber}, {exception.Message}";
    }
    catch (Exception exception)
    {
      _logger.LogError(exception.Message);
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = exception.Message;
    }
    finally
    {
      delimitedFile.Lines = lines.Count > 0 ? [.. lines] : [];
    }
    return delimitedFile;
  }
}
