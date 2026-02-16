using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Options;

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
