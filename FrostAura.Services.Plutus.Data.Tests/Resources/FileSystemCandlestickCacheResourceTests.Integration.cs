using Binance.Net;
using FrostAura.Services.Plutus.Data.Resources;
using FrostAura.Services.Plutus.Shared.Consts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public partial class FileSystemCandlestickCacheResourceTests
  {
    private const string SKIP_JUSTIFICATION_MESSAGE = null; // "Skipping due to being a long running integration test.";

    [Fact(Skip = SKIP_JUSTIFICATION_MESSAGE)]
    public async Task SetCandlesticksAsync_WithRealRequest_ShouldPersistEachSymbolsDataToFile()
    {
      var directoryResource = new DirectoryResource();
      var binanceLogger = Substitute.For<ILogger<BinanceApiResource>>();
      var fileResource = new FileResource();
      var configurationResource = new StaticConfigurationResource();
      var candlestickResource = new BinanceApiResource(new BinanceClient(), new BinanceSocketClient(), binanceLogger);
      var instance = new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource);
      var symbols = await configurationResource.GetSymbolsAsync(_token);
      var candlestickData = await candlestickResource.GetCandlesticksAsync(symbols, Interval.FifteenMinutes, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow, _token);
      var request = candlestickData
        .Select(c => (Symbol: c.Key, Interval: Interval.FifteenMinutes, Data: c.Value));

      await instance.InitializeAsync(_token);
      await instance.SetCandlesticksAsync(request, _token);
    }
  }
}
