namespace Sphere.FileTransfer.Cli.Constants;

internal static class OptionNames
{
  // Common options
  public const string Help = "--help";

  // Root Command options
  public const string Version = "--version";
  public const string Info = "--info";

  public const string Sources = "--sources";
  public const string SourcesAlias = "-s";
  public const string Destination = "--destination";
  public const string DestinationAlias = "-d";
  public const string Operation = "--operation";
  public const string Overwrite = "--overwrite";
  public const string OutputFormat = "--output-format";
  public const string DryRun = "--dry-run";
  public const string Quiet = "--quiet";

  // Delimited Command options
  public const string File = "--file";
  public const string FileAlias = "-f";
  public const string Column = "--column";
  public const string ColumnAlias = "-c";
  public const string NoHeader = "--no-header";
  public const string Delimiter = "--delimiter";

  // FileNamePattern Command options
  public const string SearchPattern = "--search-pattern";
  public const string SearchPatternAlias = "-sp";
}