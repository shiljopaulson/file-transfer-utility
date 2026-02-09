using System.CommandLine;
using FileSegregator.Cli.Constants;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Options;

public sealed class DelimiterOption : Option<Delimiter>
{
  public DelimiterOption() : base(OptionNames.Delimiter)
  {
    Description = $"Field delimiter character ({string.Join("|", Enum.GetNames<Delimiter>())}).";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return Delimiter.Comma;
    };
  }
}
