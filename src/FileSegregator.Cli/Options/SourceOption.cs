using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class SourceOption : Option<DirectoryInfo>
{
  public SourceOption() : base("--source", ["-s"])
  {
    Description = "Source directory to search for files to be copied or moved";
    Arity = ArgumentArity.ExactlyOne;
    Required = true;
    AcceptLegalFilePathsOnly();
    AddValidators();
  }

  private void AddValidators()
  {
    Validators.Add(result =>
    {
      var directoryInfo = result.GetValue<DirectoryInfo>(Name);
      if (directoryInfo is null)
      {
        result.AddError($"Option '{Name}' is required");
      }
      else if (!directoryInfo.Exists)
      {
        result.AddError($"Option '{Name}' provided directory doesn't exist or lacks permission to access");
      }
      else if (directoryInfo.Exists && !directoryInfo.EnumerateFiles().Any())
      {
        result.AddError($"Option '{Name}' provided directory doesn't have any files or lacks permission to access");
      }
    });
  }
}
