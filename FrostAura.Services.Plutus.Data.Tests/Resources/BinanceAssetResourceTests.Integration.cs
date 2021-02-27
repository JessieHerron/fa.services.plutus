using Binance.Net;
using FrostAura.Services.Plutus.Data.Resources;
using FrostAura.Services.Plutus.Shared.Decorators;
using FrostAura.Services.Plutus.Shared.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public partial class BinanceAssetResourceTests
  {
    private readonly ITestOutputHelper _testOutputHelper;

    public BinanceAssetResourceTests(ITestOutputHelper testOutputHelper)
    {
      _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WhenRanToCompletion_ShouldLogTiming()
    {
      var logger = Substitute.For<ILogger<BinanceAssetResource>>();
      var binanceClient = new BinanceClient();
      var instance = GetInstance(logger: logger, client: binanceClient);

      var timer = new TimingDecorator();
      IDictionary<string, IEnumerable<Candlestick>> results;

      WireUpLogger(logger);

      using (timer)
      {
        results = await instance.GetCandlestickDataForPairsAsync(symbols.Take(1), interval, from, to, token);
      }

      var expectedMessageBeginning = $"Candlestick data fetch completed in {(int)timer.Stopwatch.Elapsed.TotalSeconds} seconds.";

      logger
        .ReceivedWithAnyArgs()
        .LogInformation(default);

      Assert.NotEmpty(results);
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WithLongPeriod_ShouldReturnResultsForEntirePeriod()
    {
      var logger = Substitute.For<ILogger<BinanceAssetResource>>();
      var binanceClient = new BinanceClient();
      var instance = GetInstance(logger: logger, client: binanceClient);

      var timer = new TimingDecorator();
      IDictionary<string, IEnumerable<Candlestick>> results;
      var fromDate = DateTime.UtcNow.AddYears(-1);

      WireUpLogger(logger);

      using (timer)
      {
        results = await instance.GetCandlestickDataForPairsAsync(symbols, interval, fromDate, to, token);
      }

      foreach (var symbol in symbols)
      {
        Assert.True(results.ContainsKey(symbol));

        var symbolCandles = results[symbol];
        var firstCandle = symbolCandles
          .First();
        var lastCandle = symbolCandles
          .Last();

        // Test for the start time.
        Assert.Equal($"{symbol}-{fromDate.Day}", $"{symbol}-{firstCandle.OpenTime.Day}");
        Assert.Equal($"{symbol}-{fromDate.Month}", $"{symbol}-{firstCandle.OpenTime.Month}");
        Assert.Equal($"{symbol}-{fromDate.Year}", $"{symbol}-{firstCandle.OpenTime.Year}");

        // Test for the end time.
        Assert.Equal($"{symbol}-{to.Day}", $"{symbol}-{lastCandle.OpenTime.Day}");
        Assert.Equal($"{symbol}-{to.Month}", $"{symbol}-{lastCandle.OpenTime.Month}");
        Assert.Equal($"{symbol}-{to.Year}", $"{symbol}-{lastCandle.OpenTime.Year}");
      }
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WithLongPeriodAndManySymbols_ShouldReturnResultsForEntirePeriod()
    {
      var logger = Substitute.For<ILogger<BinanceAssetResource>>();
      var binanceClient = new BinanceClient();
      var instance = GetInstance(logger: logger, client: binanceClient);

      var timer = new TimingDecorator();
      IDictionary<string, IEnumerable<Candlestick>> results;
      var fromDate = DateTime.UtcNow.AddYears(-1);
      var customSymbols = await new IntegrationTestingStaticConfigurationResource().GetPairsAsync(token);

      WireUpLogger(logger);

      using (timer)
      {
        results = await instance.GetCandlestickDataForPairsAsync(customSymbols, interval, fromDate, to, token);
      }

      var resultContainsDataForAllSymbols = symbols
        .All(s => results.ContainsKey(s));

      Assert.True(resultContainsDataForAllSymbols);

      var messagesForSymbolsWithoutSufficientInformation = new List<string>();

      foreach (var symbol in customSymbols)
      {
        var symbolCandles = results[symbol];
        var firstCandle = symbolCandles
          .First();
        var lastCandle = symbolCandles
          .Last();

        // Test for the start time.
        if (fromDate != firstCandle.OpenTime)
          messagesForSymbolsWithoutSufficientInformation.Add($"First candlestick for symbol '{symbol}' ({firstCandle.OpenTime}) did not match specified from time {from}.");

        // Test for the end time.
        if (to != lastCandle.OpenTime)
          messagesForSymbolsWithoutSufficientInformation.Add($"Last candlestick for symbol '{symbol}' ({lastCandle.OpenTime}) did not match specified to time {to}.");
      }

      messagesForSymbolsWithoutSufficientInformation
        .ForEach(i => this._testOutputHelper.WriteLine($"MISSING: {i}"));
    }

    private void WireUpLogger(ILogger logger)
    {
      logger
        .WhenForAnyArgs(l => l.LogDebug(default))
        .Do(callerInfo =>
        {
          var logLevel = callerInfo
            .Arg<LogLevel>();
          var message = callerInfo
            .ArgAt<object>(2)
            .ToString();

          if (logLevel != LogLevel.Debug) return;

          this._testOutputHelper.WriteLine($"DEBUG: {message}");
        });
      logger
        .WhenForAnyArgs(l => l.LogInformation(default))
        .Do(callerInfo =>
        {
          var logLevel = callerInfo
            .Arg<LogLevel>();
          var message = callerInfo
            .ArgAt<object>(2)
            .ToString();

          if (logLevel != LogLevel.Information) return;

          this._testOutputHelper.WriteLine($"INFO: {message}");
        });
      logger
        .WhenForAnyArgs(l => l.LogWarning(default))
        .Do(callerInfo =>
        {
          var logLevel = callerInfo
            .Arg<LogLevel>();
          var message = callerInfo
            .ArgAt<object>(2)
            .ToString();

          if (logLevel != LogLevel.Warning) return;

          this._testOutputHelper.WriteLine($"WARNING: {message}");
        });
      logger
        .WhenForAnyArgs(l => l.LogError(default))
        .Do(callerInfo =>
        {
          var logLevel = callerInfo
            .Arg<LogLevel>();
          var message = callerInfo
            .ArgAt<object>(2)
            .ToString();

          if (logLevel != LogLevel.Error) return;

          this._testOutputHelper.WriteLine($"ERROR: {message}");
        });
    }
  }
}
