using FrostAura.Services.Plutus.Data.Resources;
using Xunit;

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public class FileSystemCandlestickCacheResourceTests
  {
    [Fact]
    public void Constructor_WithNoParams_ShouldConstruct()
    {
      var actual = GetInstance();

      Assert.NotNull(actual);
    }

    private FileSystemCandlestickCacheResource GetInstance()
    {
      return new FileSystemCandlestickCacheResource();
    }
  }
}
