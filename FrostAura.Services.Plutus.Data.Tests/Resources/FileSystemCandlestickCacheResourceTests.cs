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
      var candlestickResource = Substitute.For<ICandlestickResource>();

      var actual = Assert.Throws<ArgumentNullException>(() => new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource, candlestickResource));

      Assert.NotNull(actual);
      Assert.Equal(nameof(fileResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidDirectoryResource_ShouldThrow()
    {
      var fileResource = Substitute.For<IFileResource>();
      IDirectoryResource directoryResource = null;
      var configurationResource = Substitute.For<IConfigurationResource>();
      var candlestickResource = Substitute.For<ICandlestickResource>();

      var actual = Assert.Throws<ArgumentNullException>(() => new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource, candlestickResource));

      Assert.NotNull(actual);
      Assert.Equal(nameof(directoryResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidConfigurationResource_ShouldThrow()
    {
      var fileResource = Substitute.For<IFileResource>();
      var directoryResource = Substitute.For<IDirectoryResource>();
      IConfigurationResource configurationResource = null;
      var candlestickResource = Substitute.For<ICandlestickResource>();

      var actual = Assert.Throws<ArgumentNullException>(() => new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource, candlestickResource));

      Assert.NotNull(actual);
      Assert.Equal(nameof(configurationResource), actual.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidCandlestickResource_ShouldThrow()
    {
      var fileResource = Substitute.For<IFileResource>();
      var directoryResource = Substitute.For<IDirectoryResource>();
      var configurationResource = Substitute.For<IConfigurationResource>();
      ICandlestickResource candlestickResource = null;

      var actual = Assert.Throws<ArgumentNullException>(() => new FileSystemCandlestickCacheResource(fileResource, directoryResource, configurationResource, candlestickResource));

      Assert.NotNull(actual);
      Assert.Equal(nameof(candlestickResource), actual.ParamName);
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
    public async Task GetCandlesticksAsync_WithInvalidSymbols_ShouldThrow()
    {
      IEnumerable<string> symbols = default;
      var interval = Interval.OneDay;
      var fromDate = DateTime.UtcNow.AddYears(-1);
      var toDate = DateTime.UtcNow;
      var instance = GetInstance();

      var actual = await Assert.ThrowsAsync<ArgumentNullException>(async () => await instance.GetCandlesticksAsync(symbols, interval, fromDate, toDate, _token));

      Assert.Equal(nameof(symbols), actual.ParamName);
    }

    [Fact]
    public async Task GetCandlesticksAsync_WithEmptySymbols_ShouldThrow()
    {
      var symbols = new List<string>();
      var interval = Interval.OneDay;
      var fromDate = DateTime.UtcNow.AddYears(-1);
      var toDate = DateTime.UtcNow;
      var expectedError = "Request can not be empty.";
      var instance = GetInstance();

      var actual = await Assert.ThrowsAsync<ArgumentException>(async () => await instance.GetCandlesticksAsync(symbols, interval, fromDate, toDate, _token));

      Assert.Equal(nameof(symbols), actual.ParamName);
      Assert.StartsWith(expectedError, actual.Message);
    }

    [Fact]
    public async Task GetCandlesticksAsync_WithFromDateGreaterThanToDate_ShouldThrow()
    {
      var symbols = new List<string> { "ETHBTC" };
      var interval = Interval.OneDay;
      var fromDate = DateTime.UtcNow;
      var toDate = DateTime.UtcNow.AddYears(-1);
      var expectedError = "The from date is required to be before the to date.";
      var instance = GetInstance();

      var actual = await Assert.ThrowsAsync<ArgumentException>(async () => await instance.GetCandlesticksAsync(symbols, interval, fromDate, toDate, _token));

      Assert.StartsWith(expectedError, actual.Message);
    }

    [Fact]
    public async Task GetCandlesticksAsync_WithValidParams_ShouldCheckIfFileExistsForEachSymbol()
    {
      var fileProvider = Substitute.For<IFileResource>();
      var symbol = "ETHBTC";
      var symbols = new List<string> { symbol };
      var interval = Interval.OneDay;
      var fromDate = DateTime.UtcNow.AddYears(-1);
      var toDate = DateTime.UtcNow;
      var expectedFileName = $"{symbol.Replace("/", string.Empty)}.txt";
      var expectedContentFromFile = new List<Candlestick>
      {
        new Candlestick
        {
          OpenTime = fromDate.AddDays(10),
          CloseTime = toDate.AddDays(-1)
        }
      };
      var instance = GetInstance(fileResource: fileProvider);

      fileProvider
        .Exists(Arg.Is<string>(f => f.EndsWith(expectedFileName)))
        .Returns(true);
      fileProvider
        .ReadAllTextAsync(default, _token)
        .ReturnsForAnyArgs(JsonConvert.SerializeObject(expectedContentFromFile));

      await instance.InitializeAsync(_token);
      await instance.GetCandlesticksAsync(symbols, interval, fromDate, toDate, _token);

      Received.InOrder(() =>
      {
        fileProvider.Exists(Arg.Is<string>(f => f.EndsWith(expectedFileName)));
      });
    }

    [Fact]
    public async Task GetCandlesticksAsync_WithExistingFile_ShouldReadSymbolContentFromFile()
    {
      var fileProvider = Substitute.For<IFileResource>();
      var symbol = "ETHBTC";
      var symbols = new List<string> { symbol };
      var interval = Interval.OneDay;
      var fromDate = DateTime.UtcNow.AddYears(-1);
      var toDate = DateTime.UtcNow;
      var expectedFileName = $"{symbol.Replace("/", string.Empty)}.txt";
      var expectedContentFromFile = new List<Candlestick>
      {
        new Candlestick
        {
          OpenTime = fromDate.AddDays(10),
          CloseTime = toDate.AddDays(-1)
        }
      };
      var instance = GetInstance(fileResource: fileProvider);

      fileProvider
        .Exists(Arg.Is<string>(f => f.EndsWith(expectedFileName)))
        .Returns(true);
      fileProvider
        .ReadAllTextAsync(default, _token)
        .ReturnsForAnyArgs(JsonConvert.SerializeObject(expectedContentFromFile));

      await instance.InitializeAsync(_token);
      await instance.GetCandlesticksAsync(symbols, interval, fromDate, toDate, _token);
      Received.InOrder(() =>
      {
        fileProvider.Exists(Arg.Is<string>(f => f.EndsWith(expectedFileName)));
        fileProvider.ReadAllTextAsync(Arg.Is<string>(f => f.EndsWith(expectedFileName)), _token);
      });
    }

    [Fact]
    public async Task GetCandlesticksAsync_WithNonExistingFile_ShouldFetchDataFromResource()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task GetCandlesticksAsync_AfterFetchingFromResource_ShouldPersistToFile()
    {
      throw new NotImplementedException();
    }

    [Fact]
    public async Task GetCandlesticksAsync_WithResultsFromCacheAndResource_ShouldReturnConcatResult()
    {
      throw new NotImplementedException();
    }

    private FileSystemCandlestickCacheResource GetInstance(
    IFileResource fileResource = null,
    IDirectoryResource directoryResource = null,
    IConfigurationResource configurationResource = null,
    ICandlestickResource candlestickResource = null)
    {
      var mockedConfigurationResource = Substitute.For<IConfigurationResource>();

      mockedConfigurationResource
        .GetRelativeDirectoryPathForSymbolCaching(_token)
        .Returns(_cacheDirectory);

      return new FileSystemCandlestickCacheResource(
        fileResource ?? Substitute.For<IFileResource>(),
        directoryResource ?? Substitute.For<IDirectoryResource>(),
        configurationResource ?? mockedConfigurationResource,
        candlestickResource ?? Substitute.For<ICandlestickResource>()
      );
    }
  }
}
