using System.Text.Json.Serialization;

namespace FileSegregator.Cli.Models;

public sealed class DelimitedFile
{
  public string FileName { get; set; } = string.Empty;
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public Models.Status Status { get; set; } = Models.Status.Unprocessed;
  public string? Message { get; set; }
  public DelimitedFileLine[]? Lines { get; set; }

  public override string ToString()
  {
    return $"FileName:{FileName}, Status:{Status}, Message:{Message}, Lines:[{Lines}],";
  }
}
