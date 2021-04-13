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
  public partial class MLNetSymbolPriceForecastingEngineTest
  {
    private readonly CancellationToken _token = CancellationToken.None;

    [Fact]
    public void Constructor_WithInvalidCandlestickCacheResource_ShouldThrow()
    {
      ICandlestickCacheResource candlestickCacheResource = null;
      var fileResource = Substitute.For<IFileResource>();
      var configurationResource = Substitute.For<IConfigurationResource>();
      var actual = Assert.Throws<ArgumentNullException>(() => new MLNetSymbolPriceForecastingEngine(candlestickCacheResource, fileResource, configurationResource));

      Assert.Equal(nameof(candlestickCacheResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidFileResourceResource_ShouldThrow()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      IFileResource fileResource = null;
      var configurationResource = Substitute.For<IConfigurationResource>();
      var actual = Assert.Throws<ArgumentNullException>(() => new MLNetSymbolPriceForecastingEngine(candlestickCacheResource, fileResource, configurationResource));

      Assert.Equal(nameof(fileResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidConfigurationResourceResource_ShouldThrow()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      var fileResource = Substitute.For<IFileResource>();
      IConfigurationResource configurationResource = null;
      var actual = Assert.Throws<ArgumentNullException>(() => new MLNetSymbolPriceForecastingEngine(candlestickCacheResource, fileResource, configurationResource));

      Assert.Equal(nameof(configurationResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParams_ShouldConstruct()
    {
      var actual = GetInstance();

      Assert.NotNull(actual);
    }

    [Fact]
    public async Task ForecastSymbolPriceAsync_WithInvalidSymbol_ShouldThrow()
    {
      var instance = GetInstance();
      string symbol = default;
      var date = DateTime.UtcNow.AddDays(1);

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.ForecastSymbolPriceForSpecificDayAsync(symbol, date, _token));

      Assert.Equal(nameof(symbol), actual.ParamName);
    }

    [Fact]
    public async Task ForecastSymbolPriceAsync_WithDefaultDate_ShouldThrow()
    {
      var instance = GetInstance();
      var symbol = "ETHBTC";
      DateTime date = default;

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.ForecastSymbolPriceForSpecificDayAsync(symbol, date, _token));

      Assert.Equal(nameof(date), actual.ParamName);
    }

    [Fact]
    public async Task ForecastSymbolPriceAsync_WithPastDate_ShouldThrow()
    {
      var instance = GetInstance();
      var symbol = "ETHBTC";
      var date = DateTime.UtcNow.AddDays(-1);
      var expectedErrorMessage = "The date has to be in the future.";

      var actual = await Assert.ThrowsAsync<ArgumentException>(async () => await instance.ForecastSymbolPriceForSpecificDayAsync(symbol, date, _token));

      Assert.StartsWith(expectedErrorMessage, actual.Message);
      Assert.Equal(nameof(date), actual.ParamName);
    }

    [Fact]
    public async Task ForecastSymbolPriceAsync_WithValidParams_ShouldCallInitializeAsyncOnCandlestickCacheResource()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource);
      var symbol = "ETHBTC";
      var date = DateTime.UtcNow.AddDays(1);

      await instance.ForecastSymbolPriceForSpecificDayAsync(symbol, date, _token);

      Received.InOrder(async () =>
      {
        await candlestickCacheResource.InitializeAsync(_token);
      });
    }

    [Fact]
    public async Task ForecastSymbolPriceAsync_WithMultipleCalls_ShouldCallInitializeAsyncOnCandlestickCacheResourceOnlyOnce()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource);
      var symbol = "ETHBTC";
      var date = DateTime.UtcNow.AddDays(1);

      await instance.ForecastSymbolPriceForSpecificDayAsync(symbol, date, _token);
      await instance.ForecastSymbolPriceForSpecificDayAsync(symbol, date, _token);
      await instance.ForecastSymbolPriceForSpecificDayAsync(symbol, date, _token);

      Received.InOrder(async () =>
      {
        await candlestickCacheResource.InitializeAsync(_token);
      });
    }

    [Fact]
    public async Task ForecastSymbolPriceAsync_WithValidParams_ShouldCallGetCandlesticksAsyncOnCandlestickCacheResource()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource);
      var symbol = "ETHBTC";
      var expectedFromDate = DateTime.UtcNow.AddYears(-1);
      var expectedToDate = DateTime.UtcNow;
      var date = DateTime.UtcNow.AddDays(1);

      var actual = await instance.ForecastSymbolPriceForSpecificDayAsync(symbol, date, _token);

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

    [Fact]
    public async Task GenerateCandlestickForecastingFileAsync_WithInvalidSymbol_ShouldThrow()
    {
      var instance = GetInstance();
      string symbol = default;
      var interval = Interval.OneHour;
      var filePath = "test.csv";

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.GenerateCandlestickForecastingFileAsync(symbol, interval, filePath, _token));

      Assert.Equal(nameof(symbol), actual.ParamName);
    }

    [Fact]
    public async Task GenerateCandlestickForecastingFileAsync_WithInvalidFilePath_ShouldThrow()
    {
      var instance = GetInstance();
      var symbol = "ETHBTC";
      var interval = Interval.OneHour;
      string filePath = default;

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.GenerateCandlestickForecastingFileAsync(symbol, interval, filePath, _token));

      Assert.Equal(nameof(filePath), actual.ParamName);
    }

    [Fact]
    public async Task GenerateCandlestickForecastingFileAsync_WithValidParams_ShouldGetCandlestickDataFromCache()
    {
      var candlestickCacheResource = Substitute.For<ICandlestickCacheResource>();
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource);
      var symbol = "ETHBTC";
      var interval = Interval.OneHour;
      var fromDate = DateTime.UtcNow.AddYears(-1);
      var toDate = DateTime.UtcNow;
      var filePath = "test.csv";

      var actual = await instance.GenerateCandlestickForecastingFileAsync(symbol, interval, filePath, _token);

      Received.InOrder(async () =>
      {
        await candlestickCacheResource.GetCandlesticksAsync(Arg.Is<List<string>>(
          a => a.Contains(symbol)), 
          interval, 
          Arg.Is<DateTime>(a => a.Date == fromDate.Date),
          Arg.Is<DateTime>(a => a.Date == toDate.Date), 
          _token);
      });
    }

    private MLNetSymbolPriceForecastingEngine GetInstance(
      ICandlestickCacheResource candlestickCacheResource = null,
      IFileResource fileResource = null,
      IConfigurationResource configurationResource = null)
    {
      return new MLNetSymbolPriceForecastingEngine(
        candlestickCacheResource ?? Substitute.For<ICandlestickCacheResource>(),
        fileResource ?? Substitute.For<IFileResource>(),
        configurationResource ?? Substitute.For<IConfigurationResource>());
    }
  }
}