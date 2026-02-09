namespace FileSegregator.Cli.Models;

public sealed class FileNamePatternOption(DirectoryInfo? Source, DirectoryInfo? Destination, Mode Mode, OutputFormat OutputFormat, bool Overwrite, bool DryRun, bool Quiet, string? FileNamePattern) : BaseFileOptions(Source, Destination, Mode, OutputFormat, Overwrite, DryRun, Quiet)
{
    public string? FileNamePattern { get; init; } = FileNamePattern;

    public override string ToString()
    {
        return $"{base.ToString()}, FileNamePattern:{FileNamePattern}";
    }
}