using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

public sealed class FileOption : Option<FileInfo>
{
  public FileOption() : base(OptionNames.File, OptionNames.FileAlias)
  {
    Description = $"Delimited file (Refer {OptionNames.Delimiter} for supported delimiters).";
    Arity = ArgumentArity.ExactlyOne;
    Required = true;
    AcceptLegalFilePathsOnly();
    AddValidators();
  }

  private void AddValidators()
  {
    Validators.Add(result =>
    {
      var file = result.GetValue<FileInfo>(Name);
      if (file is null)
      {
        return;
      }
      if (!file.Exists)
      {
        result.AddError($"Option '{Name}' provided file doesn't exist or lacks permission access");
      }
      else if (Utility.GetEncoding(file.FullName) is null)
      {
        result.AddError($"Option '{Name}' provided file is not a valid delimited file");
      }
    });
  }
}
