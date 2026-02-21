using System.Text.Json;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Writer;

public sealed class PatternResultWriter : IResultWriter<SegregatedDirectory[]>
{
  public void Write(SegregatedDirectory[] result, OutputFormat format, CancellationToken cancellationToken)
  {
    if (result is null || result.Length == 0)
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

  private void WriteText(SegregatedDirectory[] result, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    var sourceDirectories = result.Select(x => x.DirectoryPath).ToArray();
    var totalFiles = result.Sum(x => x.Files.Length);
    var totalUniqueFiles = result.SelectMany(x => x.Files).Select(x => x.File.Name).Distinct().Count();
    int terminalWidth = Console.WindowWidth;
    int spacing = 4;
    string repeatedString = new('─', terminalWidth - spacing);
    string padding = new(' ', spacing / 2);
    Console.WriteLine();
    Console.WriteLine($"{padding}{repeatedString}");
    Console.WriteLine();
    Console.WriteLine($"{padding}{padding}Sources             : {string.Join(", ", sourceDirectories)}");
    Console.WriteLine($"{padding}{padding}Total files         : {totalFiles}");
    Console.WriteLine($"{padding}{padding}Total unique files  : {totalUniqueFiles}");
    Console.WriteLine();
    Console.WriteLine($"{padding}{repeatedString}");
    Console.WriteLine();

    foreach (var directory in result)
    {
      if (directory.Files is null || directory.Files.Length == 0)
      {
        continue;
      }
      foreach (var file in directory.Files)
      {
        switch (file.Status)
        {
          case FileStatus.Processed:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File.Name}", ConsoleColor.Green);
            break;
          case FileStatus.Unprocessed:
          case FileStatus.Canceled:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File.Name}", ConsoleColor.Cyan);
            break;
          default:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File.Name} - ({file.Message})", ConsoleColor.Red);
            break;
        }
      }
    }
  }

  private static void WriteJson(SegregatedDirectory[] result, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    Console.WriteLine(JsonSerializer.Serialize(result));
  }
}