namespace FileSegregator.Cli.Models;

public sealed class DelimitedFileOptions(DirectoryInfo[]? Sources, DirectoryInfo? Destination, Mode Mode, OutputFormat OutputFormat, bool Overwrite, bool DryRun, bool Quiet, FileInfo? InputFile, byte Column, bool NoHeader, Delimiter Delimiter) : BaseFileOptions(Sources, Destination, Mode, OutputFormat, Overwrite, DryRun, Quiet)
{
    public FileInfo? InputFile { get; init; } = InputFile;
    public byte Column { get; init; } = Column;
    public bool NoHeader { get; init; } = NoHeader;
    public Delimiter Delimiter { get; init; } = Delimiter;

    public override string ToString()
    {
        return $"{base.ToString()}, InputFile:{InputFile}, Column:{Column}, NoHeader:{NoHeader}, Delimiter:{Delimiter}";
    }
}
