using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Shared.Consts;
using FrostAura.Services.Plutus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Resources
{
  /// <summary>
  /// A file system-based caching service for candlestick information.
  /// </summary>
  public class FileSystemCandlestickCacheResource : ICandlestickCacheResource
  {
    /// <summary>
    /// Initialize the asset resource async in order to allow for bootstrapping, subscriptions etc operations to occur.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public Task InitializeAsync(CancellationToken token)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get candlestick data for a given timeframe, given a collection of symbols.
    /// </summary>
    /// <param name="symbols">Collection of pairs to fetch the candlestick data for.</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="from">The starting date of the range which to fetch data for.</param>
    /// <param name="to">The end date of the range which to fetch data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A dictionary with the pair as the key and the candlestick data as the value.</returns>
    public Task<IDictionary<string, IEnumerable<Candlestick>>> GetCandlesticksAsync(IEnumerable<string> symbols, Interval interval, DateTime from, DateTime to, CancellationToken token)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Set candlestick data for multiple symbols atomically.
    /// </summary>
    /// <param name="request">Collection of symbols with their intervals and candlestick data.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public Task SetCandlesticksAsync(IEnumerable<(string Symbol, Interval Interval, IEnumerable<Candlestick> Data)> request, CancellationToken token)
    {
      throw new NotImplementedException();
    }
  }
}
