using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Interfaces
{
  /// <summary>
  /// A provider for accessing the file system.
  /// </summary>
  public interface IFileResource
  {
    /// <summary>
    /// Asynchronously creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string to write to the file.</param>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteAllTextAsync(string path, string contents, CancellationToken token);
  }
}