using System.CommandLine;

namespace Sphere.FileTransfer.Cli.Mappers;

internal interface IOptionsMapper<out TOptions> : IMap<ParseResult, TOptions>
{
  new TOptions Map(ParseResult parseResult);
}
