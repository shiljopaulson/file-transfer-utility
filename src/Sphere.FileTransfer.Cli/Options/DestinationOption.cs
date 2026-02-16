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
      /* else if (directoryInfo.Exists)
      {
        var sources = result.GetValue<DirectoryInfo[]>(OptionNames.Sources);
        if (sources is null || sources.Length == 0)
        {
          return;
        }
        var destinationFullName = directoryInfo.FullName.TrimEnd(Path.DirectorySeparatorChar);
        for (var i = 0; i < sources.Length; i++)
        {
          var sourceFullName = sources[i].FullName.TrimEnd(Path.DirectorySeparatorChar);
          if (sourceFullName == destinationFullName)
          {
            result.AddError($"Option '{OptionNames.Sources}' of [{i}] & Option '{Name}' cannot be same directory location");
          }
        }
      } */
    });
  }
}
