using FrostAura.Services.Plutus.Shared.Consts;
using FrostAura.Services.Plutus.Shared.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Interfaces
{
  /// <summary>
  /// A caching service for candlestick information.
  /// </summary>
  public interface ICandlestickCacheResource : ICandlestickResource
  {
    /// <summary>
    /// Set candlestick data for multiple symbols atomically.
    /// </summary>
    /// <param name="request">Collection of symbols with their intervals and candlestick data.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    Task SetCandlesticksAsync(IEnumerable<(string Symbol, Interval Interval, IEnumerable<Candlestick> Data)> request, CancellationToken token);
  }
}