using Microsoft.ML.Data;
using System.Diagnostics;

namespace FrostAura.Services.Plutus.Core.Models
{
  /// <summary>
  /// Model to represent output information. 
  /// </summary>
  [DebuggerDisplay("PriceDeltaFromPreviousCandle: {PriceDeltaFromPreviousCandle}")]
  public class CandlestickForecastingOutputModel
  {
    /// <summary>
    /// The delta between price of the previos candlestick's open price and this candlestick's close price as a percentage.
    /// </summary>
    [ColumnName("Score")]
    public float PriceDeltaFromPreviousCandle { get; set; }
  }
}
