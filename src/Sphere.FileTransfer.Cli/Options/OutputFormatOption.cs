using System.CommandLine;

using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Options;

internal sealed class OutputFormatOption : Option<OutputFormat>
{
  public OutputFormatOption() : base(OptionNames.OutputFormat)
  {
    Description = $"Output format ({string.Join("|", Enum.GetNames<OutputFormat>())}).";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return OutputFormat.Text;
    };
  }
}
