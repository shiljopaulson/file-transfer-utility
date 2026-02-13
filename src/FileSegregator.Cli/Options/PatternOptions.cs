using FileSegregator.Cli.Models;
namespace FileSegregator.Cli.Options;

public sealed class PatternOptions(DirectoryInfo[]? Sources, DirectoryInfo? Destination, Operation Mode, OutputFormat OutputFormat, bool Overwrite, bool DryRun, bool Quiet, string? SearchPattern) : BaseFileOptions(Sources, Destination, Mode, OutputFormat, Overwrite, DryRun, Quiet)
{
    public string? SearchPattern { get; init; } = SearchPattern;
}