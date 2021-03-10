using FrostAura.Services.Plutus.Data.Interfaces;
using NSubstitute;
using System;
using Xunit;

namespace FrostAura.Services.Plutus.Data.Resources
{
  public class MLNetSymbolPricePredictionResourceTest
  {
    [Fact]
    public void Constructor_WithInvalidCandlestickCacheResourceParams_ShouldThrow()
    {
      ICandlestickCacheResource candlestickCacheResource = null;
      var actual = Assert.Throws<ArgumentNullException>(() => new MLNetSymbolPricePredictionResource(candlestickCacheResource));

      Assert.Equal(nameof(candlestickCacheResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParams_ShouldConstruct()
    {
      var actual = GetInstance();

      Assert.NotNull(actual);
    }

    private MLNetSymbolPricePredictionResource GetInstance(ICandlestickCacheResource candlestickCacheResource = null)
    {
      return new MLNetSymbolPricePredictionResource(candlestickCacheResource ?? Substitute.For<ICandlestickCacheResource>());
    }
  }
}