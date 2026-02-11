using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Constants;

internal sealed class DefaultOptions
{
  public const Models.Operation FileMode = Models.Operation.Copy;
  public const OutputFormat OutputFormat = Models.OutputFormat.Text;
  public const bool Overwrite = false;
  public const bool DryRun = false;
  public const bool Quiet = false;
  public const bool NoHeader = false;
  public const byte Column = 1;
  public const Delimiter Delimiter = Models.Delimiter.Comma;
  public const string FileNamePattern = "*.*";
}
