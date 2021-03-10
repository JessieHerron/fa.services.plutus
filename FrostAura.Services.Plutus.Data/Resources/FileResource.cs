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

    /// <summary>
    /// Asynchronously opens a text file, reads all the text in the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in the file.</returns>
    public Task<string> ReadAllTextAsync(string path, CancellationToken token)
    {
      return File.ReadAllTextAsync(path, token);
    }

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The file to check.</param>
    /// <returns> True if the caller has the required permissions and path contains the name of an existing file; otherwise, false. This method also returns false if path is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns false regardless of the existence of path.</returns>
    public bool Exists(string path)
    {
      return File.Exists(path);
    }
  }
}
