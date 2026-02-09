using System.CommandLine;
using FileSegregator.Cli.Constants;

namespace FileSegregator.Cli.Options;

public sealed class SourcesOption : Option<DirectoryInfo[]>
{
  public SourcesOption() : base(OptionNames.Sources, OptionNames.SourcesAlias)
  {
    Description = "Sources are the directories to search for files to be copied or moved.";
    Arity = ArgumentArity.OneOrMore;
    Required = true;
    AcceptLegalFilePathsOnly();
    AddValidators();
  }

  private void AddValidators()
  {
    Validators.Add(result =>
    {
      var directoryInfos = result.GetValue<DirectoryInfo[]>(Name);
      if (directoryInfos is null || directoryInfos.Length == 0)
      {
        result.AddError($"Option '{Name}' is required");
        return;
      }
      else if (directoryInfos.Select(x => x.FullName.TrimEnd(Path.DirectorySeparatorChar)).Distinct().Count() != directoryInfos.Length)
      {
        result.AddError($"Option '{Name}' contains duplicates. Multiple '{Name}' entires pointing to the same directory");
        return;
      }
      for (var i = 0; i < directoryInfos.Length; i++)
      {
        if (directoryInfos[i] is null)
        {
          result.AddError($"Option '{Name}' of [{i}] is required");
        }
        else if (!directoryInfos[i].Exists)
        {
          result.AddError($"Option '{Name}' of [{i}] provided directory doesn't exist or lacks permission to access");
        }
        else if (directoryInfos[i].Exists && !directoryInfos[i].EnumerateFiles().Any())
        {
          result.AddError($"Option '{Name}' of [{i}] provided directory doesn't have any files or lacks permission to access");
        }
      }
    });
  }
}
