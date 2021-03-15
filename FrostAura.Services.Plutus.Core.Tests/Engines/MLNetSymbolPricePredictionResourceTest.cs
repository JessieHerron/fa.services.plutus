using FrostAura.Services.Plutus.Core.Engines;
using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Shared.Consts;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FrostAura.Services.Plutus.Core.Tests.Engines
{
  public class MLNetSymbolPricePredictionResourceTest
  {
    private readonly CancellationToken _token = CancellationToken.None;

    [Fact]
    public void Constructor_WithInvalidCandlestickCacheResourceParams_ShouldThrow()
    {
      ICandlestickCacheResource candlestickCacheResource = null;
      var actual = Assert.Throws<ArgumentNullException>(() => new MLNetSymbolPricePredictionEngine(candlestickCacheResource));

      Assert.Equal(nameof(candlestickCacheResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParams_ShouldConstruct()
    {
      var actual = GetInstance();

      Assert.NotNull(actual);
    }

    [Fact]
    public async Task PredictSymbolPriceAsync_WithInvalidSymbol_ShouldThrow()
    {
      var instance = GetInstance();
      string symbol = default;
      var date = DateTime.UtcNow.AddDays(1);

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.PredictSymbolPriceForSpecificDayAsync(symbol, date, _token));

      Assert.Equal(nameof(symbol), actual.ParamName);
    }

    [Fact]
    public async Task PredictSymbolPriceAsync_WithDefaultDate_ShouldThrow()
    {
      var instance = GetInstance();
      var symbol = "ETHBTC";
      DateTime date = default;

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.PredictSymbolPriceForSpecificDayAsync(symbol, date, _token));

      Assert.Equal(nameof(date), actual.ParamName);
    }

    [Fact]
    public async Task PredictSymbolPriceAsync_WithPastDate_ShouldThrow()
    {
      var instance = GetInstance();
      var symbol = "ETHBTC";
      var date = DateTime.UtcNow.AddDays(-1);
      var expectedErrorMessage = "The date has to be in the future.";

      var actual = await Assert.ThrowsAsync<ArgumentException>(async () => await instance.PredictSymbolPriceForSpecificDayAsync(symbol, date, _token));

      Assert.StartsWith(expectedErrorMessage, actual.Message);
      Assert.Equal(nameof(date), actual.ParamName);
    }

    [Fact]
    public async Task PredictSymbolPriceAsync_WithValidParams_ShouldCallInitializeAsyncOnCandlestickCacheResource()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource);
      var symbol = "ETHBTC";
      var date = DateTime.UtcNow.AddDays(1);

      await instance.PredictSymbolPriceForSpecificDayAsync(symbol, date, _token);

      Received.InOrder(async () =>
      {
        await candlestickCacheResource.InitializeAsync(_token);
      });
    }

    [Fact]
    public async Task PredictSymbolPriceAsync_WithMultipleCalls_ShouldCallInitializeAsyncOnCandlestickCacheResourceOnlyOnce()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource);
      var symbol = "ETHBTC";
      var date = DateTime.UtcNow.AddDays(1);

      await instance.PredictSymbolPriceForSpecificDayAsync(symbol, date, _token);
      await instance.PredictSymbolPriceForSpecificDayAsync(symbol, date, _token);
      await instance.PredictSymbolPriceForSpecificDayAsync(symbol, date, _token);

      Received.InOrder(async () =>
      {
        await candlestickCacheResource.InitializeAsync(_token);
      });
    }

    [Fact]
    public async Task PredictSymbolPriceAsync_WithValidParams_ShouldCallGetCandlesticksAsyncOnCandlestickCacheResource()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource);
      var symbol = "ETHBTC";
      var expectedFromDate = DateTime.UtcNow.AddYears(-1);
      var expectedToDate = DateTime.UtcNow;
      var date = DateTime.UtcNow.AddDays(1);

      var actual = await instance.PredictSymbolPriceForSpecificDayAsync(symbol, date, _token);

      Received.InOrder(async () =>
      {
        await candlestickCacheResource.GetCandlesticksAsync(
          Arg.Is<List<string>>(symbols => symbols.Contains(symbol)), 
          Interval.OneDay, 
          Arg.Is<DateTime>(d => d.Date == expectedFromDate.Date),
          Arg.Is<DateTime>(d => d.Date == expectedToDate.Date),
          _token);
      });
    }

    private MLNetSymbolPricePredictionEngine GetInstance(ICandlestickCacheResource candlestickCacheResource = null)
    {
      return new MLNetSymbolPricePredictionEngine(candlestickCacheResource ?? Substitute.For<ICandlestickCacheResource>());
    }
  }
}