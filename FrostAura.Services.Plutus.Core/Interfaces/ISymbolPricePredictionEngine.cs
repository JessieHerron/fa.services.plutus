using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Core.Interfaces
{
  /// <summary>
  /// A provider for predicting timeseries prices for symbols.
  /// </summary>
  public interface ISymbolPricePredictionEngine
  {
    /// <summary>
    /// Predict the price of a symbol at a specific date.
    /// </summary>
    /// <param name="symbol">Symbol which to predict the price for. E.g. ETHBTC</param>
    /// <param name="date">The date/time which to predict the price for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The price prediction for the specified symbol, for the given time.</returns>
    Task<double> PredictSymbolPriceForSpecificDayAsync(string symbol, DateTime date, CancellationToken token);
  }
}