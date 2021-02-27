using FrostAura.Services.Plutus.Shared.Consts;
using FrostAura.Services.Plutus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Interfaces
{
  /// <summary>
  /// A provider for asset's candlestick information from a respected source.
  /// </summary>
  public interface ICandlestickResource
  {
    /// <summary>
    /// Initialize the asset resource async in order to allow for bootstrapping, subscriptions etc operations to occur.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    Task InitializeAsync(CancellationToken token);

    /// <summary>
    /// Get candlestick data for a given timeframe, given a collection of symbols.
    /// </summary>
    /// <param name="symbols">Collection of pairs to fetch the candlestick data for.</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="from">The starting date of the range which to fetch data for.</param>
    /// <param name="to">The end date of the range which to fetch data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A dictionary with the pair as the key and the candlestick data as the value.</returns>
    Task<IDictionary<string, IEnumerable<Candlestick>>> GetCandlesticksAsync(IEnumerable<string> symbols, Interval interval, DateTime from, DateTime to, CancellationToken token);
  }
}