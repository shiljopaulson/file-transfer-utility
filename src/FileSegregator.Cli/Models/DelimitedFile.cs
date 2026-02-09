namespace FileSegregator.Cli.Models;

public sealed class DelimitedFile : SegregationFile
{
  public DelimitedFileLine[]? Lines { get; set; }
}
