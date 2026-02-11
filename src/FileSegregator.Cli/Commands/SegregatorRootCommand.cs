using System.CommandLine;
using System.Reflection;
using System.Text;
using FileSegregator.Cli.Constants;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Commands;

public sealed class SegregatorRootCommand
{
  public static RootCommand Create()
  {
    var description = $"Segregate files using a delimited files ({string.Join(",", Enum.GetNames<Delimiter>())}) and directory search patterns ({DefaultOptions.FileNamePattern})";
    RootCommand rootCommand = new(description);
    rootCommand.Options.Add(new Options.InfoOption());

    DelimitedFileCommand delimitedFileCommand = [];
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
