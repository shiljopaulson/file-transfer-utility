using System.CommandLine;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Options;

public sealed class OutputFormatOption : Option<OutputFormat>
{
  public OutputFormatOption() : base("--output-format")
  {
    Description = $"Output format ({string.Join("|", Enum.GetNames<OutputFormat>())}), default: {DefaultValueFactory}";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return OutputFormat.Text;
    };
  }
}
