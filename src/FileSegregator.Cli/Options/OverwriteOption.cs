using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class OverwriteOption : Option<bool>
{
  public OverwriteOption() : base("--overwrite")
  {
    var defaultValue = false;
    Description = $"Overwrite existing files in destination, default: {defaultValue}";
    DefaultValueFactory = (result) =>
    {
      return defaultValue;
    };
  }
}
