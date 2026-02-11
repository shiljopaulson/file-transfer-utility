namespace FileSegregator.Cli.Models;

public enum Status
{
  Unprocessed,
  Processed,
  PartiallyProcessed,
  Copied,
  Moved,
  Skipped,
  Error,
  Failure,
  Duplicate,
  DirectoryNotFound,
  FileNotFound,
  FileFound,
  NoMatchingFilesFound,
  IO,
  ArgumentNull,
  Argument,
  UnauthorizedAccess,
  NotSupported,
  PathTooLong,
  OperationCanceled
}
