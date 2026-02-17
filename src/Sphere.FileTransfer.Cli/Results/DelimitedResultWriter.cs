using System.Text.Json;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Results;

public sealed class DelimitedResultWriter : IResultWriter<DelimitedFile>
{
  public void Write(DelimitedFile result, OutputFormat format, CancellationToken cancellationToken)
  {
    switch (format)
    {
      case OutputFormat.Json:
        WriteJson(result);
        break;
      case OutputFormat.Text:
      default:
        WriteText(result);
        break;
    }
  }

  private void WriteText(DelimitedFile result)
  {
    if (result is null)
    {
      return;
    }
    var rows = 0;
    if (result.Lines is not null && result.Lines.Length > 0)
    {
      rows = result.HasHeader
        ? (result.Lines.Length == 0 ? 0 : result.Lines.Length - 1)
        : result.Lines.Length;
    }
    int terminalWidth = Console.WindowWidth;
    string repeatedString = new string('─', terminalWidth - 4);
    string spaceBetween = "  ";
    Console.WriteLine();
    Console.WriteLine($"{spaceBetween}{repeatedString}");
    Console.WriteLine();
    Console.WriteLine($"{spaceBetween}File      : {result.FileFullName}");
    Console.WriteLine($"{spaceBetween}Delimiter : '{result.Delimiter}'");
    Console.WriteLine($"{spaceBetween}Lines     : {(result.HasHeader && result.Lines is not null ? result.Lines.Length - 1 : result.Lines?.Length)}");
    Console.WriteLine();
    Console.WriteLine($"{spaceBetween}{repeatedString}");

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

  private static void WriteJson(DelimitedFile result) =>
      Console.WriteLine(JsonSerializer.Serialize(result));
}