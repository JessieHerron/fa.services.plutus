using Binance.Net;
using FrostAura.Services.Plutus.Data.Resources;
using FrostAura.Services.Plutus.Shared.Decorators;
using FrostAura.Services.Plutus.Shared.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections.Generic;
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

      using(timer)
      {
        results = await instance.GetCandlestickDataForPairsAsync(symbols, interval, from, to, token);
      }

      var expectedMessageBeginning = $"Candlestick data fetch completed in {(int)timer.Stopwatch.Elapsed.TotalSeconds} seconds.";

      logger
        .Received()
        .LogInformation(expectedMessageBeginning);

      Assert.NotEmpty(results);
    }
  }
}
