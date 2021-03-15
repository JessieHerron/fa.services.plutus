using Binance.Net;
using FrostAura.Services.Plutus.Data.Resources;
using FrostAura.Services.Plutus.Shared.Consts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public partial class FileSystemCandlestickCacheResourceTests
  {
    private const string SKIP_JUSTIFICATION_MESSAGE = "Skipping due to being a long running integration test.";

    [Fact(Skip = SKIP_JUSTIFICATION_MESSAGE)]
    public async Task SetCandlesticksAsync_WithRealRequest_ShouldPersistEachSymbolsDataToFile()
    {
      var directoryResource = new DirectoryResource();
      var binanceLogger = Substitute.For<ILogger<BinanceApiResource>>();
      var fileResource = new FileResource();
      var configurationResource = new StaticConfigurationResource();
      var candlestickResource = new BinanceApiResource(new BinanceClient(), new BinanceSocketClient(), binanceLogger);
      var instance = new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource, candlestickResource);
      var symbols = await configurationResource.GetSymbolsAsync(_token);

      await instance.InitializeAsync(_token);

      var actual = await instance.GetCandlesticksAsync(symbols, Interval.FifteenMinutes, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow, _token);

      foreach (var symbol in actual.Keys)
      {
        Assert.NotEmpty(actual[symbol]);
      }
    }
  }
}
