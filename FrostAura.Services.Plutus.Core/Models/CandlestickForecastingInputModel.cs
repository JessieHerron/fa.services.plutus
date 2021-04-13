using FrostAura.Services.Plutus.Shared.Models;
using Microsoft.ML.Data;
using System.Diagnostics;

namespace FrostAura.Services.Plutus.Core.Models
{
  /// <summary>
  /// Model to represent input information.
  /// </summary>
  [DebuggerDisplay("Delta: {PriceDeltaFromPreviousCandle}, Direction: {ForecastDirection}, Open: {Open}, Close: {Close}")]
  public class CandlestickForecastingInputModel : Candlestick
  {
    /// <summary>
    /// The delta between price of the previos candlestick's open price and this candlestick's close price as a percentage.
    /// </summary>
    [LoadColumn(0)]
    public float PriceDeltaFromPreviousCandle { get; set; }
    /// <summary>
    /// The relative strength index of the asset.
    /// </summary>
    [LoadColumn(1)]
    public float Rsi { get; set; }
    /// <summary>
    /// The upper bolinger bands value of the asset.
    /// </summary>
    [LoadColumn(2)]
    public float BBUpper { get; set; }
    /// <summary>
    /// The mid bolinger bands value of the asset.
    /// </summary>
    [LoadColumn(3)]
    public float BBMid { get; set; }
    /// <summary>
    /// The lower bolinger bands value of the asset.
    /// </summary>
    [LoadColumn(4)]
    public float BBLower { get; set; }
    /// <summary>
    /// The stochastic slow K value of the asset.
    /// </summary>
    [LoadColumn(5)]
    public float StochSlowK { get; set; }
    /// <summary>
    /// The stochastic slow D value of the asset.
    /// </summary>
    [LoadColumn(6)]
    public float StochSlowD { get; set; }
  }
}
