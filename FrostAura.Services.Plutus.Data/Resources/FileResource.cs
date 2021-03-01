using FrostAura.Services.Plutus.Data.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Resources
{
  /// <summary>
  /// A provider for accessing the file system.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class FileResource : IFileResource
  {
    /// <summary>
    /// Asynchronously creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string to write to the file.</param>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public Task WriteAllTextAsync(string path, string contents, CancellationToken token)
    {
      return File.WriteAllTextAsync(path, contents, token);
    }
  }
}
