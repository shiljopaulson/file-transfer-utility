using System.Text.Json.Serialization;

namespace FileSegregator.Cli.Models;

public class SegregationFile
{
  public required FileInfo File { get; set; }
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public Models.Status Status { get; set; } = Models.Status.Unprocessed;
  public string? Message { get; set; } = string.Empty;

  public static SegregationFile New(FileInfo fileInfo, Models.Status status = Models.Status.Unprocessed, string message = "")
  {
    return new SegregationFile { File = fileInfo, Status = status, Message = message };
  }
}
