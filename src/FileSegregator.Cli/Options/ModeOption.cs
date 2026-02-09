using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class ModeOption : Option<Models.Mode>
{
  public ModeOption() : base("--mode")
  {
    Description = $"Operation mode ({string.Join("|", Enum.GetNames<Models.Mode>())}), default: {DefaultValueFactory}";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return Models.Mode.Copy;
    };
  }
}
