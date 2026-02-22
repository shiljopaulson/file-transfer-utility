using System.CommandLine;
using System.Text;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Handlers;

public interface ICommandHandler
{
  Task<int> Handle(ParseResult parseResult, CancellationToken cancellationToken);
}

public class RootHandler : ICommandHandler
{
  private readonly ILogger<RootHandler> _logger;
  public RootHandler(ILogger<RootHandler> logger)
  {
    _logger = logger;
  }

  public async Task<int> Handle(ParseResult parseResult, CancellationToken cancellationToken)
  {
    _logger.LogTrace("Entering RootHandler => Handle");
    cancellationToken.ThrowIfCancellationRequested();
    if (parseResult.GetValue<bool>(OptionNames.Info))
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append("\n\n");
      var assemblyDetails = Utility.GetAssemblyDetails();
      if (assemblyDetails is not null)
      {
        stringBuilder.AppendLine($"Title: {assemblyDetails.Title}");
        stringBuilder.AppendLine($"Description: {assemblyDetails.Description}");
        stringBuilder.AppendLine($"Product: {assemblyDetails.Product}");
        stringBuilder.AppendLine($"Company: {assemblyDetails.Company}");
        stringBuilder.AppendLine($"Trademark: {assemblyDetails.Trademark}");
        stringBuilder.AppendLine($"Copyright: {assemblyDetails.Copyright}");
        stringBuilder.AppendLine($"License: {assemblyDetails.License}");
        stringBuilder.AppendLine($"Version: {assemblyDetails.Version}");
        stringBuilder.AppendLine($"Informational Version: {assemblyDetails.InformationalVersion}");
        stringBuilder.AppendLine($"Contributors: {assemblyDetails.Contributors}");
        stringBuilder.AppendLine($"More Info: {assemblyDetails.MoreInfo}");
      }
      stringBuilder.Append("\n");
      Console.Write(stringBuilder);
    }
    return ExitCodes.Success;
  }
}
