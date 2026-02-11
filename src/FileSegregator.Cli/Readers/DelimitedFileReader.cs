using System.Diagnostics;
using System.Security;
using System.Text;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Readers;

public interface IDelimitedFileReader
{
  DelimitedFile? Read(string filePath, char delimiter, bool skipHeader, byte columnIndex, CancellationToken cancellationToken);
}
internal sealed class DelimitedFileReader : IDelimitedFileReader
{

  public DelimitedFile? Read(string filePath, char delimiter, bool skipHeader, byte columnIndex, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    var delimitedFile = new DelimitedFile { FileName = filePath, Lines = [] };
    var delimitedFileLines = new List<DelimitedFileLine>();
    var lineNumber = 0;
    try
    {
      var lookUp = new Dictionary<string, int>();
      string line;

      using StreamReader streamReader = new(filePath, true);
      while ((line = streamReader.ReadLine()) != null)
      {
        cancellationToken.ThrowIfCancellationRequested();
        lineNumber++;
        var delimitedFileLine = new DelimitedFileLine { Number = lineNumber, OriginalAt = lineNumber, Status = Status.Unprocessed, Data = line };
        if (skipHeader)
        {
          skipHeader = false;
          delimitedFileLine.Status = Status.Skipped;
          delimitedFileLines.Add(delimitedFileLine);
          continue;
        }

        var (status, message, column) = GetColumn(delimitedFileLine.Data, delimiter, columnIndex);
        delimitedFileLine.ColumnValue = column;
        if (status == Status.Error || string.IsNullOrWhiteSpace(column))
        {
          delimitedFileLine.Status = Status.Error;
          delimitedFileLine.Message = message;
          delimitedFileLines.Add(delimitedFileLine);
          continue;
        }

        if (lookUp.TryGetValue(column, out int value))
        {
          delimitedFileLine.OriginalAt = value;
          delimitedFileLine.Status = Status.Duplicate;
          delimitedFileLine.Message = $"Duplicate entry found for '{column}' first entry found at line number '{value}'";
          delimitedFileLines.Add(delimitedFileLine);
          continue;
        }
        lookUp[column] = delimitedFileLine.Number;
        delimitedFileLines.Add(delimitedFileLine);
      }
    }
    catch (OperationCanceledException operationCanceledException)
    {
      delimitedFile.Status = Status.OperationCanceled;
      delimitedFile.Message = $"While Processing line number {lineNumber}, {operationCanceledException.Message}";
    }
    catch (PathTooLongException pathTooLongException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = pathTooLongException.Message;
    }
    catch (DirectoryNotFoundException pathTooLongException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = pathTooLongException.Message;
    }
    catch (FileNotFoundException fileNotFoundException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = fileNotFoundException.Message;
    }
    catch (IOException ioException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = $"While Processing line number {lineNumber}, {ioException.Message}";
    }
    catch (OutOfMemoryException outOfMemoryException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = $"While Processing line number {lineNumber}, {outOfMemoryException.Message}";
    }
    catch (ArgumentNullException argumentNullException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = argumentNullException.Message;
    }
    catch (ArgumentException argumentException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = argumentException.Message;
    }
    catch (UnauthorizedAccessException unauthorizedAccessException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = unauthorizedAccessException.Message;
    }
    catch (NotSupportedException notSupportedException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = notSupportedException.Message;
    }
    catch (SecurityException securityException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = securityException.Message;
    }
    catch (Exception exception)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Message = exception.Message;
    }
    delimitedFile.Lines = delimitedFileLines.Count > 0 ? delimitedFileLines.ToArray() : [];
    return delimitedFile;
  }

  private static (Status, string?, string?) GetColumn(string lineData, char delimiter, int columnIndex)
  {
    if (string.IsNullOrWhiteSpace(lineData))
    {
      return (Status.Error, "Blank line", null);
    }
    string[] columns = lineData.Split(delimiter);
    if (columns.Length < columnIndex)
    {
      return (Status.Error, "Has less number of columns or missing column", null);
    }
    var column = $"{columns[columnIndex],-15}".Trim();
    if (string.IsNullOrWhiteSpace(column))
    {
      return (Status.Error, "Missing entry in the column", column);
    }
    return (Status.Unprocessed, null, column);
  }
}
