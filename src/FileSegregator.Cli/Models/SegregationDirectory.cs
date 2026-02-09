using System.Text.Json.Serialization;

namespace FileSegregator.Cli.Models;

public class SegregationDirectory
{
  public required string[] DirectoryNames { get; set; }
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public Models.Status Status { get; set; } = Models.Status.Unprocessed;
  public string? Error { get; set; }
  public SegregationFile[] Files { get; set; } = [];
}
