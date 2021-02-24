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

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public partial class BinanceAssetResourceTests
  {
    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WhenRanToCompletion_ShouldLogTiming()
    {
      var logger = Substitute.For<ILogger<BinanceAssetResource>>();
      var binanceClient = new BinanceClient();
      var instance = GetInstance(logger: logger, client: binanceClient);

      var timer = new TimingDecorator();
      IDictionary<string, IEnumerable<Candlestick>> results;

      using (timer)
      {
        results = await instance.GetCandlestickDataForPairsAsync(symbols, interval, from, to, token);
      }

      var expectedMessageBeginning = $"Candlestick data fetch completed in {(int)timer.Stopwatch.Elapsed.TotalSeconds} seconds.";

      logger
        .Received()
        .LogInformation(expectedMessageBeginning);

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

      using (timer)
      {
        results = await instance.GetCandlestickDataForPairsAsync(symbols, interval, fromDate, to, token);
      }

      var firstSymbolCandles = results
        .First()
        .Value
        .ToList();
      var firstCandle = firstSymbolCandles
        .First();
      var lastCandle = firstSymbolCandles
        .Last();

      // Test for the start time.
      Assert.Equal(fromDate.Day, firstCandle.OpenTime.Day);
      Assert.Equal(fromDate.Month, firstCandle.OpenTime.Month);
      Assert.Equal(fromDate.Year, firstCandle.OpenTime.Year);

      // Test for the end time.
      Assert.Equal(to.Day, lastCandle.OpenTime.Day);
      Assert.Equal(to.Month, lastCandle.OpenTime.Month);
      Assert.Equal(to.Year, lastCandle.OpenTime.Year);
    }
  }
}
