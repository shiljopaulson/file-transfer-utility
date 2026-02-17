using System.CommandLine;
using System.Text;
using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Handlers;

public interface ICommandHandler
{
  Task<int> Handle(ParseResult parseResult, CancellationToken cancellationToken);
}

public class RootHandler : ICommandHandler
{
  public async Task<int> Handle(ParseResult parseResult, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (parseResult.GetValue<bool>(OptionNames.Info))
    {
      var (version, informationalVersion) = Utility.GetVersion();
      var stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("_description");
      stringBuilder.Append("\n\n");
      stringBuilder.AppendLine($"Version: {version}");
      stringBuilder.AppendLine($"Informational Version: {informationalVersion}");
      stringBuilder.AppendLine("License: MIT License(https://mit-license.org/)");
      stringBuilder.AppendLine("Learn more: https://github.com/shiljopaulson");
      stringBuilder.AppendLine("Contributors: Shiljo Paulson");
      Console.Write(stringBuilder);
    }
    return ExitCodes.Success;
  }
}
