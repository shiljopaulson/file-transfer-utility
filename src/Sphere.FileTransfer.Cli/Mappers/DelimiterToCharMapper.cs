using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Mappers;

internal sealed class DelimiterToChar : IMap<Delimiter, char>
{
  public char Map(Delimiter delimiter)
  {
    return delimiter switch
    {
      Delimiter.Tab => '\t',
      Delimiter.Pipe => '|',
      _ => ',',
    };
  }
}