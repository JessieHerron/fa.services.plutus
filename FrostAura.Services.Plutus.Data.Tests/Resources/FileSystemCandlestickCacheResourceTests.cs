using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Data.Resources;
using FrostAura.Services.Plutus.Data.Tests.Helpers;
using NSubstitute;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public class FileSystemCandlestickCacheResourceTests
  {
    private CancellationToken _token = CancellationToken.None;

    [Fact]
    public void Constructor_WithInvalidFileResource_ShouldThrow()
    {
      IFileResource fileResource = null;
      var directoryResource = Substitute.For<IDirectoryResource>();
      var configurationResource = Substitute.For<IConfigurationResource>();

      var actual = Assert.Throws<ArgumentNullException>(() => new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource));

      Assert.NotNull(actual);
      Assert.Equal(nameof(fileResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidDirectoryResource_ShouldThrow()
    {
      var fileResource = Substitute.For<IFileResource>();
      IDirectoryResource directoryResource = null;
      var configurationResource = Substitute.For<IConfigurationResource>();

      var actual = Assert.Throws<ArgumentNullException>(() => new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource));

      Assert.NotNull(actual);
      Assert.Equal(nameof(directoryResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidConfigurationResource_ShouldThrow()
    {
      var fileResource = Substitute.For<IFileResource>();
      var directoryResource = Substitute.For<IDirectoryResource>();
      IConfigurationResource configurationResource = null;

      var actual = Assert.Throws<ArgumentNullException>(() => new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource));

      Assert.NotNull(actual);
      Assert.Equal(nameof(configurationResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParams_ShouldConstruct()
    {
      var actual = GetInstance();

      Assert.NotNull(actual);
    }

    [Fact]
    public async Task InitializeAsync_WithValidParams_ShouldCallGetRelativeDirectoryPathForSymbolCachingOnConfigurationResource()
    {
      var configurationResource = Substitute.For<IConfigurationResource>();
      var instance = GetInstance(configurationResource: configurationResource);

      await instance.InitializeAsync(_token);
    }

    [Fact]
    public async Task InitializeAsync_WithValidParams_ShouldSetCorrectCacheDirectory()
    {
      var configurationResource = Substitute.For<IConfigurationResource>();
      var instance = GetInstance(configurationResource: configurationResource);
      var expectedAssemblyPath = Path.GetDirectoryName(typeof(FileSystemCandlestickCacheResource)
        .Assembly
        .Location);
      var expectedCacheDirectory = "cache/symbols";
      var expectedFullCacheDirectoryPath = Path.Combine(expectedAssemblyPath, expectedCacheDirectory);

      configurationResource
        .GetRelativeDirectoryPathForSymbolCaching(_token)
        .Returns(expectedCacheDirectory);

      await instance.InitializeAsync(_token);

      var actual = new PrivateObject(instance).GetPrivateFieldValue<string>("_cacheDirectoryPath");

      Assert.Equal(expectedFullCacheDirectoryPath, actual);
    }

    private FileSystemCandlestickCacheResource GetInstance(
      IFileResource fileResource = null, 
      IDirectoryResource directoryResource = null,
      IConfigurationResource configurationResource = null)
    {
      return new FileSystemCandlestickCacheResource(
        fileResource ?? Substitute.For<IFileResource>(),
        directoryResource ?? Substitute.For<IDirectoryResource>(),
        configurationResource ?? Substitute.For<IConfigurationResource>()
      );
    }
  }
}
