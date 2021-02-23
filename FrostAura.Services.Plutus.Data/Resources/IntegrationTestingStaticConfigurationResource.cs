using FrostAura.Services.Plutus.Data.Interfaces;
using FrostAura.Services.Plutus.Shared.Consts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrostAura.Services.Plutus.Data.Resources
{
  /// <summary>
  /// Configuration resource that uses options in the back-end.
  /// </summary>
  public class IntegrationTestingStaticConfigurationResource : IConfigurationResource
  {
    /// <summary>
    /// Get the currently configured exchange use.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Configured exchange.</returns>
    public Task<SupportedExchange> GetExchangeAsync(CancellationToken token)
    {
      return Task.FromResult(SupportedExchange.Binance);
    }

    /// <summary>
    /// Get the pair list to use. 
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Pair list to use.</returns>
    public Task<IEnumerable<string>> GetPairsAsync(CancellationToken token)
    {
      var response = new List<string>
      {
        "RCN/BTC",
        "LRC/BTC",
        "SUSHI/BTC",
        "XLM/BTC",
        "BQX/BTC",
        "FTM/BTC",
        "ADA/BTC",
        "ONT/BTC",
        "BRD/BTC",
        "XRP/BTC",
        "XEM/BTC",
        "LOOM/BTC",
        "ETH/BTC",
        "GRT/BTC",
        "DNT/BTC",
        "MANA/BTC",
        "DATA/BTC",
        "BCH/BTC",
        "AKRO/BTC",
        "IRIS/BTC",
        "EOS/BTC",
        "SXP/BTC",
        "VET/BTC",
        "TRX/BTC",
        "UNFI/BTC",
        "FIO/BTC",
        "ATOM/BTC",
        "AXS/BTC",
        "GLM/BTC",
        "BOT/BTC",
        "OMG/BTC",
        "ICX/BTC",
        "STRAX/BTC",
        "BNB/BTC",
        "XMR/BTC",
        "ALGO/BTC",
        "THETA/BTC",
        "AAVE/BTC",
        "COTI/BTC",
        "BAT/BTC",
        "LTC/BTC",
        "HBAR/BTC",
        "SOL/BTC",
        "FET/BTC",
        "FIL/BTC",
        "XTZ/BTC",
        "IOTA/BTC",
        "MATIC/BTC",
        "LTO/BTC",
        "SRM/BTC",
        "TFUEL/BTC",
        "SYS/BTC",
        "ZIL/BTC",
        "SAND/BTC",
        "ROSE/BTC"
      };

      return Task.FromResult((IEnumerable<string>)response);
    }
  }
}