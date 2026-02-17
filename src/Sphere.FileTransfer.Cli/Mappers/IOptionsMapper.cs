using System.CommandLine;

namespace Sphere.FileTransfer.Cli.Mappers;

public interface IOptionsMapper<TOptions> : IMap<ParseResult, TOptions>
{
  new TOptions Map(ParseResult parseResult);
}
