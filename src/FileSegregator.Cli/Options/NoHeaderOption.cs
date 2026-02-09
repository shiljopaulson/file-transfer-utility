using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class NoHeaderOption : Option<bool>
{
  public NoHeaderOption() : base("--no-header")
  {
    var defaultValue = false;
    Description = $"Input file has no header row, default: {defaultValue}";
    DefaultValueFactory = (result) =>
    {
      return defaultValue;
    };
  }
}
