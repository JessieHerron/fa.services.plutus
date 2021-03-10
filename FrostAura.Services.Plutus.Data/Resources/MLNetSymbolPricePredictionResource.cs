using FrostAura.Libraries.Core.Extensions.Validation;
using FrostAura.Services.Plutus.Data.Interfaces;

namespace FrostAura.Services.Plutus.Data.Resources
{
  /// <summary>
  /// A provider for predicting timeseries prices for symbols.
  /// </summary>
  /// <remarks>
  /// ML.Net Timeseries Documentation: https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_TaxiFarePrediction
  /// </remarks>
  public class MLNetSymbolPricePredictionResource : ISymbolPricePredictionResource
  {
    /// <summary>
    /// Cache resource for candlestick symbol timeseries information.
    /// </summary>
    private readonly ICandlestickCacheResource _candlestickCacheResource;

    /// <summary>
    /// Overloaded constructor for injecting parameters.
    /// </summary>
    /// <param name="candlestickCacheResource">Cache resource for candlestick symbol timeseries information.</param>
    public MLNetSymbolPricePredictionResource(ICandlestickCacheResource candlestickCacheResource)
    {
      _candlestickCacheResource = candlestickCacheResource.ThrowIfNull(nameof(candlestickCacheResource));
    }
  }
}