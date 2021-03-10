using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Data.Resources;
using FrostAura.Services.Plutus.Data.Tests.Helpers;
using FrostAura.Services.Plutus.Shared.Consts;
using FrostAura.Services.Plutus.Shared.Models;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FrostAura.Services.Plutus.Data.Tests.Resources
{
  public partial class FileSystemCandlestickCacheResourceTests
  {
    private CancellationToken _token = CancellationToken.None;
    private readonly string _cacheDirectory = "cache";

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

    [Fact]
    public async Task InitializeAsync_WithExistingDirectory_ShouldNotCallCreateDirectoryOnDirectoryResource()
    {
      var directoryResource = Substitute.For<IDirectoryResource>();
      var instance = GetInstance(directoryResource: directoryResource);
      var directoryExists = true;

      directoryResource
        .Exists(default)
        .ReturnsForAnyArgs(directoryExists);

      await instance.InitializeAsync(_token);

      directoryResource
        .DidNotReceiveWithAnyArgs()
        .CreateDirectory(default);
    }

    [Fact]
    public async Task InitializeAsync_WithNonExistingDirectory_ShouldCallCreateDirectoryOnDirectoryResource()
    {
      var directoryResource = Substitute.For<IDirectoryResource>();
      var instance = GetInstance(directoryResource: directoryResource);
      var directoryExists = false;

      directoryResource
        .Exists(default)
        .ReturnsForAnyArgs(directoryExists);

      await instance.InitializeAsync(_token);

      directoryResource
        .ReceivedWithAnyArgs()
        .CreateDirectory(default);
    }

    [Fact]
    public async Task SetCandlesticksAsync_WithNullRequest_ShouldThrow()
    {
      var instance = GetInstance();
      IEnumerable<(string Symbol, Interval Interval, IEnumerable<Candlestick> Data)> request = null;

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.SetCandlesticksAsync(request, _token));

      Assert.NotNull(actual);
      Assert.Equal(nameof(request), actual.ParamName);
    }

    [Fact]
    public async Task SetCandlesticksAsync_WithEmptyRequest_ShouldThrow()
    {
      var instance = GetInstance();
      var request = new List<(string Symbol, Interval Interval, IEnumerable<Candlestick> Data)>();

      var actual = await Assert.ThrowsAsync<ArgumentException>(async () => await instance.SetCandlesticksAsync(request, _token));

      Assert.NotNull(actual);
      Assert.Equal(nameof(request), actual.ParamName);
    }

    [Fact]
    public async Task SetCandlesticksAsync_WithValidRequest_ShouldCheckExistenceOfEachIntervalDirectory()
    {
      var directoryResource = Substitute.For<IDirectoryResource>();
      var instance = GetInstance(directoryResource: directoryResource);
      var request = new List<(string Symbol, Interval Interval, IEnumerable<Candlestick> Data)>
      {
        ("RCN/BTC", Interval.FifteenMinutes, new List<Candlestick>()),
        ("LRC/BTC", Interval.OneHour, new List<Candlestick>()),
        ("XRP/BTC", Interval.FifteenMinutes, new List<Candlestick>())
      };

      await instance.InitializeAsync(_token);
      await instance.SetCandlesticksAsync(request, _token);

      foreach (var interval in request
        .Select(r => r.Interval))
      {
        directoryResource
          .Received()
          .Exists(Arg.Is<string>(s => s.EndsWith(interval.ToString())));
      }
    }

    [Fact]
    public async Task SetCandlesticksAsync_WithNonExistingIntervalDirectories_ShouldCreateDirectoriesForIntervals()
    {
      var directoryResource = Substitute.For<IDirectoryResource>();
      var instance = GetInstance(directoryResource: directoryResource);
      var request = new List<(string Symbol, Interval Interval, IEnumerable<Candlestick> Data)>
      {
        ("RCN/BTC", Interval.FifteenMinutes, new List<Candlestick>()),
        ("LRC/BTC", Interval.OneHour, new List<Candlestick>()),
        ("XRP/BTC", Interval.FifteenMinutes, new List<Candlestick>())
      };

      directoryResource
        .Exists(default)
        .ReturnsForAnyArgs(false);

      await instance.InitializeAsync(_token);
      await instance.SetCandlesticksAsync(request, _token);

      foreach (var interval in request
        .Select(r => r.Interval))
      {
        directoryResource
          .Received()
          .CreateDirectory(Arg.Is<string>(s => s.EndsWith(interval.ToString())));
      }
    }

    [Fact]
    public async Task SetCandlesticksAsync_WithValidRequest_ShouldPersistEachSymbolsDataToFile()
    {
      var directoryResource = Substitute.For<IDirectoryResource>();
      var fileResource = Substitute.For<IFileResource>();
      var instance = GetInstance(directoryResource: directoryResource, fileResource: fileResource);
      var request = new List<(string Symbol, Interval Interval, IEnumerable<Candlestick> Data)>
      {
        ("RCN/BTC", Interval.FifteenMinutes, new List<Candlestick>
        {
          new Candlestick
          {
            Volume = 1234,
            TradeCount = 5678
          }
        }),
        ("LRC/BTC", Interval.OneHour, new List<Candlestick>()),
        ("XRP/BTC", Interval.FifteenMinutes, new List<Candlestick>())
      };

      directoryResource
        .Exists(default)
        .ReturnsForAnyArgs(true);

      await instance.InitializeAsync(_token);
      await instance.SetCandlesticksAsync(request, _token);

      foreach (var requestItem in request)
      {
        var expectedFileEnding = Path.Combine(requestItem.Interval.ToString(), $"{requestItem.Symbol.Replace("/", string.Empty)}.txt");
        var expectedContent = JsonConvert.SerializeObject(requestItem.Data);

        fileResource
          .Received()
          .WriteAllTextAsync(Arg.Is<string>(s => s.EndsWith(expectedFileEnding)), expectedContent, _token);
      }
    }

    [Fact]
    public async Task WriteCandlestickForSymbolToFileAsync_WithInvalidSymbol_ShouldThrow()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task WriteCandlestickForSymbolToFileAsync_WithInvalidData_ShouldThrow()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task WriteCandlestickForSymbolToFileAsync_WithValidParams_ShouldWriteAllTextAsync()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task ReadCandlestickForSymbolFromFileAsync_WithInvalidSymbol_ShouldThrow()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task ReadCandlestickForSymbolFromFileAsync_WithValidParams_ShouldCheckFileExists()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task ReadCandlestickForSymbolFromFileAsync_WithNonExistingFile_ShouldReturnEmpty()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task ReadCandlestickForSymbolFromFileAsync_WithExistingFile_ShouldReadAllTextAsync()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task ReadCandlestickForSymbolFromFileAsync_WithExistingFile_ShouldReturnSymbolAndResults()
    {
      throw new NotImplementedException();
    }

    private FileSystemCandlestickCacheResource GetInstance(
      IFileResource fileResource = null,
      IDirectoryResource directoryResource = null,
      IConfigurationResource configurationResource = null)
    {
      var mockedConfigurationResource = Substitute.For<IConfigurationResource>();

      mockedConfigurationResource
        .GetRelativeDirectoryPathForSymbolCaching(_token)
        .Returns(_cacheDirectory);

      return new FileSystemCandlestickCacheResource(
        fileResource ?? Substitute.For<IFileResource>(),
        directoryResource ?? Substitute.For<IDirectoryResource>(),
        configurationResource ?? mockedConfigurationResource
      );
    }
  }
}
