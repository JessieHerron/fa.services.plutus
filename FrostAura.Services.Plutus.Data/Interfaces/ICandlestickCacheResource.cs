using FrostAura.Services.Plutus.Shared.Consts;
using FrostAura.Services.Plutus.Shared.Models;
using System;
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

    /// <summary>
    /// Persist a given request item to disk.
    /// </summary>
    /// <param name="symbol">Symbol the request is for. E.g. ETHBTC</param>
    /// <param name="interval">The interval / data resolution of the request.</param>
    /// <param name="data">The candlestick information for the provided symbol.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    Task WriteCandlestickForSymbolToFileAsync(string symbol, Interval interval, IEnumerable<Candlestick> data, CancellationToken token);

    /// <summary>
    /// Read candlestick information from the cache.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="from">The starting date of the range which to fetch data for.</param>
    /// <param name="to">The end date of the range which to fetch data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Candlestick data as the value for the given symbol.</returns>
    Task<(string Symbol, IEnumerable<Candlestick> Data)> ReadCandlestickForSymbolFromFileAsync(string symbol, Interval interval, DateTime from, DateTime to, CancellationToken token);
  }
}