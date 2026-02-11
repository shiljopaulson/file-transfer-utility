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

  public override string ToString()
  {
    return $"Number:{Number}, Data:{Data}, Column:{ColumnValue}, OriginalAt:{OriginalAt}, Status:{Status}, Message:{Message},";
  }
}