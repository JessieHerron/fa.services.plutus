using Binance.Net.Interfaces;
using FrostAura.Services.Plutus.Data.Resources;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public partial class BinanceAssetResourceTests
  {
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
      var token = CancellationToken.None;

      await instance.InitializeAsync(token);
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
        ILogger<BinanceAssetResource> logger = null
      )
    {
      return new BinanceAssetResource(
          client ?? Substitute.For<IBinanceClient>(),
          socketClient ?? Substitute.For<IBinanceSocketClient>(),
          logger ?? Substitute.For<ILogger<BinanceAssetResource>>()
        );
    }
  }
}
