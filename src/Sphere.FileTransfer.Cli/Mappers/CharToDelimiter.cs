using Microsoft.Extensions.Logging;

using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Mappers;

internal sealed class CharToDelimiter(ILogger<CharToDelimiter> logger) : IMap<char, Delimiter>
{
  private readonly ILogger<CharToDelimiter> _logger = logger;

  public Delimiter Map(char delimitedChar)
  {
    _logger.LogTrace("Entering CharToDelimiter => Map");
    return delimitedChar switch
    {
      '\t' => Delimiter.Tab,
      '|' => Delimiter.Pipe,
      _ => Delimiter.Comma
    };
  }
}