using System.CommandLine;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Options;

public sealed class DelimiterOption : Option<Delimiter>
{
  public DelimiterOption() : base("--delimiter")
  {
    Description = $"Field delimiter character ({string.Join("|", Enum.GetNames<Delimiter>())}), default: {DefaultValueFactory}";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return Delimiter.Comma;
    };
  }
}
