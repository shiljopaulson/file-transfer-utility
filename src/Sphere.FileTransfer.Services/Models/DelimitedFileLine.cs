namespace Sphere.FileTransfer.Services.Models;

public class DelimitedFileLine
{
  public int Number { get; set; }
  public string? Data { get; set; }
  public string[] DelimitedFields { get; set; } = [];
  public LineStatus Status { get; set; } = LineStatus.Unprocessed;
  public string? Message { get; set; }
}