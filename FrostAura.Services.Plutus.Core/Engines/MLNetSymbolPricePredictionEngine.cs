using FrostAura.Libraries.Core.Extensions.Validation;
using FrostAura.Services.Plutus.Core.Interfaces;
using FrostAura.Services.Plutus.Data.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrostAura.Services.Plutus.Shared.Consts;

namespace FrostAura.Services.Plutus.Core.Engines
{
  /// <summary>
  /// A provider for predicting timeseries prices for symbols.
  /// </summary>
  /// <remarks>
  /// ML.Net Timeseries Documentation: https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_TaxiFarePrediction
  /// </remarks>
  public class MLNetSymbolPricePredictionEngine : ISymbolPricePredictionEngine
  {
    /// <summary>
    /// Cache resource for candlestick symbol timeseries information.
    /// </summary>
    private readonly ICandlestickCacheResource _candlestickCacheResource;
    /// <summary>
    /// Whether the instance have been initialized.
    /// </summary>
    private bool _isInitialized = false;

    /// <summary>
    /// Overloaded constructor for injecting parameters.
    /// </summary>
    /// <param name="candlestickCacheResource">Cache resource for candlestick symbol timeseries information.</param>
    public MLNetSymbolPricePredictionEngine(ICandlestickCacheResource candlestickCacheResource)
    {
      _candlestickCacheResource = candlestickCacheResource.ThrowIfNull(nameof(candlestickCacheResource));
    }

    /// <summary>
    /// Predict the price of a symbol at a specific date.
    /// </summary>
    /// <param name="symbol">Symbol which to predict the price for. E.g. ETHBTC</param>
    /// <param name="date">The date/time which to predict the price for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The price prediction for the specified symbol, for the given time.</returns>
    public async Task<double> PredictSymbolPriceForSpecificDayAsync(string symbol, DateTime date, CancellationToken token)
    {
      if (date == default) throw new ArgumentNullException(nameof(date));
      if (date < DateTime.UtcNow) throw new ArgumentException("The date has to be in the future.", nameof(date));
      symbol.ThrowIfNullOrWhitespace(nameof(symbol));

      await EnsureInitializedAsync(token);

      var fromDate = DateTime.UtcNow.AddYears(-1);
      var toDate = DateTime.UtcNow;
      var interval = Interval.OneDay;
      var symbolHistoricalData = await _candlestickCacheResource.GetCandlesticksAsync(new List<string> { symbol }, interval, fromDate, toDate, token);

      throw new NotImplementedException("TODO: ML.Net timeseries prediction engine.");

      return default;
    }

    /// <summary>
    /// Initialize all required components.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    private async Task EnsureInitializedAsync(CancellationToken token)
    {
      if (_isInitialized) return;

      await _candlestickCacheResource.InitializeAsync(token);

      _isInitialized = true;
    }
  }
}