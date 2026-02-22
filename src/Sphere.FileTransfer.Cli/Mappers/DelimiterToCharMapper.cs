using Microsoft.Extensions.Logging;

using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Mappers;

internal sealed class DelimiterToChar(ILogger<DelimiterToChar> logger) : IMap<Delimiter, char>
{
  private readonly ILogger<DelimiterToChar> _logger = logger;

  public char Map(Delimiter delimiter)
  {
    _logger.LogTrace("Entering DelimiterToChar => Map");
    return delimiter switch
    {
      Delimiter.Tab => '\t',
      Delimiter.Pipe => '|',
      _ => ',',
    };
  }
}