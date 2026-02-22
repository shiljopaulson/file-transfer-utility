using System.Collections.Immutable;

namespace Sphere.FileTransfer.Services.Models;

public sealed class DelimitedFileLine
{
  public int Number { get; set; }
  public string? Data { get; set; }
  public ImmutableArray<string> DelimitedFields { get; set; } = [];
  public LineStatus Status { get; set; } = LineStatus.Unprocessed;
  public string? Message { get; set; }

  public override string ToString()
  {
    return $"Number:{Number}, Data: {Data}, Status:{Status}, Message:{Message}";
  }
}