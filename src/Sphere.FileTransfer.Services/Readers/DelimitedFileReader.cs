using System.Security;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Services.Readers;

public interface IDelimitedFileReader
{
  DelimitedFile Read(string fileFullName, char delimiter, bool hasHeader, CancellationToken cancellationToken);
}

public sealed class DelimitedFileReader : IDelimitedFileReader
{
  public DelimitedFile Read(string fileFullName, char delimiter, bool hasHeader, CancellationToken cancellationToken)
  {
    Console.WriteLine("Readers.Read");
    cancellationToken.ThrowIfCancellationRequested();
    var delimitedFile = new DelimitedFile { FileFullName = fileFullName, Delimiter = delimiter, HasHeader = hasHeader };
    var lines = new List<DelimitedFileLine>();
    var lineNumber = 0;
    var skipHeader = delimitedFile.HasHeader;
    try
    {
      var lookUp = new Dictionary<string, int>();
      string line;

      using StreamReader streamReader = new(delimitedFile.FileFullName, true);
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
    }
    catch (OperationCanceledException operationCanceledException)
    {
      delimitedFile.Status = FileStatus.Canceled;
      delimitedFile.Message = $"{nameof(OperationCanceledException)} while reading line number {lineNumber}, {operationCanceledException.Message}";
    }
    catch (PathTooLongException pathTooLongException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = pathTooLongException.Message;
    }
    catch (DirectoryNotFoundException directoryNotFoundException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(DirectoryNotFoundException)} while reading line number {lineNumber}, {directoryNotFoundException.Message}";
    }
    catch (FileNotFoundException fileNotFoundException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(FileNotFoundException)} while reading line number {lineNumber}, {fileNotFoundException.Message}";
    }
    catch (IOException ioException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(IOException)} while reading line number {lineNumber}, {ioException.Message}";
    }
    catch (OutOfMemoryException outOfMemoryException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(OutOfMemoryException)} while reading line number {lineNumber}, {outOfMemoryException.Message}";
    }
    catch (ArgumentNullException argumentNullException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = argumentNullException.Message;
    }
    catch (ArgumentException argumentException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = argumentException.Message;
    }
    catch (UnauthorizedAccessException unauthorizedAccessException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(UnauthorizedAccessException)} while reading line number {lineNumber}, {unauthorizedAccessException.Message}";
    }
    catch (NotSupportedException notSupportedException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(NotSupportedException)} while reading line number {lineNumber}, {notSupportedException.Message}";
    }
    catch (SecurityException securityException)
    {
      delimitedFile.Status = FileStatus.Error;
      delimitedFile.Message = $"{nameof(SecurityException)} while reading line number {lineNumber}, {securityException.Message}";
    }
    catch (Exception exception)
    {
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
