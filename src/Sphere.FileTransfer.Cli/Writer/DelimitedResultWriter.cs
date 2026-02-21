using System.Text.Json;
using Sphere.FileTransfer.Cli.Mappers;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Writer;

public sealed class DelimitedResultWriter : IResultWriter<DelimitedFile>
{
  private readonly IMap<char, Delimiter> _charToDelimiterMapper;
  public DelimitedResultWriter(IMap<char, Delimiter> charToDelimiterMapper)
  {
    _charToDelimiterMapper = charToDelimiterMapper;
  }
  public void Write(DelimitedFile result, OutputFormat format, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (result is null)
    {
      return;
    }
    switch (format)
    {
      case OutputFormat.Json:
        WriteJson(result, cancellationToken);
        break;
      case OutputFormat.Text:
      default:
        WriteText(result, cancellationToken);
        break;
    }
  }

  private void WriteText(DelimitedFile result, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    var rows = 0;
    if (result.Lines is not null && result.Lines.Length > 0)
    {
      rows = result.HasHeader
        ? (result.Lines.Length == 0 ? 0 : result.Lines.Length - 1)
        : result.Lines.Length;
    }
    int terminalWidth = Console.WindowWidth;
    int spacing = 4;
    string repeatedString = new('─', terminalWidth - spacing);
    string padding = new(' ', spacing / 2);
    Console.WriteLine();
    Console.WriteLine($"{padding}{repeatedString}");
    Console.WriteLine();
    Console.WriteLine($"{padding}{padding}File      : {result.FileFullName}");
    Console.WriteLine($"{padding}{padding}Delimiter : '{result.Delimiter}'({_charToDelimiterMapper.Map(result.Delimiter)})");
    Console.WriteLine($"{padding}{padding}Lines     : {(result.HasHeader && result.Lines is not null ? result.Lines.Length - 1 : result.Lines?.Length)}");
    Console.WriteLine();
    Console.WriteLine($"{padding}{repeatedString}");
    Console.WriteLine();

    if (result.Lines is null
      || result.Lines.Length == 0)
    {
      return;
    }
    foreach (var item in result.Lines)
    {
      switch (item.Status)
      {
        case LineStatus.Skipped:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}", ConsoleColor.DarkGreen);
          break;
        case LineStatus.Unprocessed:
        case LineStatus.Canceled:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, Message: {item.Message}", ConsoleColor.Cyan);
          break;
        case LineStatus.Duplicate:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, Message: {item.Message}", ConsoleColor.Yellow);
          break;
        case LineStatus.Processed:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, Message: {item.Message}", ConsoleColor.Green);
          break;
        default:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, Message: {item.Message}", ConsoleColor.Red);
          break;
      }
    }

    Console.WriteLine();
  }

  private static void WriteJson(DelimitedFile result, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    Console.WriteLine(JsonSerializer.Serialize(result));
  }
}
