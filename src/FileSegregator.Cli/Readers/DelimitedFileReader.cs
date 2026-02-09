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
  private static DelimitedFile ReadAllLines(string filePath, Encoding encoding, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    var delimitedFile = new DelimitedFile { FileName = filePath };
    try
    {
      var lines = File.ReadAllLines(filePath, encoding);
      if (lines is null || lines.Length == 0)
      {
        delimitedFile.Status = Status.Error;
        delimitedFile.Error = "Empty file";
        return delimitedFile;
      }

      delimitedFile.Lines = new DelimitedFileLine[lines.Length];
      var lineNumber = 0;
      for (var i = 0; i < lines.Length; i++)
      {
        lineNumber++;
        delimitedFile.Lines[i] = new DelimitedFileLine { Number = lineNumber, Data = lines[i] };
      }
      return delimitedFile;
    }
    catch (OperationCanceledException operationCanceledException)
    {
      delimitedFile.Status = Status.OperationCanceled;
      delimitedFile.Error = operationCanceledException.Message;
    }
    catch (PathTooLongException pathTooLongException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = pathTooLongException.Message;
    }
    catch (DirectoryNotFoundException pathTooLongException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = pathTooLongException.Message;
    }
    catch (FileNotFoundException fileNotFoundException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = fileNotFoundException.Message;
    }
    catch (IOException ioException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = ioException.Message;
    }
    catch (ArgumentNullException argumentNullException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = argumentNullException.Message;
    }
    catch (ArgumentException argumentException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = argumentException.Message;
    }
    catch (UnauthorizedAccessException unauthorizedAccessException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = unauthorizedAccessException.Message;
    }
    catch (NotSupportedException notSupportedException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = notSupportedException.Message;
    }
    catch (SecurityException securityException)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = securityException.Message;
    }
    catch (Exception exception)
    {
      delimitedFile.Status = Status.Failure;
      delimitedFile.Error = exception.Message;
    }
    return delimitedFile;
  }

  public DelimitedFile? Read(string filePath, char delimiter, bool skipHeader, byte columnIndex, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    Trace.TraceInformation("Reading Delimited file");
    var delimitedFile = ReadAllLines(filePath, Encoding.UTF8, cancellationToken);
    if (delimitedFile is null || delimitedFile.Lines is null || delimitedFile.Lines.Length == 0)
    {
      return delimitedFile;
    }
    var lookUp = new Dictionary<string, int[]>();
    for (var i = 0; i < delimitedFile.Lines.Length; i++)
    {
      Trace.TraceInformation($"Preprocessing Delimited file's line number {i}");
      cancellationToken.ThrowIfCancellationRequested();
      if (skipHeader)
      {
        skipHeader = false;
        delimitedFile.Lines[i].Status = Status.Skipped;
        continue;
      }
      var lineData = delimitedFile.Lines[i].Data;
      if (string.IsNullOrWhiteSpace(lineData))
      {
        delimitedFile.Lines[i].Status = Status.Error;
        delimitedFile.Lines[i].Error = "Empty line";
        continue;
      }
      string[] columns = lineData.Split(delimiter);
      if (columns.Length < columnIndex)
      {
        delimitedFile.Lines[i].Status = Status.Error;
        delimitedFile.Lines[i].Error = "Has less number of Columns or missing";
        continue;
      }
      var column = $"{columns[columnIndex],-15}".Trim();
      if (string.IsNullOrWhiteSpace(column))
      {
        delimitedFile.Lines[i].Status = Status.Error;
        delimitedFile.Lines[i].Error = "Column Value Missing";
        continue;
      }
      if (lookUp.TryGetValue(column, out int[]? value))
      {
        lookUp[column] = [.. value, delimitedFile.Lines[i].Number];
        delimitedFile.Lines[i].Status = Status.Duplicate;
        delimitedFile.Lines[i].Error = $"Duplicate entries found for '{column}' at lines '{string.Join(", ", lookUp[column])}'";
        continue;
      }
      delimitedFile.Lines[i].FileName = column;
      lookUp[column] = [delimitedFile.Lines[i].Number];
    }
    return delimitedFile;
  }
}
