﻿using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.MarketData;
using CryptoExchange.Net.Objects;
using FrostAura.Libraries.Core.Extensions.Validation;
using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Shared.Consts;
using FrostAura.Services.Plutus.Shared.Decorators;
using FrostAura.Services.Plutus.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Resources
{
  /// <summary>
  /// Binance-specific provider for asset's candlestick information.
  /// </summary>
  public class BinanceAssetResource : IAssetResource, IAsyncDisposable
  {
    /// <summary>
    /// Binance rest client.
    /// </summary>
    private readonly IBinanceClient _client;
    /// <summary>
    /// Binance socket client.
    /// </summary>
    private readonly IBinanceSocketClient _socketClient;
    /// <summary>
    /// Instance logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Overloaded constructor to allow for injecting dependencies.
    /// </summary>
    /// <param name="client">Binance rest client.</param>
    /// <param name="socketClient">Binance socket client.</param>
    /// <param name="logger">Instance logger.</param>
    public BinanceAssetResource(IBinanceClient client, IBinanceSocketClient socketClient, ILogger<BinanceAssetResource> logger)
    {
      _client = client.ThrowIfNull(nameof(client));
      _socketClient = socketClient.ThrowIfNull(nameof(socketClient));
      _logger = logger.ThrowIfNull(nameof(logger));
    }

    /// <summary>
    /// Initialize the asset resource async in order to allow for bootstrapping, subscriptions etc operations to occur.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public Task InitializeAsync(CancellationToken token)
    {
      /*var subResult = await _socketClient.Spot.SubscribeToKlineUpdatesAsync("BTCUSDT", KlineInterval.FifteenMinutes, data =>
      {
        LastKline = data;
        OnKlineData?.Invoke(data);
      });
      if (subResult.Success)
        _subscription = subResult.Data;*/
      return Task.CompletedTask;
    }

    /// <summary>
    /// Get candlestick data for a given timeframe, given a collection of pairs.
    /// </summary>
    /// <param name="symbols">Collection of pairs to fetch the candlestick data for.</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="from">The starting date of the range which to fetch data for.</param>
    /// <param name="to">The end date of the range which to fetch data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A dictionary with the pair as the key and the candlestick data as the value.</returns>
    public async Task<IDictionary<string, IEnumerable<Candlestick>>> GetCandlestickDataForPairsAsync(IEnumerable<string> symbols, Interval interval, DateTime from, DateTime to, CancellationToken token)
    {
      if (!symbols.ThrowIfNull(nameof(symbols)).Any())
        throw new ArgumentException("At least one symbol should be provided.", nameof(symbols));

      _logger.LogInformation($"Fetching candlestick data from Binance for {symbols.Count()} symbols at '{Enum.GetName(typeof(Interval), (int)interval)}' interval from {from.ToShortDateString()} to {to.ToShortDateString()}.");

      var timer = new TimingDecorator();
      IDictionary<string, IEnumerable<Candlestick>> response;

      using (timer)
      {
        var requestsTasks = symbols
          .ToDictionary(
            s => s, 
            s => this.GetKlinesForSymbolRecursivelyAsync(s.Replace("/", string.Empty), (KlineInterval)((int)interval), from, to, token));

        // Parallel execution does not work well due to the threshold that Binance enforces on requests per minute.
        //await Task.WhenAll(requestsTasks.Select(r => r.Value));

        // Force sequential processing.
        foreach (var task in requestsTasks.Select(t => t.Value))
        {
          await task;
        }

        var responses = requestsTasks
          .ToDictionary(r => r.Key, r => r.Value.Result);
        var errors = responses
          .Where(r => !r.Value.Success);

        foreach (var error in errors)
        {
          _logger.LogWarning($"Failed to fetch candlestick data for symbol '{error.Key}' with code {error.Value.Error.Code} and message '{error.Value.Error.Message}'.");
        }

        response = responses
          .Where(r => r.Value.Success)
          .ToDictionary(r => r.Key, r =>
          {
            var candlesticks = r
              .Value
              .Data
              .Select(i => new Candlestick
              {
                // TODO: Migrate this mapping to an automapper implementation.
                OpenTime = i.OpenTime,
                Open = i.Open,
                High = i.High,
                Low = i.Low,
                Close = i.Close,
                Volume = i.BaseVolume,
                CloseTime = i.CloseTime,
                TradeCount = i.TradeCount
              });

            return candlesticks;
          });
      }

      _logger.LogInformation($"Candlestick data fetch completed in {(int)timer.Stopwatch.Elapsed.TotalSeconds} seconds.");

      return response;
    }

    /// <summary>
    /// Get candlestick information for a given symbol recursively until we have accumulated enought data to satisfy our range. This technique is used to bypass the limit of return results Binance enforces. 
    /// </summary>
    /// <param name="symbol">Symbol to fetch the data for.</param>
    /// <param name="interval">Interval which to fetch data for.</param>
    /// <param name="from">Range from which to fetch data for.</param>
    /// <param name="to">Range to which to fetch data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    private async Task<WebCallResult<IEnumerable<IBinanceKline>>> GetKlinesForSymbolRecursivelyAsync(string symbol, KlineInterval interval, DateTime from, DateTime to, CancellationToken token)
    {
      var result = await _client
        .Spot
        .Market
        .GetKlinesAsync(symbol, interval, startTime: from, endTime: to, ct: token);

      // If error or no results returned, propagate the error all the way up.
      if (!result.Success)
      {
        if(result.ResponseStatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
          // Enforce artificial delay to allow for bypassing Binance throughput threshold.
          await Task.Delay(TimeSpan.FromSeconds(30));

          return await this.GetKlinesForSymbolRecursivelyAsync(symbol, interval, from, to, token);
        }

        return result;
      };
      if (!result.Data.Any()) return result;

      // Check if the last item matches the to date we actually want.
      var itemsList = result.Data.ToList();
      var lastItem = itemsList.Last();
      var lastItemMatchesSpecifiedEndDate = lastItem.OpenTime.Year == to.Year && lastItem.OpenTime.Month == to.Month && lastItem.OpenTime.Day == to.Day;

      if (lastItemMatchesSpecifiedEndDate) return result;

      // If not, recursively fetch the rest of the data starting from the last open date we have till the to date.
      var gapFillingResults = await this.GetKlinesForSymbolRecursivelyAsync(symbol, interval, lastItem.OpenTime, to, token);

      // If error, propagate the error all the way up.
      if (!gapFillingResults.Success) return gapFillingResults;

      // Merge previous and nested results together.
      var resultList = (List<BinanceSpotKline>)result.Data;

      resultList.AddRange((List<BinanceSpotKline>)gapFillingResults.Data);

      return result;
    }

    /// <summary>
    /// Dispose of subscriptions and sockets async.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
      //await _socketClient.Unsubscribe(_subscription);
    }
  }
}
