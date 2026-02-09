using System.CommandLine;
using FileSegregator.Cli.Constants;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Options;

public sealed class OutputFormatOption : Option<OutputFormat>
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
