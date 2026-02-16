using System.CommandLine;
using System.Reflection;
using System.Text;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Services;
using Sphere.FileTransfer.Services.Readers;

namespace Sphere.FileTransfer.Cli.Commands;

public sealed class SegregatorRootCommand
{
  private static readonly string _description;
  private static readonly IDelimitedFileService _delimitedFileService;

  static SegregatorRootCommand()
  {
    var delimiters = Enum.GetNames<Delimiter>().Select(x => x.ToLowerInvariant());
    _description = $"A data driven file transfer utility which copies or moves files from one or multiple source directories to the destination directory based on the `delimited file` ({string.Join(",", delimiters)}) or directory `search pattern` (Example: *.png, *.*).";
    var delimitedFileReader = new DelimitedFileReader();
    _delimitedFileService = new DelimitedFileService(delimitedFileReader);
  }

  public static RootCommand Create()
  {
    RootCommand rootCommand = new(_description);
    rootCommand.Options.Add(new Options.InfoOption());

    var delimitedFileCommand = new DelimitedFileCommand(_delimitedFileService);
    PatternCommand patternCommand = [];
    rootCommand.Subcommands.Add(delimitedFileCommand.Create());
    rootCommand.Subcommands.Add(patternCommand.Create());

    rootCommand.SetAction(static async (parseResult, cancellationToken) =>
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (parseResult.GetValue<bool>(OptionNames.Info))
      {
        var (version, informationalVersion) = GetVersion();
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(_description);
        stringBuilder.Append("\n\n");
        stringBuilder.AppendLine($"Version: {version}");
        stringBuilder.AppendLine($"Informational Version: {informationalVersion}");
        stringBuilder.AppendLine("License: MIT License(https://mit-license.org/)");
        stringBuilder.AppendLine("Learn more: https://github.com/shiljopaulson");
        stringBuilder.AppendLine("Contributors: Shiljo Paulson");
        Console.Write(stringBuilder);
      }
      return ExitCodes.Success;
    });
    return rootCommand;
  }
  private static (string?, string?) GetVersion()
  {
    var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    if (string.IsNullOrWhiteSpace(version))
    {
      return (string.Empty, string.Empty);
    }
    return (version.Split('+')[0], version);
  }
}
