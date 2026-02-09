using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class QuietOption : Option<bool>
{
  public QuietOption() : base("--quiet")
  {
    var defaultValue = false;
    Description = $"Execute without printing, default: {defaultValue}";
    DefaultValueFactory = (result) =>
    {
      return defaultValue;
    };
  }
}
