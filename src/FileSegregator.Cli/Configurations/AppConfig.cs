using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Configurations;

public class AppConfig
{
  public DirectoryInfo[]? Sources { get; set; }
  public DirectoryInfo? Destination { get; set; }
  public Operation Operation { get; set; }
  public OutputFormat OutputFormat { get; set; }
  public bool? Quiet { get; set; }
  public bool? Overwrite { get; set; }
  public bool? DryRun { get; set; }

  public Delimiter? Delimiter { get; set; }
  public FileInfo? DelimiterFile { get; set; }
  public byte? Column { get; set; }

  public string? SearchPattern { get; set; }
}
