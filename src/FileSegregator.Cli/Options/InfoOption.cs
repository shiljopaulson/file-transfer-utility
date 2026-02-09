using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class InfoOption : Option<bool>
{
  public InfoOption() : base("--info")
  {
    var defaultValue = false;
    Description = $"Information about the application";
    DefaultValueFactory = (result) =>
    {
      return defaultValue;
    };
  }
}
