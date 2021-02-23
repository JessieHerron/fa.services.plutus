using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Shared.Consts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Resources
{
  /// <summary>
  /// Configuration resource that uses options in the back-end.
  /// </summary>
  public class OptionsConfigurationResource : IConfigurationResource
  {
    /// <summary>
    /// Get the currently configured exchange use.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Configured exchange.</returns>
    public Task<SupportedExchange> GetExchangeAsync(CancellationToken token)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get the pair list to use. 
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Pair list to use.</returns>
    public Task<IEnumerable<string>> GetPairsAsync(CancellationToken token)
    {
      throw new NotImplementedException();
    }
  }
}