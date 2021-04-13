using Binance.Net;
using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Data.Resources;
using FrostAura.Services.Plutus.Shared.Consts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace FrostAura.Services.Plutus.Core.Tests.Engines
{
  public partial class MLNetSymbolPriceForecastingEngineTest
  {
    private const string SKIP_JUSTIFICATION_MESSAGE = null;//"Skipping due to being a long running integration test.";

    [Fact(Skip = SKIP_JUSTIFICATION_MESSAGE)]
    public async Task GenerateCandlestickForecastingFileAsync_WithRealRequest_ShouldPersistDataToFileAndReturnPath()
    {
      var binanceLogger = Substitute.For<ILogger<BinanceApiResource>>();
      var fileResource = new FileResource();
      var directoryResource = new DirectoryResource();
      var candlestickResource = new BinanceApiResource(new BinanceClient(), new BinanceSocketClient(), binanceLogger);
      var configurationResource = Substitute.For<IConfigurationResource>();
      var candlestickCacheResource = new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource, candlestickResource);
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource);
      var symbol = "ETHBTC";
      var relativePath = "cache";
      var interval = Interval.OneDay;
      var filePath = Path.Combine(relativePath, interval.ToString(), $"{symbol}.ml.csv");

      configurationResource
        .GetRelativeDirectoryPathForSymbolCachingAsync(_token)
        .Returns(relativePath);

      var actual = await instance.GenerateCandlestickForecastingFileAsync(symbol, interval, filePath, _token);
    }

    [Fact(Skip = SKIP_JUSTIFICATION_MESSAGE)]
    public async Task ForecastNextCandlestickPercentageDeltaAsync_WithRealRequest_ShouldForecastNextCandlestickValue()
    {
      var binanceLogger = Substitute.For<ILogger<BinanceApiResource>>();
      var fileResource = new FileResource();
      var directoryResource = new DirectoryResource();
      var candlestickResource = new BinanceApiResource(new BinanceClient(), new BinanceSocketClient(), binanceLogger);
      var configurationResource = Substitute.For<IConfigurationResource>();
      var candlestickCacheResource = new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource, candlestickResource);
      var instance = GetInstance(candlestickCacheResource: candlestickCacheResource, configurationResource: configurationResource);
      var symbol = "ETHBTC";
      var relativePath = "cache";
      var interval = Interval.ThreeDay;

      configurationResource
        .GetRelativeDirectoryPathForSymbolCachingAsync(_token)
        .Returns(relativePath);

      var actual = await instance.ForecastNextCandlestickPercentageDeltaAsync(symbol, interval, _token);
    }
  }
}