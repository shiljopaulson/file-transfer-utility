namespace FileSegregator.Cli.Models;

public class DelimitedFileLine : SegregationFile
{
  public int Number { get; set; }
  public string? Data { get; set; }
}