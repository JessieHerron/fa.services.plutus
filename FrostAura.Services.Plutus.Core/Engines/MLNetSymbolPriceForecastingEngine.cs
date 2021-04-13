using FrostAura.Libraries.Core.Extensions.Validation;
using FrostAura.Services.Plutus.Core.Interfaces;
using FrostAura.Services.Plutus.Data.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrostAura.Services.Plutus.Shared.Consts;
using Microsoft.ML;
using FrostAura.Services.Plutus.Core.Models;
using FrostAura.Services.Plutus.Shared.Models;
using System.Linq;
using FrostAura.Services.Plutus.Core.Extensions;
using LINQtoCSV;
using System.IO;

namespace FrostAura.Services.Plutus.Core.Engines
{
  /// <summary>
  /// A provider for forecasting prices for symbols.
  /// </summary>
  public class MLNetSymbolPriceForecastingEngine : ISymbolPriceForecastingEngine
  {
    /// <summary>
    /// Cache resource for candlestick symbol timeseries information.
    /// </summary>
    private readonly ICandlestickCacheResource _candlestickCacheResource;
    /// <summary>
    /// A provider for accessing the file system.
    /// </summary>
    private readonly IFileResource _fileResource;
    /// <summary>
    /// Configuration provider.
    /// </summary>
    private readonly IConfigurationResource _configurationResource;
    /// <summary>
    /// Whether the instance have been initialized.
    /// </summary>
    private bool _isInitialized = false;

    /// <summary>
    /// Overloaded constructor for injecting parameters.
    /// </summary>
    /// <param name="candlestickCacheResource">Cache resource for candlestick symbol timeseries information.</param>
    /// <param name="fileResource">A provider for accessing the file system.</param>
    /// <param name="configurationResource">Configuration provider.</param>
    public MLNetSymbolPriceForecastingEngine(ICandlestickCacheResource candlestickCacheResource, IFileResource fileResource, IConfigurationResource configurationResource)
    {
      _candlestickCacheResource = candlestickCacheResource.ThrowIfNull(nameof(candlestickCacheResource));
      _fileResource = fileResource.ThrowIfNull(nameof(fileResource));
      _configurationResource = configurationResource.ThrowIfNull(nameof(configurationResource));
    }

    /// <summary>
    /// Predict the price of a symbol at a specific date.
    /// </summary>
    /// <remarks>
    /// ML.Net Timeseries Documentation: https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_TaxiFarePrediction
    /// </remarks>
    /// <param name="symbol">Symbol which to predict the price for. E.g. ETHBTC</param>
    /// <param name="date">The date/time which to predict the price for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The price prediction for the specified symbol, for the given time.</returns>
    public async Task<double> ForecastSymbolPriceForSpecificDayAsync(string symbol, DateTime date, CancellationToken token)
    {
      if (date == default) throw new ArgumentNullException(nameof(date));
      if (date < DateTime.UtcNow) throw new ArgumentException("The date has to be in the future.", nameof(date));
      symbol.ThrowIfNullOrWhitespace(nameof(symbol));

      await EnsureInitializedAsync(token);

      var fromDate = DateTime.UtcNow.AddYears(-1);
      var toDate = DateTime.UtcNow;
      var interval = Interval.OneDay;
      var symbolHistoricalData = await _candlestickCacheResource.GetCandlesticksAsync(new List<string> { symbol }, interval, fromDate, toDate, token);

      throw new NotImplementedException("TODO: ML.Net timeseries forecasting engine.");
    }

