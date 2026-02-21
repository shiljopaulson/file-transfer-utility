using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Mappers;

public class CharToDelimiter : IMap<char, Delimiter>
{
  public Delimiter Map(char delimitedChar)
  {
    return delimitedChar switch
    {
      '\t' => Delimiter.Tab,
      '|' => Delimiter.Pipe,
      _ => Delimiter.Comma
    };
  }
}