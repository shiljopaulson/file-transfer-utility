using System.Text.Json.Serialization;
using System.Linq;

namespace FileSegregator.Cli.Models;

public class SegregationDirectory
{
  public DirectoryInfo? Directory { get; set; }

  [JsonConverter(typeof(JsonStringEnumConverter))]
  public Models.Status Status { get; set; } = Models.Status.Unprocessed;
  public string? Message { get; set; }
  public SegregationFile[] Files { get; set; } = [];
}