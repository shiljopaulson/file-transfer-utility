using System.CommandLine;

using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

internal sealed class NoHeaderOption : Option<bool>
{
  public NoHeaderOption() : base(OptionNames.NoHeader)
  {
    Description = $"Input file has no header row.";
  }
}
