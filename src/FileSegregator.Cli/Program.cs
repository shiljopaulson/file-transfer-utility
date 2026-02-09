using System.CommandLine;
using FileSegregator.Cli.Commands;

namespace FileSegregator.Cli;

class Program
{
    async static Task<int> Main(string[] args)
    {
        var exitCode = 0;
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };
        try
        {
            RootCommand rootCommand = SegregatorRootCommand.Create();
            exitCode = await rootCommand.Parse(args).InvokeAsync(cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation canceled.");
            exitCode = 130;
        }
        return exitCode;
    }
}
