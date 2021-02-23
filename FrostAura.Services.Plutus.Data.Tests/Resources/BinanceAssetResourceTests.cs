using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.SubClients.Spot;
using CryptoExchange.Net.Objects;
using FrostAura.Services.Plutus.Data.Resources;
using FrostAura.Services.Plutus.Shared.Consts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public partial class BinanceAssetResourceTests
  {
    List<string> symbols = new List<string>
      {
        "RCN/BTC",
        "LRC/BTC",
        "SUSHI/BTC"
      };
    Interval interval = Interval.FifteenMinutes;
    DateTime from = DateTime.UtcNow.AddDays(-90);
    DateTime to = DateTime.UtcNow;
    CancellationToken token = CancellationToken.None;

    [Fact]
    public void Constructor_WithInvalidClient_ShouldThrow()
    {
      IBinanceClient client = null;
      var actual = Assert.Throws<ArgumentNullException>(() => new BinanceAssetResource(
          client,
          Substitute.For<IBinanceSocketClient>(),
          Substitute.For<ILogger<BinanceAssetResource>>()
        ));

      Assert.Equal(nameof(client), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidSocketClient_ShouldThrow()
    {
      IBinanceSocketClient socketClient = null;
      var actual = Assert.Throws<ArgumentNullException>(() => new BinanceAssetResource(
          Substitute.For<IBinanceClient>(),
          socketClient,
          Substitute.For<ILogger<BinanceAssetResource>>()
        ));

      Assert.Equal(nameof(socketClient), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidLogger_ShouldThrow()
    {
      ILogger<BinanceAssetResource> logger = null;
      var actual = Assert.Throws<ArgumentNullException>(() => new BinanceAssetResource(
          Substitute.For<IBinanceClient>(),
          Substitute.For<IBinanceSocketClient>(),
          logger
        ));

      Assert.Equal(nameof(logger), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParams_ShouldNotThrow()
    {
      var instance = GetInstance();

      Assert.NotNull(instance);
    }

    [Fact]
    public async Task InitializeAsync_WithValidParams_ShouldNotThrow()
    {
      var instance = GetInstance();

      await instance.InitializeAsync(token);
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WithNullSymbols_ShouldThrow()
    {
      var instance = GetInstance();
      IEnumerable<string> symbols = null;

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.GetCandlestickDataForPairsAsync(symbols, interval, from, to, token));

      Assert.Equal(nameof(symbols), actual.ParamName);
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WithEmptySymbols_ShouldThrow()
    {
      var instance = GetInstance();
      var symbols = new List<string>();
      var expectedErrorMessage = $"At least one symbol should be provided. (Parameter '{nameof(symbols)}')";

      var actual = await Assert.ThrowsAsync<ArgumentException>(async () => await instance.GetCandlestickDataForPairsAsync(symbols, interval, from, to, token));

      Assert.Equal(nameof(symbols), actual.ParamName);
      Assert.Equal(expectedErrorMessage, actual.Message);
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WithValidParams_ShouldCallLoggerForInformation()
    {
      var logger = Substitute.For<ILogger<BinanceAssetResource>>();
      var instance = GetInstance(logger: logger);
      var expectedMessage = $"Fetching candlestick data from Binance for {symbols.Count()} symbols at '{Enum.GetName(typeof(Interval), (int)interval)}' interval from {from.ToShortDateString()} to {to.ToShortDateString()}.";

      var actual = await instance.GetCandlestickDataForPairsAsync(symbols, interval, from, to, token);

      logger
        .Received()
        .LogInformation(expectedMessage);
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WithValidParams_ShouldCallGetKlinesAsyncForEachPair()
    {
      var market = Substitute.For<IBinanceClientSpotMarket>();
      var instance = GetInstance(spotClientMarket: market);
      var resultData = new List<IBinanceKline>();
      var results = new WebCallResult<IEnumerable<IBinanceKline>>(code: HttpStatusCode.OK, responseHeaders: new List<KeyValuePair<string, IEnumerable<string>>>(), error: null, data: resultData);

      market
        .GetKlinesAsync(Arg.Any<string>(), KlineInterval.FifteenMinutes, from, to, ct: token)
        .Returns(ci => results);

      var actual = await instance.GetCandlestickDataForPairsAsync(symbols, interval, from, to, token);

      Received.InOrder(async () =>
      {
        await market.GetKlinesAsync(Arg.Is<string>(a => a == symbols[0].Replace("/", string.Empty)), KlineInterval.FifteenMinutes, from, to, ct: token);
        await market.GetKlinesAsync(Arg.Is<string>(a => a == symbols[1].Replace("/", string.Empty)), KlineInterval.FifteenMinutes, from, to, ct: token);
        await market.GetKlinesAsync(Arg.Is<string>(a => a == symbols[2].Replace("/", string.Empty)), KlineInterval.FifteenMinutes, from, to, ct: token);
      });
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WithErrors_ShouldCallLowWarningForErrors()
    {
      var logger = Substitute.For<ILogger<BinanceAssetResource>>();
      var market = Substitute.For<IBinanceClientSpotMarket>();
      var instance = GetInstance(spotClientMarket: market, logger: logger);
      var resultData = new List<IBinanceKline>();
      var result = new WebCallResult<IEnumerable<IBinanceKline>>(code: HttpStatusCode.OK, responseHeaders: new List<KeyValuePair<string, IEnumerable<string>>>(), error: null, data: resultData);
      var error = Substitute.For<Error>(1, "Test failure.", null);
      var errorResult = new WebCallResult<IEnumerable<IBinanceKline>>(code: HttpStatusCode.BadRequest, responseHeaders: new List<KeyValuePair<string, IEnumerable<string>>>(), error: error, data: resultData);
      var expectedMessage = $"Failed to fetch candlestick data for symbol '{symbols.First()}' with code {error.Code} and message '{error.Message}'.";

      market
        .GetKlinesAsync(Arg.Any<string>(), KlineInterval.FifteenMinutes, from, to, ct: token)
        .Returns(result);
      market
        .GetKlinesAsync(symbols.First().Replace("/", string.Empty), KlineInterval.FifteenMinutes, from, to, ct: token)
        .Returns(errorResult);

      var actual = await instance.GetCandlestickDataForPairsAsync(symbols, interval, from, to, token);

      logger
        .Received()
        .LogWarning(expectedMessage);
    }

    [Fact]
    public async Task GetCandlestickDataForPairsAsync_WithSuccesses_ShouldReturnMappedResponse()
    {
      var logger = Substitute.For<ILogger<BinanceAssetResource>>();
      var market = Substitute.For<IBinanceClientSpotMarket>();
      var instance = GetInstance(spotClientMarket: market, logger: logger);
      var candle1 = Substitute.For<IBinanceKline>();
      var candle2 = Substitute.For<IBinanceKline>();
      var resultData = new List<IBinanceKline>
      {
        candle1,
        candle2
      };
      var result = new WebCallResult<IEnumerable<IBinanceKline>>(code: HttpStatusCode.OK, responseHeaders: new List<KeyValuePair<string, IEnumerable<string>>>(), error: null, data: resultData);

      market
        .GetKlinesAsync(Arg.Any<string>(), KlineInterval.FifteenMinutes, from, to, ct: token)
        .Returns(result);
      candle1
        .OpenTime
        .Returns(DateTime.Now.AddSeconds(-15));
      candle2
        .OpenTime
        .Returns(DateTime.Now.AddSeconds(-15));
      candle1
        .CloseTime
        .Returns(DateTime.Now);
      candle2
        .CloseTime
        .Returns(DateTime.Now);
      candle1
        .Open
        .Returns(123);
      candle2
        .Open
        .Returns(123);
      candle1
        .High
        .Returns(456);
      candle2
        .High
        .Returns(456);
      candle1
        .Low
        .Returns(111);
      candle2
        .Low
        .Returns(111);
      candle1
        .BaseVolume
        .Returns(999);
      candle2
        .BaseVolume
        .Returns(999);
      candle1
        .TradeCount
        .Returns(888);
      candle2
        .TradeCount
        .Returns(888);

      var actual = await instance.GetCandlestickDataForPairsAsync(symbols, interval, from, to, token);

      // Assert mapped response.
      foreach (var symbol in symbols)
      {
        var candles = actual[symbol].ToList();

        Assert.Equal(123, candles[0].Open);
        Assert.Equal(123, candles[1].Open);
        Assert.Equal(456, candles[0].High);
        Assert.Equal(456, candles[1].High);
        Assert.Equal(111, candles[0].Low);
        Assert.Equal(111, candles[1].Low);
        Assert.Equal(999, candles[0].Volume);
        Assert.Equal(999, candles[1].Volume);
        Assert.Equal(888, candles[0].TradeCount);
        Assert.Equal(888, candles[1].TradeCount);
      }
    }

    [Fact]
    public async Task DisposeAsync_WithNoParams_ShouldNotThrow()
    {
      var instance = GetInstance();

      await instance.DisposeAsync();
    }

    private BinanceAssetResource GetInstance(
        IBinanceClient client = null,
        IBinanceSocketClient socketClient = null,
        ILogger<BinanceAssetResource> logger = null,
        IBinanceClientSpotMarket spotClientMarket = null
      )
    {
      var clientSubstritute = Substitute.For<IBinanceClient>();
      var spotClient = Substitute.For<IBinanceClientSpot>();
      spotClientMarket = spotClientMarket ?? Substitute.For<IBinanceClientSpotMarket>();
      var resultData = new List<IBinanceKline>();
      var results = new WebCallResult<IEnumerable<IBinanceKline>>(code: HttpStatusCode.OK, responseHeaders: new List<KeyValuePair<string, IEnumerable<string>>>(), error: null, data: resultData);

      clientSubstritute
        .Spot
        .Returns(spotClient);
      spotClient
        .Market
        .Returns(spotClientMarket);
      spotClientMarket
        .GetKlinesAsync(Arg.Any<string>(), KlineInterval.FifteenMinutes, from, to, ct: token)
        .Returns(ci => results);

      return new BinanceAssetResource(
          client ?? clientSubstritute,
          socketClient ?? Substitute.For<IBinanceSocketClient>(),
          logger ?? Substitute.For<ILogger<BinanceAssetResource>>()
        );
    }
  }
}
