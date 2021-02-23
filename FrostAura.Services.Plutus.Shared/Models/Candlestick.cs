using System;
using System.Diagnostics;

namespace FrostAura.Services.Plutus.Shared.Models
{
  /// <summary>
  /// Model representing a candlestick.
  /// </summary>
  [DebuggerDisplay("Open: {Open}, Close: {Close}, High: {High}, Low: {Low}, Time: {CloseTime}")]
  public class Candlestick
  {
    /// <summary>
    /// The time this candlestick opened.
    /// </summary>
    public DateTime OpenTime { get; set; }
    /// <summary>
    /// The price at which this candlestick opened.
    /// </summary>
    public decimal Open { get; set; }
    /// <summary>
    /// The highest price in this candlestick.
    /// </summary>
    public decimal High { get; set; }
    /// <summary>
    /// The lowest price in this candlestick.
    /// </summary>
    public decimal Low { get; set; }
    /// <summary>
    /// The price at which this candlestick closed.
    /// </summary>
    public decimal Close { get; set; }
    /// <summary>
    /// The volume traded during this candlestick.
    /// </summary>
    public decimal Volume { get; set; }
    /// <summary>
    /// The close time of this candlestick
    /// </summary>
    public DateTime CloseTime { get; set; }
    /// <summary>
    /// The amount of trades in this candlestick
    /// </summary>
    public int TradeCount { get; set; }
  }
}
