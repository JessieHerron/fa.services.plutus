using FrostAura.Services.Plutus.Shared.Consts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Interfaces
{
  /// <summary>
  /// Configuration resource accessor used for providing various configuration from a respected source.
  /// </summary>
  public interface IConfigurationResource
  {
    /// <summary>
    /// Get the exchange use.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Exchange to use.</returns>
    Task<SupportedExchange> GetExchangeAsync(CancellationToken token);

    /// <summary>
    /// Get the pair list to use. 
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Pair list to use.</returns>
    Task<IEnumerable<string>> GetSymbolsAsync(CancellationToken token);
  }
}