using System;
using System.Diagnostics;

namespace FrostAura.Services.Plutus.Shared.Decorators
{
  /// <summary>
  /// Decorator for allowing timing code.
  /// </summary>
  public class TimingDecorator : IDisposable
  {
    /// <summary>
    /// Timer component. After the dispose has been called, this can be used to look at how long an operation took.
    /// </summary>
    public readonly Stopwatch Stopwatch = Stopwatch.StartNew();

    /// <summary>
    /// Hook into the end of using wraps.
    /// </summary>
    public void Dispose()
    {
      Stopwatch.Stop();
    }
  }
}
