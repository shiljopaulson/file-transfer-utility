using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class DestinationOption : Option<DirectoryInfo>
{
  public DestinationOption() : base("--destination", ["-d"])
  {
    Description = "Destination directory to (copy or move) files from Source directory";
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
      else if (directoryInfo.Exists)
      {
        var sourceOptionName = "--source";
        var sourceDirectory = result.GetValue<DirectoryInfo>(sourceOptionName);
        if (sourceDirectory is null)
        {
          return;
        }
        else if (sourceDirectory.FullName.TrimEnd(Path.DirectorySeparatorChar) == directoryInfo.FullName.TrimEnd(Path.DirectorySeparatorChar))
        {
          result.AddError($"Option '{sourceOptionName}' & Option '{Name}' cannot be same directory location");
        }
      }
    });
  }
}