    /// <summary>
    /// Forecast the general direction of a symbol's price for the next candlestick for a given interval.
    /// </summary>
    /// <remarks>
    /// ML.Net Timeseries Documentation: https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_TaxiFarePrediction
    /// </remarks>
    /// <param name="symbol">Symbol which to predict the price for. E.g. ETHBTC</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The general forecasted direction of a symbol's price for the next candlestick for a given interval.</returns>
    public async Task<float> ForecastNextCandlestickPercentageDeltaAsync(string symbol, Interval interval, CancellationToken token)
    {
      symbol.ThrowIfNullOrWhitespace(nameof(symbol));

      await EnsureInitializedAsync(token);

      var separatorCharacter = ',';
      var relativeCachePath = await _configurationResource.GetRelativeDirectoryPathForSymbolCachingAsync(token);
      var filePath = Path.Combine(relativeCachePath, $"{interval}/{symbol}.ml.csv");
      var mlReadyCandlestickInformation = await GenerateCandlestickForecastingFileAsync(symbol, interval, filePath, token, separatorCharacter: separatorCharacter);

      mlReadyCandlestickInformation.ThrowIfNull(nameof(mlReadyCandlestickInformation));

      // Initialize the ML context and load the data.
      var seed = 0;
      var mlContext = new MLContext(seed: seed);
      var data = mlContext
        .Data
        .LoadFromTextFile<CandlestickForecastingInputModel>(filePath, separatorChar: separatorCharacter);
      var splitData = mlContext
        .Data
        .TrainTestSplit(data, testFraction: 0.3, seed: seed);

      // Create the data pipeline.
      var features = mlContext
        .Transforms
        .Concatenate(
          "Features",
          nameof(CandlestickForecastingInputModel.Rsi),
          nameof(CandlestickForecastingInputModel.BBUpper),
          //nameof(CandlestickForecastingInputModel.BBMid),
          nameof(CandlestickForecastingInputModel.BBLower),
          nameof(CandlestickForecastingInputModel.StochSlowD),
          nameof(CandlestickForecastingInputModel.StochSlowK),
          nameof(CandlestickForecastingInputModel.Open), 
          //nameof(CandlestickForecastingInputModel.High),
          //nameof(CandlestickForecastingInputModel.Low),
          nameof(CandlestickForecastingInputModel.Close),
          nameof(CandlestickForecastingInputModel.Volume));
      var dataPipeline = mlContext
        .Transforms
        .CopyColumns(outputColumnName: "Label", inputColumnName: nameof(CandlestickForecastingInputModel.PriceDeltaFromPreviousCandle))
        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.Rsi)))
        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.BBUpper)))
        //.Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.BBMid)))
        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.BBLower)))
        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.StochSlowD)))
        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.StochSlowK)))
        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.Open)))
        //.Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.High)))
        //.Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.Low)))
        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.Close)))
        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CandlestickForecastingInputModel.Volume)))
        .Append(features);

      // Create the training pipeline.
      var trainer = mlContext
        .Regression
        .Trainers
        .Sdca();
      var trainingPipeline = dataPipeline
        .Append(trainer);

      // Train / fit the model.
      var trainedModel = trainingPipeline
        .Fit(splitData.TrainSet);

      // Evaluate predictions.
      var predictions = trainedModel
        .Transform(splitData.TestSet);
      var evaluation = mlContext
        .Regression
        .Evaluate(predictions);
      var modelAccuracyPercentage = evaluation
        .RSquared * 100;

      // Create prediction engine.
      var predictionEngine = mlContext
        .Model
        .CreatePredictionEngine<CandlestickForecastingInputModel, CandlestickForecastingOutputModel>(trainedModel);

      // Make the actual prediction.
      var prediction = predictionEngine
        .Predict(mlReadyCandlestickInformation.Last());

      return prediction.PriceDeltaFromPreviousCandle;
    }

    /// <summary>
    /// Fetch the data for a given symbol for a given interval over a the last year and generate indicator values for the candlestick information. Then persist the data to a file.
    /// </summary>
    /// <param name="symbol">Symbol which to predict the price for. E.g. ETHBTC</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="fileName">Full file path or the file to generate.</param>
    /// <param name="token">Cancellation token.</param>
    /// <param name="separatorCharacter">Column separator character.</param>
    /// <returns>The path of the file generated and persisted.</returns>
    public async Task<IEnumerable<CandlestickForecastingInputModel>> GenerateCandlestickForecastingFileAsync(string symbol, Interval interval, string filePath, CancellationToken token, char separatorCharacter = ',')
    {
      symbol.ThrowIfNullOrWhitespace(nameof(symbol));
      filePath.ThrowIfNullOrWhitespace(nameof(filePath));

      await EnsureInitializedAsync(token);

      var fromDate = DateTime.UtcNow.AddYears(-5);
      var toDate = DateTime.UtcNow;
      var symbolHistoricalData = await _candlestickCacheResource.GetCandlesticksAsync(new List<string> { symbol }, interval, fromDate, toDate, token);
      var decoratedSymbolHistoricalData = await GetDecoratedCandlesticksCollectionAsync(symbolHistoricalData, token);

      // Convert the model to a CSV.
      var csvContext = new CsvContext();
      var csvFileDescriptor = new CsvFileDescription
      {
        SeparatorChar = separatorCharacter,
        FirstLineHasColumnNames = true
      };

      if (!decoratedSymbolHistoricalData.Any()) return default;

      csvContext.Write(
        decoratedSymbolHistoricalData.First().Value,
        filePath,
        csvFileDescriptor);

      return decoratedSymbolHistoricalData.First().Value;
    }

    /// <summary>
    /// Decorate a collection of candlestick data with additional contextual information.
    /// </summary>
    /// <param name="symbolHistoricalData">Candlestick data for a given timeframe, given a collection of symbols.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A dictionary with the pair as the key and the decorated candlestick data as the value.</returns>
    private async Task<IDictionary<string, IEnumerable<CandlestickForecastingInputModel>>> GetDecoratedCandlesticksCollectionAsync(IDictionary<string, IEnumerable<Candlestick>> symbolHistoricalData, CancellationToken token)
    {
      var decorationTasks = symbolHistoricalData
        .Keys
        .Select(k => GetDecoratedCandlesticksAsync(k, symbolHistoricalData[k].ToList(), token));
      var decorationResults = await Task.WhenAll(decorationTasks);
      var result = new Dictionary<string, IEnumerable<CandlestickForecastingInputModel>>();

      foreach (var decorationResult in decorationResults)
      {
        result[decorationResult.Symbol] = decorationResult.Data;
      }

      return result;
    }

    /// <summary>
    /// Decorate candlestick data with additional contextual information.
    /// </summary>
    /// <param name="symbolHistoricalData">Candlestick data for a given timeframe, given a collection of symbols.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The symbol for which the deecorated candlestick data as the value together with the data for it.</returns>
    private Task<(string Symbol, List<CandlestickForecastingInputModel> Data)> GetDecoratedCandlesticksAsync(string symbol, List<Candlestick> symbolHistoricalData, CancellationToken token)
    {
      var results = new List<CandlestickForecastingInputModel>();

      // Calculate indicator values we would like to subscribe to.
      var inputClose = symbolHistoricalData
          .Select(c => (decimal)c.Close)
          .ToArray();
      var inputLow = symbolHistoricalData
          .Select(c => (decimal)c.Low)
          .ToArray();
      var inputHigh = symbolHistoricalData
          .Select(c => (decimal)c.High)
          .ToArray();
      var rsiValues = new decimal[inputClose.Length];
      var bbUpperValues = new decimal[inputClose.Length];
      var bbMidValues = new decimal[inputClose.Length];
      var bbLowerValues = new decimal[inputClose.Length];
      var stochSlowKValues = new decimal[inputClose.Length];
      var stochSlowDValues = new decimal[inputClose.Length];

      TALib.Core.Rsi(
        inputClose,
        0,
        inputClose.Length - 1,
        rsiValues,
        out int outRsiBegIdx,
        out int outRsiNbElement
      );
      TALib.Core.Bbands(
        inputClose,
        0,
        inputClose.Length - 1,
        bbUpperValues,
        bbMidValues,
        bbLowerValues,
        out int outBBBegIdx,
        out int outBBNbElement,
        optInTimePeriod: 14
        );
      TALib.Core.Stoch(
        inputHigh,
        inputLow,
        inputClose,
        0,
        inputClose.Length - 1,
        stochSlowKValues,
        stochSlowDValues,
        out int stochOutBegIdx,
        out int stochOutNbElement
        );

      for (var i = 0; i < symbolHistoricalData.Count; i++)
      {
        var previousCandlestick = i == 0 ? default : symbolHistoricalData[i - 1];
        var currentCandlestick = symbolHistoricalData[i];
        // Do a simple remap to the derived type.
        var decoratedCandlestick = new CandlestickForecastingInputModel
        {
          Close = currentCandlestick.Close,
          CloseTime = currentCandlestick.CloseTime,
          High = currentCandlestick.High,
          Low = currentCandlestick.Low,
          Open = currentCandlestick.Open,
          OpenTime = currentCandlestick.OpenTime,
          TradeCount = currentCandlestick.TradeCount,
          Volume = currentCandlestick.Volume
        };

        // Calculate the delta between the previous candle and te current.
        if(previousCandlestick != default)
        {
          decoratedCandlestick.PriceDeltaFromPreviousCandle = (float)Math.Round(MathExtensions.GetPercentageDelta((decimal)previousCandlestick.Open, (decimal)currentCandlestick.Close), 2);
        }

        // Assign indicator values.
        decoratedCandlestick.Rsi = (float)rsiValues[i];
        decoratedCandlestick.BBUpper = (float)bbUpperValues[i];
        decoratedCandlestick.BBMid = (float)bbMidValues[i];
        decoratedCandlestick.BBLower = (float)bbLowerValues[i];
        decoratedCandlestick.StochSlowD = (float)stochSlowDValues[i];
        decoratedCandlestick.StochSlowK = (float)stochSlowKValues[i];

        results.Add(decoratedCandlestick);
      }

      return Task.FromResult((Symbol: symbol, Data: results));
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