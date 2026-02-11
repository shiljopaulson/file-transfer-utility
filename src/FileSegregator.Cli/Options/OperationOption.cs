using System.CommandLine;
using FileSegregator.Cli.Constants;

namespace FileSegregator.Cli.Options;

public sealed class OperationOption : Option<Models.Operation>
{
  public OperationOption() : base(OptionNames.Operation)
  {
    Description = $"File operation ({string.Join("|", Enum.GetNames<Models.Operation>())}).";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return Models.Operation.Copy;
    };
  }
}
