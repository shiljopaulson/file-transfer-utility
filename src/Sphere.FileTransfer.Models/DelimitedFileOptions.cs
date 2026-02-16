namespace Sphere.FileTransfer.Models;

public sealed class DelimitedFileOptions(DirectoryInfo[]? Sources, DirectoryInfo? Destination, Operation Operation, bool Overwrite, bool DryRun, FileInfo? DelimitedFile, byte Column, bool NoHeader, char Delimiter) : BaseFileOptions(Sources, Destination, Operation, Overwrite, DryRun)
{
    public FileInfo? DelimitedFile { get; init; } = DelimitedFile;
    public byte Column { get; init; } = Column;
    public bool NoHeader { get; init; } = NoHeader;
    public char Delimiter { get; init; } = Delimiter;
}
