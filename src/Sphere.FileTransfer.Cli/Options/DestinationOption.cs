using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

public sealed class DestinationOption : Option<DirectoryInfo>
{
  public DestinationOption() : base(OptionNames.Destination, OptionNames.DestinationAlias)
  {
    Description = $"Destination directory to (copy or move) files from one of the Sources ({OptionNames.Sources}) directory.";
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
    });
  }
}
