using FrostAura.Services.Plutus.Core.Models;
using FrostAura.Services.Plutus.Shared.Consts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Core.Interfaces
{
  /// <summary>
  /// A provider for forecasting prices for symbols.
  /// </summary>
  public interface ISymbolPriceForecastingEngine
  {
    /// <summary>
    /// Predict the price of a symbol at a specific date.
    /// </summary>
    /// <param name="symbol">Symbol which to predict the price for. E.g. ETHBTC</param>
    /// <param name="date">The date/time which to predict the price for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The price prediction for the specified symbol, for the given time.</returns>
    Task<double> ForecastSymbolPriceForSpecificDayAsync(string symbol, DateTime date, CancellationToken token);

    /// <summary>
    /// Forecast the general direction of a symbol's price for the next candlestick for a given interval.
    /// </summary>
    /// <param name="symbol">Symbol which to predict the price for. E.g. ETHBTC</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The general forecasted direction of a symbol's price for the next candlestick for a given interval.</returns>
    Task<float> ForecastNextCandlestickPercentageDeltaAsync(string symbol, Interval interval, CancellationToken token);

    /// <summary>
    /// Fetch the data for a given symbol for a given interval over a the last year and generate indicator values for the candlestick information. Then persist the data to a file.
    /// </summary>
    /// <param name="symbol">Symbol which to predict the price for. E.g. ETHBTC</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="fileName">Full file path or the file to generate.</param>
    /// <param name="token">Cancellation token.</param>
    /// <param name="separatorCharacter">Column separator character.</param>
    /// <returns>The path of the file generated and persisted.</returns>
    Task<IEnumerable<CandlestickForecastingInputModel>> GenerateCandlestickForecastingFileAsync(string symbol, Interval interval, string filePath, CancellationToken token, char separatorCharacter = ',');
  }
}