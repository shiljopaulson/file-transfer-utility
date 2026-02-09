using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Mappers;

public class EnumMappers
{
  public static char Map(Delimiter delimiter)
  {
    return delimiter switch
    {
      Delimiter.Tab => '\t',
      Delimiter.Pipe => '|',
      _ => ',',
    };
  }
}
