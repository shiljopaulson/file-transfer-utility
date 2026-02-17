using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Results;

/// <summary>
/// Renders a result of type <typeparamref name="T"/> to stdout
/// in the requested <see cref="OutputFormat"/>.
/// </summary>
public interface IResultWriter<T>
{
  void Write(T result, OutputFormat format, CancellationToken cancellationToken);
}
