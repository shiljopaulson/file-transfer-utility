namespace FileSegregator.Cli.Constants;

internal sealed class OptionNames
{
  // Common options
  public const string Help = "--help";

  // Root Command options
  public const string Version = "--version";
  public const string Info = "--info";

  public const string Source = "--source";
  public const string SourceAlias = "-s";
  public const string Destination = "--destination";
  public const string DestinationAlias = "-d";
  public const string Mode = "--mode";
  public const string Overwrite = "--overwrite";
  public const string OutputFormat = "--output-format";
  public const string DryRun = "--dry-run";
  public const string Quiet = "--quiet";

  // Delimited Command options
  public const string InputFile = "--input-file";
  public const string InputFileAlias = "-i";
  public const string Column = "--column";
  public const string ColumnAlias = "-c";
  public const string NoHeader = "--no-header";
  public const string Delimiter = "--delimiter";

  // FileNamePattern Command options
  public const string SearchPattern = "--search-pattern";
  public const string SearchPatternAlias = "-p";
}
