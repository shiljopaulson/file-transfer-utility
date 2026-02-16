using System.CommandLine;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

public sealed class OperationOption : Option<Operation>
{
  public OperationOption() : base(OptionNames.Operation)
  {
    Description = $"File operation ({string.Join("|", Enum.GetNames<Operation>())}).";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return Operation.Copy;
    };
  }
}
