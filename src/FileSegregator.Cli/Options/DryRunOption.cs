using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class DryRunOption : Option<bool>
{
  public DryRunOption() : base("--dry-run")
  {
    var defaultValue = false;
    Description = $"Do not actually perform any file operations, default: {defaultValue}";
    DefaultValueFactory = (result) =>
    {
      return defaultValue;
    };
  }
}
