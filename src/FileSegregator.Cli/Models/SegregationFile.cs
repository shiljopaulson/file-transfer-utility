using System.Text.Json.Serialization;

namespace FileSegregator.Cli.Models;

public class SegregationFile
{
  public string FileName { get; set; } = string.Empty;
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public Models.Status Status { get; set; } = Models.Status.Unprocessed;
  public string? Error { get; set; }
}
