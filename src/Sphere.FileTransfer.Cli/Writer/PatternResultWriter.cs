using System.Collections.Immutable;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Writer;

internal sealed class PatternResultWriter(ILogger<PatternResultWriter> logger) : IResultWriter<ImmutableArray<SegregatedDirectory>>
{
  private readonly ILogger<PatternResultWriter> _logger = logger;

  public void Write(ImmutableArray<SegregatedDirectory> result, OutputFormat format, CancellationToken cancellationToken)
  {
    _logger.LogTrace("Entering PatternResultWriter => Write");
    if (result.Length == 0)
    {
      return;
    }
    switch (format)
    {
      case OutputFormat.Json:
        WriteJson(result, cancellationToken);
        break;
      default:
        WriteText(result, cancellationToken);
        break;
    }
  }

  private static void WriteText(ImmutableArray<SegregatedDirectory> result, CancellationToken cancellationToken)
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

    for (int i = 0; i < result.Length; i++)
    {
      SegregatedDirectory? directory = result[i];
      if (directory.Files.Length == 0)
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

  private static void WriteJson(ImmutableArray<SegregatedDirectory> result, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    Console.WriteLine(JsonSerializer.Serialize(result));
  }
}