using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Writer;

/// <summary>
/// Renders a result of type <typeparamref name="T"/> to stdout
/// in the requested <see cref="OutputFormat"/>.
/// </summary>
internal interface IResultWriter<in T>
{
  void Write(T? result, OutputFormat format, CancellationToken cancellationToken);
}
