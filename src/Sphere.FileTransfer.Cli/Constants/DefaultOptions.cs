using Sphere.FileTransfer.Models;

namespace Sphere.FileTransfer.Cli.Constants;

internal static class DefaultOptions
{
  public const Operation FileMode = Operation.Copy;
  public const Cli.Models.OutputFormat OutputFormat = Cli.Models.OutputFormat.Text;
  public const bool Overwrite = false;
  public const bool DryRun = false;
  public const bool Quiet = false;
  public const bool NoHeader = false;
  public const byte Column = 1;
  public const Cli.Models.Delimiter Delimiter = Cli.Models.Delimiter.Comma;
  public const string FileNamePattern = "*.*";
}
