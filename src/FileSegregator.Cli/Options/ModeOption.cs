using System.CommandLine;
using FileSegregator.Cli.Constants;

namespace FileSegregator.Cli.Options;

public sealed class ModeOption : Option<Models.Mode>
{
  public ModeOption() : base(OptionNames.Mode)
  {
    Description = $"Operation mode ({string.Join("|", Enum.GetNames<Models.Mode>())}).";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return Models.Mode.Copy;
    };
  }
}
