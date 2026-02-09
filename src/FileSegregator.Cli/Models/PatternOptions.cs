namespace FileSegregator.Cli.Models;

public sealed class PatternOptions(DirectoryInfo[]? Sources, DirectoryInfo? Destination, Mode Mode, OutputFormat OutputFormat, bool Overwrite, bool DryRun, bool Quiet, string? SearchPattern) : BaseFileOptions(Sources, Destination, Mode, OutputFormat, Overwrite, DryRun, Quiet)
{
    public string? SearchPattern { get; init; } = SearchPattern;

    public override string ToString()
    {
        return $"{base.ToString()}, SearchPattern:{SearchPattern}";
    }
}