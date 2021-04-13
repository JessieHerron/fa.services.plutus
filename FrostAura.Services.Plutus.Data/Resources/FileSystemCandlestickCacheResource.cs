using FrostAura.Libraries.Core.Extensions.Validation;
using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Shared.Consts;
using FrostAura.Services.Plutus.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Resources
{
  /// <summary>
  /// A file system-based caching service for candlestick information.
  /// </summary>
  public class FileSystemCandlestickCacheResource : ICandlestickCacheResource
  {
    /// <summary>
    /// Provider for accessing the file system.
    /// </summary>
    private readonly IFileResource _fileResource;
    /// <summary>
    /// Provider for accessing the directory file system.
    /// </summary>
    private readonly IDirectoryResource _directoryResource;
    /// <summary>
    /// Configuration resource accessor used for providing various configuration from a respected source.
    /// </summary>
    private readonly IConfigurationResource _configurationResource;
    /// <summary>
    /// Resource to provide candlestick information from a respected source.
    /// </summary>
    private readonly ICandlestickResource _candlestickResource;
    /// <summary>
    /// Full cache directory path to use.
    /// </summary>
    private string _cacheDirectoryPath;

    /// <summary>
    /// Inject dependencies via DI.
    /// </summary>
    /// <param name="fileResource">Provider for accessing the file system.</param>
    /// <param name="directoryResource">Provider for accessing the directory file system.</param>
    /// <param name="configurationResource">Configuration resource accessor used for providing various configuration from a respected source.</param>
    /// <param name="candlestickResource">Resource to provide candlestick information from a respected source.</param>
    public FileSystemCandlestickCacheResource(IFileResource fileResource, IDirectoryResource directoryResource, IConfigurationResource configurationResource, ICandlestickResource candlestickResource)
    {
      _fileResource = fileResource.ThrowIfNull(nameof(fileResource));
      _directoryResource = directoryResource.ThrowIfNull(nameof(directoryResource));
      _configurationResource = configurationResource.ThrowIfNull(nameof(configurationResource));
      _candlestickResource = candlestickResource.ThrowIfNull(nameof(candlestickResource));
    }

    /// <summary>
    /// Initialize the asset resource async in order to allow for bootstrapping, subscriptions etc operations to occur.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task InitializeAsync(CancellationToken token)
    {
      var relativeCacheDirectoryPath = await _configurationResource.GetRelativeDirectoryPathForSymbolCachingAsync(token);
      var executingAssemblyPath = GetType()
        .Assembly
        .Location;

      _cacheDirectoryPath = Path.Combine(Path.GetDirectoryName(executingAssemblyPath), relativeCacheDirectoryPath);

      if (_directoryResource.Exists(_cacheDirectoryPath)) return;

      _directoryResource.CreateDirectory(_cacheDirectoryPath);
    }

    /// <summary>
    /// Get candlestick data for a given timeframe, given a collection of symbols.
    /// </summary>
    /// <param name="symbols">Collection of pairs to fetch the candlestick data for.</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="from">The starting date of the range which to fetch data for.</param>
    /// <param name="to">The end date of the range which to fetch data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A dictionary with the pair as the key and the candlestick data as the value.</returns>
    public async Task<IDictionary<string, IEnumerable<Candlestick>>> GetCandlesticksAsync(IEnumerable<string> symbols, Interval interval, DateTime from, DateTime to, CancellationToken token)
    {
      if (!symbols.ThrowIfNull(nameof(symbols)).Any()) throw new ArgumentException("Request can not be empty.", nameof(symbols));
      if (from > to) throw new ArgumentException("The from date is required to be before the to date.");

      var fileReadTasks = symbols
        .Select(s => ReadCandlestickForSymbolFromFileAsync(s, interval, from, to, token));
      var fileReadResults = await Task.WhenAll(fileReadTasks);
      var symbolsWithMissingFileSystemData = fileReadResults
        .Where(r => r.Data.Count() == 0)
        .Select(r => r.Symbol);
      var freshResultsFromResource = await FetchAndPersistSymbolsDataAsync(
        symbolsWithMissingFileSystemData,
        interval,
        from,
        to,
        token);
      var results = fileReadResults
        .Where(r => r.Data.Any())
        .Concat(freshResultsFromResource)
        .ToDictionary(r => r.Symbol, r => r.Data);

      return results;
    }

    /// <summary>
    /// Fetch data for symbols from the candlestick resource, persist it and return the populated response.
    /// </summary>
    /// <param name="symbols">Symbols to process.</param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="from">The starting date of the range which to fetch data for.</param>
    /// <param name="to">The end date of the range which to fetch data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Symbol with populated candlestick data.</returns>
    private async Task<IEnumerable<(string Symbol, IEnumerable<Candlestick> Data)>> FetchAndPersistSymbolsDataAsync(IEnumerable<string> symbols, Interval interval, DateTime from, DateTime to, CancellationToken token)
    {
      if (!symbols.Any()) return new List<(string Symbol, IEnumerable<Candlestick> Data)>();

      var resourceResults = await _candlestickResource.GetCandlesticksAsync(symbols, interval, from, to, token);
      var persistenceRequest = resourceResults
        .Select(r => (Symbol: r.Key, Interval: interval, Data: r.Value));
      
      await SetCandlesticksAsync(persistenceRequest, token);

      return resourceResults
        .Select(r => r.Key)
        .Select(k => (Symbol: k, Data: resourceResults[k]));
    }

    /// <summary>
    /// Set candlestick data for multiple symbols atomically.
    /// </summary>
    /// <param name="request">Collection of symbols with their intervals and candlestick data.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    private Task SetCandlesticksAsync(IEnumerable<(string Symbol, Interval Interval, IEnumerable<Candlestick> Data)> request, CancellationToken token)
    {
      if (!request.ThrowIfNull(nameof(request)).Any()) throw new ArgumentException("Request can not be empty.", nameof(request));

      // Ensure that a directory exists for each of the intervals.
      request
        .Select(r => r.Interval)
        .Distinct()
        .Where(i => !_directoryResource.Exists(Path.Combine(_cacheDirectoryPath, i.ToString())))
        .ToList()
        .ForEach(i => _directoryResource.CreateDirectory(Path.Combine(_cacheDirectoryPath, i.ToString())));

      var fileWriteTasks = request
        .Select(r => WriteCandlestickForSymbolToFileAsync(r.Symbol, r.Interval, r.Data, token));

      return Task.WhenAll(fileWriteTasks);
    }

    /// <summary>
    /// Persist a given request item to disk.
    /// </summary>
    /// <param name="symbol">Symbol the request is for. E.g. ETHBTC</param>
    /// <param name="interval">The interval / data resolution of the request.</param>
    /// <param name="data">The candlestick information for the provided symbol.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    private Task WriteCandlestickForSymbolToFileAsync(string symbol, Interval interval, IEnumerable<Candlestick> data, CancellationToken token)
    {
      symbol.ThrowIfNullOrWhitespace(nameof(symbol));
      data.ThrowIfNull(nameof(data));

      var intervalDirectory = Path.Combine(_cacheDirectoryPath, interval.ToString());
      var filePath = Path.Combine(intervalDirectory, $"{symbol.Replace("/", string.Empty)}.txt");
      var content = JsonConvert.SerializeObject(data);

      return _fileResource.WriteAllTextAsync(filePath, content, token);
    }

    /// <summary>
    /// Read candlestick information from the cache.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="interval">Interval to indicate the resolution for which to fetch the data for.</param>
    /// <param name="from">The starting date of the range which to fetch data for.</param>
    /// <param name="to">The end date of the range which to fetch data for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Candlestick data as the value for the given symbol.</returns>
    private async Task<(string Symbol, IEnumerable<Candlestick> Data)> ReadCandlestickForSymbolFromFileAsync(string symbol, Interval interval, DateTime from, DateTime to, CancellationToken token)
    {
      symbol.ThrowIfNullOrWhitespace(nameof(symbol));

      var intervalDirectory = Path.Combine(_cacheDirectoryPath, interval.ToString());
      var filePath = Path.Combine(intervalDirectory, $"{symbol.Replace("/", string.Empty)}.txt");

      if (!_fileResource.Exists(filePath)) return (symbol, new List<Candlestick>());

      var content = await _fileResource.ReadAllTextAsync(filePath, token);
      var parsedContent = JsonConvert.DeserializeObject<IEnumerable<Candlestick>>(content);
      var result = (symbol, parsedContent
        .Where(c => c.OpenTime > from && c.CloseTime < to));

      return result;
    }
  }
}
