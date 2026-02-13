using System.Text.Json.Serialization;

namespace FileSegregator.Cli.Models;

public class DelimitedFileLine
{
  public required int Number { get; set; }
  public required string Data { get; set; }
  public string? ColumnValue { get; set; }
  public int OriginalAt { get; set; }
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public Models.Status Status { get; set; } = Models.Status.Unprocessed;
  public string? Message { get; set; }
}